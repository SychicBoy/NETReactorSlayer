/*
    Copyright (C) 2021 CodeStrikers.org
    This file is part of NETReactorSlayer.
    NETReactorSlayer is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    NETReactorSlayer is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with NETReactorSlayer.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using de4dot.blocks;
using dnlib.DotNet;
using NETReactorSlayer.Core.Helper;

namespace NETReactorSlayer.Core.Deobfuscators;

internal class ResourceResolver : IStage
{
    public void Execute()
    {
        try
        {
            if (!Find())
            {
                Logger.Warn("Couldn't find any encrypted resource.");
                return;
            }

            DeobUtils.DecryptAndAddResources(Context.Module,
                () => Decompress(_encryptedResource.Decrypt()));

            foreach (var m in _methodToRemove)
                Cleaner.AddCallToBeRemoved(m.ResolveMethodDef());
            Cleaner.AddCallToBeRemoved(_encryptedResource.DecrypterMethod);
            Cleaner.AddTypeToBeRemoved(_encryptedResource.DecrypterMethod.DeclaringType);
            Cleaner.AddResourceToBeRemoved(_encryptedResource.EmbeddedResource);
            Logger.Done("Assembly resources decrypted");
        }
        catch
        {
            Logger.Error("An unexpected error occurred during decrypting resources.");
        }

        _encryptedResource?.Dispose();
    }

    #region Private Methods

    private bool Find()
    {
        foreach (var type in Context.Module.GetTypes())
        {
            if (type.BaseType is not { FullName: "System.Object" })
                continue;
            if (!CheckFields(type.Fields))
                continue;
            foreach (var method in type.Methods)
            {
                if (!method.IsStatic || !method.HasBody)
                    continue;
                if (!DotNetUtils.IsMethod(method, "System.Reflection.Assembly",
                        "(System.Object,System.ResolveEventArgs)") &&
                    !DotNetUtils.IsMethod(method, "System.Reflection.Assembly", "(System.Object,System.Object)"))
                    continue;
                if (method.Body.ExceptionHandlers.Count != 0)
                    continue;
                var decrypterMethod = GetDecrypterMethod(method, Array.Empty<string>(), true) ??
                                      GetDecrypterMethod(method, Array.Empty<string>(), false);
                if (decrypterMethod == null)
                    continue;

                _encryptedResource = new EncryptedResource(decrypterMethod);
                if (_encryptedResource.EmbeddedResource == null)
                {
                    _encryptedResource.Dispose();
                    continue;
                }

                _methodToRemove.AddRange(type.Methods);
                return true;
            }
        }

        return false;
    }

    private bool CheckFields(ICollection<FieldDef> fields)
    {
        if (fields.Count != 3 && fields.Count != 4)
            return false;

        var numBools = fields.Count == 3 ? 1 : 2;
        var fieldTypes = new FieldTypes(fields);
        if (fieldTypes.Count("System.Boolean") != numBools)
            return false;
        if (fieldTypes.Count("System.Object") == 2)
            return true;
        if (fieldTypes.Count("System.String[]") != 1)
            return false;
        return fieldTypes.Count("System.Reflection.Assembly") == 1 || fieldTypes.Count("System.Object") == 1;
    }

    private MethodDef GetDecrypterMethod(MethodDef method, string[] additionalTypes, bool checkResource)
    {
        if (EncryptedResource.IsKnownDecrypter(method, additionalTypes, checkResource))
            return method;

        return DotNetUtils.GetCalledMethods(Context.Module, method)
            .Where(calledMethod => DotNetUtils.IsMethod(calledMethod, "System.Void", "()"))
            .FirstOrDefault(calledMethod =>
                EncryptedResource.IsKnownDecrypter(calledMethod, additionalTypes, checkResource));
    }

    private byte[] Decompress(byte[] bytes)
    {
        try
        {
            return QuickLZ.Decompress(bytes);
        }
        catch
        {
            try
            {
                return DeobUtils.Inflate(bytes, true);
            }
            catch
            {
                return null;
            }
        }
    }

    #endregion

    #region Fields

    private EncryptedResource _encryptedResource;
    private readonly List<MethodDef> _methodToRemove = new();

    #endregion
}