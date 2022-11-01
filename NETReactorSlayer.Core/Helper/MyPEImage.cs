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

namespace NETReactorSlayer.Core.Helper
{
    public sealed class MyPeImage : IDisposable
    {
        public MyPeImage(IPEImage peImage) => Initialize(peImage);

        public MyPeImage(byte[] peImageData)
        {
            _ownPeImage = true;
            PeImageData = peImageData;
            Initialize(new PEImage(peImageData));
        }

        private void Initialize(IPEImage peImage)
        {
            PeImage = peImage;
            Reader = peImage.CreateReader();
        }

        public ImageSectionHeader FindSection(RVA rva) =>
            PeImage.ImageSectionHeaders.FirstOrDefault(section =>
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
            dm.mhFlags = mbHeader.Flags;
            dm.mhMaxStack = mbHeader.MaxStack;
            dm.mhCodeSize = dm.code == null ? 0 : (uint)dm.code.Length;
            dm.mhLocalVarSigTok = mbHeader.LocalVarSigTok;
        }

        public uint RvaToOffset(uint rva) => (uint)PeImage.ToFileOffset((RVA)rva);

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

        public void OffsetWrite(uint offset, byte[] data) => Array.Copy(data, 0, PeImageData, offset, data.Length);

        private static bool Intersect(uint offset1, uint length1, uint offset2, uint length2) =>
            !(offset1 + length1 <= offset2 || offset2 + length2 <= offset1);

        private static bool Intersect(uint offset, uint length, IFileSection location) => Intersect(offset, length,
            (uint)location.StartOffset, location.EndOffset - location.StartOffset);

        public bool DotNetSafeWriteOffset(uint offset, byte[] data)
        {
            if (Metadata != null)
            {
                var length = (uint)data.Length;

                if (!IsInside(_dotNetSection, offset, length))
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
            DotNetSafeWriteOffset((uint)PeImage.ToFileOffset((RVA)rva), data);

        public void Dispose()
        {
            if (_ownPeImage)
            {
                _metadata?.Dispose();
                PeImage?.Dispose();
            }

            _metadata = null;
            PeImage = null;
            Reader = default;
        }

        private bool _dnFileInitialized;
        private ImageSectionHeader _dotNetSection;
        private Metadata _metadata;

        private readonly bool _ownPeImage;
        public byte[] PeImageData;

        public DataReader Reader;

        public Metadata Metadata
        {
            get
            {
                if (_dnFileInitialized)
                    return _metadata;
                _dnFileInitialized = true;

                var dotNetDir = PeImage.ImageNTHeaders.OptionalHeader.DataDirectories[14];
                if (dotNetDir.VirtualAddress == 0 || dotNetDir.Size < 0x48) return _metadata;
                _metadata = MetadataFactory.CreateMetadata(PeImage, false);
                _dotNetSection = FindSection(dotNetDir.VirtualAddress);

                return _metadata;
            }
        }

        public IPEImage PeImage { get; private set; }
    }
}