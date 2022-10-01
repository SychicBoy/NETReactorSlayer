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

using System.Text.RegularExpressions;
using dnlib.DotNet;

namespace NETReactorSlayer.De4dot;

internal sealed class Deobfuscator : DeobfuscatorBase
{
    public Deobfuscator(ModuleDefMD module, Options options)
        : base(module, options)
    {
        if (options.RemoveNamespaces)
            RenamingOptions |= RenamingOptions.RemoveNamespaceIfOneType;
        else
            RenamingOptions &= ~RenamingOptions.RemoveNamespaceIfOneType;
        if (options.RenameShort)
            options.ValidNameRegex.Regexes.Insert(0, new NameRegex(DeobfuscatorInfo.ShortNameRegex));
    }

    private bool CheckValidName(string name, Regex regex)
    {
        if (IsRandomName.IsMatch(name))
            return false;
        if (regex.IsMatch(name))
        {
            if (RandomNameChecker.IsRandom(name))
                return false;
            if (!RandomNameChecker.IsNonRandom(name))
                return false;
        }

        return CheckValidName(name);
    }

    public override bool IsValidNamespaceName(string ns)
    {
        if (ns == null)
            return false;
        if (ns.Contains("."))
            return base.IsValidNamespaceName(ns);
        return CheckValidName(ns, IsRandomNameTypes);
    }

    public override bool IsValidTypeName(string name) => name != null && CheckValidName(name, IsRandomNameTypes);

    public override bool IsValidMethodName(string name) => name != null && CheckValidName(name, IsRandomNameMembers);

    public override bool IsValidPropertyName(string name) => name != null && CheckValidName(name, IsRandomNameMembers);

    public override bool IsValidEventName(string name) => name != null && CheckValidName(name, IsRandomNameMembers);

    public override bool IsValidFieldName(string name) => name != null && CheckValidName(name, IsRandomNameMembers);

    public override bool IsValidGenericParamName(string name) =>
        name != null && CheckValidName(name, IsRandomNameMembers);

    public override bool IsValidMethodArgName(string name) => name != null && CheckValidName(name, IsRandomNameMembers);

    public override bool IsValidMethodReturnArgName(string name) =>
        string.IsNullOrEmpty(name) || CheckValidName(name, IsRandomNameMembers);

    public override bool IsValidResourceKeyName(string name) =>
        name != null && CheckValidName(name, IsRandomNameMembers);

    private static readonly Regex IsRandomName = new(@"^[A-Z]{30,40}$");
    private static readonly Regex IsRandomNameMembers = new(@"^(?:[a-zA-Z0-9]{9,11}|[a-zA-Z0-9]{18,20})$");
    private static readonly Regex IsRandomNameTypes = new(@"^[a-zA-Z0-9]{18,20}(?:`\d+)?$");

    public override string Name => DeobfuscatorInfo.TheName;

    internal class Options : OptionsBase
    {
        public bool RemoveNamespaces { get; set; }
        public bool RenameShort { get; set; }
        public bool RestoreTypes { get; set; }
    }
}