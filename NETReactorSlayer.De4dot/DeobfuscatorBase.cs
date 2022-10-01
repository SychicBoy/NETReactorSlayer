/*
    Copyright (C) 2011-2015 de4dot@gmail.com

    This file is part of de4dot.

    de4dot is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    de4dot is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with de4dot.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Linq;
using de4dot.blocks;
using dnlib.DotNet;
using dnlib.DotNet.Writer;

namespace NETReactorSlayer.De4dot;

public abstract class DeobfuscatorBase : IDeobfuscator, IModuleWriterListener
{
    protected DeobfuscatorBase(ModuleDefMD module, OptionsBase optionsBase)
    {
        _optionsBase = optionsBase;
        Module = module;
        InitializedDataCreator = new InitializedDataCreator(module);
    }

    protected virtual bool CheckValidName(string name) => _optionsBase.ValidNameRegex.IsMatch(name);

    private static bool IsTypeWithInvalidBaseType(TypeDef moduleType, TypeDef type) =>
        type.BaseType == null && !type.IsInterface && type != moduleType;

    public override string ToString() => Name;

    protected virtual void Dispose(bool disposing)
    {
    }

    public virtual bool IsValidNamespaceName(string ns) => ns != null && ns.Split('.').All(CheckValidName);

    public virtual bool IsValidTypeName(string name) => name != null && CheckValidName(name);

    public virtual bool IsValidMethodName(string name) => name != null && CheckValidName(name);

    public virtual bool IsValidPropertyName(string name) => name != null && CheckValidName(name);

    public virtual bool IsValidEventName(string name) => name != null && CheckValidName(name);

    public virtual bool IsValidFieldName(string name) => name != null && CheckValidName(name);

    public virtual bool IsValidGenericParamName(string name) => name != null && CheckValidName(name);

    public virtual bool IsValidMethodArgName(string name) => name != null && CheckValidName(name);

    public virtual bool IsValidMethodReturnArgName(string name) => string.IsNullOrEmpty(name) || CheckValidName(name);

    public virtual bool IsValidResourceKeyName(string name) => name != null && CheckValidName(name);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void RestoreBaseType()
    {
        var moduleType = DotNetUtils.GetModuleType(Module);
        foreach (var type in Module.GetTypes())
        {
            if (!IsTypeWithInvalidBaseType(moduleType, type))
                continue;
            var corSig = Module.CorLibTypes.GetCorLibTypeSig(type);
            if (corSig is { ElementType: ElementType.Object })
                continue;
            type.BaseType = Module.CorLibTypes.Object.TypeDefOrRef;
        }
    }

    public virtual void OnWriterEvent(ModuleWriterBase writer, ModuleWriterEvent evt)
    {
    }

    private readonly OptionsBase _optionsBase;
    protected InitializedDataCreator InitializedDataCreator;
    protected ModuleDefMD Module;
    public const string DefaultAsianValidNameRegex = @"^[\u2E80-\u9FFFa-zA-Z_<{$][\u2E80-\u9FFFa-zA-Z_0-9<>{}$.`-]*$";
    public const string DefaultValidNameRegex = @"^[a-zA-Z_<{$][a-zA-Z_0-9<>{}$.`-]*$";
    public abstract string Name { get; }
    public virtual RenamingOptions RenamingOptions { get; set; }

    public IDeobfuscatorOptions TheOptions => _optionsBase;

    public class OptionsBase : IDeobfuscatorOptions
    {
        public OptionsBase() => RenameResourcesInCode = true;

        public bool RenameResourcesInCode { get; set; }

        public NameRegexes ValidNameRegex { get; set; }
    }
}