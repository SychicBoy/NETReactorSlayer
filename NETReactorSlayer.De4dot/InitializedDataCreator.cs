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
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using de4dot.blocks;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace NETReactorSlayer.De4dot;

public class InitializedDataCreator
{
    public InitializedDataCreator(ModuleDef module) => _module = module;

    private MemberRef CreateInitializeArrayMethod()
    {
        if (_initializeArrayMethod == null)
        {
            var runtimeHelpersType = DotNetUtils.FindOrCreateTypeRef(_module, _module.CorLibTypes.AssemblyRef,
                "System.Runtime.CompilerServices", "RuntimeHelpers", false);
            var systemArrayType =
                DotNetUtils.FindOrCreateTypeRef(_module, _module.CorLibTypes.AssemblyRef, "System", "Array", false);
            var runtimeFieldHandleType = DotNetUtils.FindOrCreateTypeRef(_module, _module.CorLibTypes.AssemblyRef,
                "System", "RuntimeFieldHandle", true);
            var methodSig =
                MethodSig.CreateStatic(_module.CorLibTypes.Void, systemArrayType, runtimeFieldHandleType);
            _initializeArrayMethod = _module.UpdateRowId(new MemberRefUser(_module, "InitializeArray", methodSig,
                runtimeHelpersType.TypeDefOrRef));
        }

        return _initializeArrayMethod;
    }

    public void AddInitializeArrayCode(Block block, int start, int numToRemove, ITypeDefOrRef elementType,
        byte[] data)
    {
        var index = start;
        block.Replace(index++, numToRemove,
            Instruction.CreateLdcI4(data.Length / elementType.ToTypeSig().ElementType.GetPrimitiveSize()));
        block.Insert(index++, OpCodes.Newarr.ToInstruction(elementType));
        block.Insert(index++, OpCodes.Dup.ToInstruction());
        block.Insert(index++, OpCodes.Ldtoken.ToInstruction(Create(data)));
        block.Insert(index, OpCodes.Call.ToInstruction((IMethod)InitializeArrayMethod));
    }

    private void CreateOurType()
    {
        if (_ourType != null)
            return;

        _ourType = new TypeDefUser("", $"<PrivateImplementationDetails>{GetModuleId()}",
            _module.CorLibTypes.Object.TypeDefOrRef);
        _ourType.Attributes = TypeAttributes.NotPublic | TypeAttributes.AutoLayout |
                              TypeAttributes.Class | TypeAttributes.AnsiClass;
        _module.UpdateRowId(_ourType);
        _module.Types.Add(_ourType);
    }

    private object GetModuleId()
    {
        var memoryStream = new MemoryStream();
        var writer = new BinaryWriter(memoryStream);
        if (_module.Assembly != null)
            writer.Write(_module.Assembly.FullName);
        writer.Write((_module.Mvid ?? Guid.Empty).ToByteArray());
        var hash = new SHA1Managed().ComputeHash(memoryStream.GetBuffer());
        var guid = new Guid(BitConverter.ToInt32(hash, 0),
            BitConverter.ToInt16(hash, 4),
            BitConverter.ToInt16(hash, 6),
            hash[8], hash[9], hash[10], hash[11],
            hash[12], hash[13], hash[14], hash[15]);
        return guid.ToString("B");
    }

    private TypeDef GetArrayType(long size)
    {
        CreateOurType();

        if (_sizeToArrayType.TryGetValue(size, out var arrayType))
            return arrayType;

        if (_valueType == null)
            _valueType = DotNetUtils.FindOrCreateTypeRef(_module, _module.CorLibTypes.AssemblyRef, "System",
                "ValueType",
                false);
        arrayType = new TypeDefUser("", $"__StaticArrayInitTypeSize={size}", _valueType.TypeDefOrRef);
        _module.UpdateRowId(arrayType);
        arrayType.Attributes = TypeAttributes.NestedPrivate | TypeAttributes.ExplicitLayout |
                               TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.AnsiClass;
        _ourType.NestedTypes.Add(arrayType);
        _sizeToArrayType[size] = arrayType;
        arrayType.ClassLayout = new ClassLayoutUser(1, (uint)size);
        return arrayType;
    }

    public FieldDef Create(byte[] data)
    {
        var arrayType = GetArrayType(data.LongLength);
        var fieldSig = new FieldSig(new ValueTypeSig(arrayType));
        var attrs = FieldAttributes.Assembly | FieldAttributes.Static;
        var field = new FieldDefUser($"field_{_unique++}", fieldSig, attrs);
        _module.UpdateRowId(field);
        field.HasFieldRVA = true;
        _ourType.Fields.Add(field);
        var iv = new byte[data.Length];
        Array.Copy(data, iv, data.Length);
        field.InitialValue = iv;
        return field;
    }

    private readonly ModuleDef _module;
    private readonly Dictionary<long, TypeDef> _sizeToArrayType = new Dictionary<long, TypeDef>();

    private MemberRef _initializeArrayMethod;
    private TypeDef _ourType;
    private int _unique;
    private TypeDefOrRefSig _valueType;

    public MemberRef InitializeArrayMethod => CreateInitializeArrayMethod();
}