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
using System.IO;
using dnlib.IO;

namespace NETReactorSlayer.Core.Helper
{
    public static class MethodBodyParser
    {
        public static MethodBodyHeader
            ParseMethodBody(ref DataReader reader, out byte[] code, out byte[] extraSections)
        {
            try { return ParseMethodBody2(ref reader, out code, out extraSections); }
            catch (Exception ex) when (ex is IOException or ArgumentException) { throw new InvalidMethodBody(); }
        }

        private static MethodBodyHeader ParseMethodBody2(
            ref DataReader reader, out byte[] code, out byte[] extraSections)
        {
            var mbHeader = new MethodBodyHeader();

            uint codeOffset;
            var b = Peek(ref reader);
            if ((b & 3) == 2)
            {
                mbHeader.Flags = 2;
                mbHeader.MaxStack = 8;
                mbHeader.CodeSize = (uint)(reader.ReadByte() >> 2);
                mbHeader.LocalVarSigTok = 0;
                codeOffset = 1;
            }
            else if ((b & 7) == 3)
            {
                mbHeader.Flags = reader.ReadUInt16();
                codeOffset = (uint)(4 * (mbHeader.Flags >> 12));
                if (codeOffset != 12)
                    throw new InvalidMethodBody();
                mbHeader.MaxStack = reader.ReadUInt16();
                mbHeader.CodeSize = reader.ReadUInt32();
                if (mbHeader.CodeSize > int.MaxValue)
                    throw new InvalidMethodBody();
                mbHeader.LocalVarSigTok = reader.ReadUInt32();
                if (mbHeader.LocalVarSigTok != 0 && mbHeader.LocalVarSigTok >> 24 != 0x11)
                    throw new InvalidMethodBody();
            }
            else
                throw new InvalidMethodBody();

            if (mbHeader.CodeSize + codeOffset > reader.Length)
                throw new InvalidMethodBody();
            code = reader.ReadBytes((int)mbHeader.CodeSize);

            extraSections = (mbHeader.Flags & 8) != 0 ? ReadExtraSections2(ref reader) : null;

            return mbHeader;
        }

        private static void Align(ref DataReader reader, int alignment) =>
            reader.Position = (reader.Position + (uint)alignment - 1) & ~((uint)alignment - 1);

        private static byte[] ReadExtraSections2(ref DataReader reader)
        {
            Align(ref reader, 4);
            var startPos = (int)reader.Position;
            ParseSection(ref reader);
            var size = (int)reader.Position - startPos;
            reader.Position = (uint)startPos;
            return reader.ReadBytes(size);
        }

        private static void ParseSection(ref DataReader reader)
        {
            byte flags;
            do
            {
                Align(ref reader, 4);

                flags = reader.ReadByte();
                if ((flags & 1) == 0)
                    throw new InvalidMethodBody("Not an exception section");
                if ((flags & 0x3E) != 0)
                    throw new InvalidMethodBody("Invalid bits set");

                if ((flags & 0x40) != 0)
                {
                    reader.Position--;
                    var num = (int)(reader.ReadUInt32() >> 8) / 24;
                    reader.Position += (uint)num * 24;
                }
                else
                {
                    var num = reader.ReadByte() / 12;
                    reader.Position += 2 + (uint)num * 12;
                }
            }
            while ((flags & 0x80) != 0);
        }

        private static byte Peek(ref DataReader reader)
        {
            var b = reader.ReadByte();
            reader.Position--;
            return b;
        }
    }
}