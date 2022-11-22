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

namespace NETReactorSlayer.Core.Helper {
    public class QuickLz : QuickLzBase {
        public static byte[] Decompress(byte[] inData) => Decompress(inData, DefaultQclzSig);

        public static byte[] Decompress(byte[] inData, int sig) {
            BitConverter.ToInt32(inData, 4);
            var compressedLength = BitConverter.ToInt32(inData, 8);
            var decompressedLength = BitConverter.ToInt32(inData, 12);
            var isDataCompressed = BitConverter.ToInt32(inData, 16) == 1;
            const int headerLength = 32;
            if (BitConverter.ToInt32(inData, 0) != sig || BitConverter.ToInt32(inData, compressedLength - 4) != sig)
                throw new ApplicationException("No QCLZ sig");

            var outData = new byte[decompressedLength];

            if (!isDataCompressed) {
                Copy(inData, headerLength, outData, 0, decompressedLength);
                return outData;
            }

            Decompress(inData, headerLength, outData);
            return outData;
        }

        private const int DefaultQclzSig = 0x5A4C4351;
    }
}