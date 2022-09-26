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
using dnlib.DotNet.MD;
using dnlib.IO;
using dnlib.PE;

namespace NETReactorSlayer.Core.Helper;

public sealed class MyPEImage : IDisposable
{
    public MyPEImage(IPEImage peImage) => Initialize(peImage);

    public MyPEImage(byte[] peImageData)
    {
        ownPeImage = true;
        this.peImageData = peImageData;
        Initialize(new PEImage(peImageData));
    }

    private void Initialize(IPEImage peImage)
    {
        PEImage = peImage;
        Reader = peImage.CreateReader();
    }

    public ImageSectionHeader FindSection(RVA rva) =>
        PEImage.ImageSectionHeaders.FirstOrDefault(section =>
            section.VirtualAddress <= rva &&
            rva < section.VirtualAddress + Math.Max(section.VirtualSize, section.SizeOfRawData));

    public void ReadMethodTableRowTo(DumpedMethod dm, uint rid)
    {
        dm.token = 0x06000000 + rid;
        if (!Metadata.TablesStream.TryReadMethodRow(rid, out var row))
            throw new ArgumentException("Invalid Method rid");
        dm.mdRVA = row.RVA;
        dm.mdImplFlags = row.ImplFlags;
        dm.mdFlags = row.Flags;
        dm.mdName = row.Name;
        dm.mdSignature = row.Signature;
        dm.mdParamList = row.ParamList;
    }

    public void UpdateMethodHeaderInfo(DumpedMethod dm, MethodBodyHeader mbHeader)
    {
        dm.mhFlags = mbHeader.flags;
        dm.mhMaxStack = mbHeader.maxStack;
        dm.mhCodeSize = dm.code == null ? 0 : (uint)dm.code.Length;
        dm.mhLocalVarSigTok = mbHeader.localVarSigTok;
    }

    public uint RvaToOffset(uint rva) => (uint)PEImage.ToFileOffset((RVA)rva);

    private static bool IsInside(ImageSectionHeader section, uint offset, uint length) =>
        offset >= section.PointerToRawData &&
        offset + length <= section.PointerToRawData + section.SizeOfRawData;

    public byte ReadByte(uint rva) => OffsetReadByte(RvaToOffset(rva));

    public byte[] ReadBytes(uint rva, int size) => OffsetReadBytes(RvaToOffset(rva), size);

    public int ReadInt32(uint rva) => (int)OffsetReadUInt32(RvaToOffset(rva));

    public uint OffsetReadUInt32(uint offset)
    {
        Reader.Position = offset;
        return Reader.ReadUInt32();
    }

    public byte OffsetReadByte(uint offset)
    {
        Reader.Position = offset;
        return Reader.ReadByte();
    }

    public byte[] OffsetReadBytes(uint offset, int size)
    {
        Reader.Position = offset;
        return Reader.ReadBytes(size);
    }

    public void OffsetWrite(uint offset, byte[] data) => Array.Copy(data, 0, peImageData, offset, data.Length);

    private static bool Intersect(uint offset1, uint length1, uint offset2, uint length2) =>
        !(offset1 + length1 <= offset2 || offset2 + length2 <= offset1);

    private static bool Intersect(uint offset, uint length, IFileSection location) => Intersect(offset, length,
        (uint)location.StartOffset, location.EndOffset - location.StartOffset);

    public bool DotNetSafeWriteOffset(uint offset, byte[] data)
    {
        if (Metadata != null)
        {
            var length = (uint)data.Length;

            if (!IsInside(dotNetSection, offset, length))
                return false;
            if (Intersect(offset, length, Metadata.ImageCor20Header))
                return false;
            if (Intersect(offset, length, Metadata.MetadataHeader))
                return false;
        }

        OffsetWrite(offset, data);
        return true;
    }

    public bool DotNetSafeWrite(uint rva, byte[] data) =>
        DotNetSafeWriteOffset((uint)PEImage.ToFileOffset((RVA)rva), data);

    public void Dispose()
    {
        if (ownPeImage)
        {
            metadata?.Dispose();
            PEImage?.Dispose();
        }

        metadata = null;
        PEImage = null;
        Reader = default;
    }

    private readonly bool ownPeImage;

    private bool dnFileInitialized;
    private ImageSectionHeader dotNetSection;
    private Metadata metadata;
    public byte[] peImageData;

    public DataReader Reader;

    public Metadata Metadata
    {
        get
        {
            if (dnFileInitialized)
                return metadata;
            dnFileInitialized = true;

            var dotNetDir = PEImage.ImageNTHeaders.OptionalHeader.DataDirectories[14];
            if (dotNetDir.VirtualAddress != 0 && dotNetDir.Size >= 0x48)
            {
                metadata = MetadataFactory.CreateMetadata(PEImage, false);
                dotNetSection = FindSection(dotNetDir.VirtualAddress);
            }

            return metadata;
        }
    }

    public IPEImage PEImage { get; private set; }
}