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

namespace NETReactorSlayer.Core.Helper
{
    public class QuickLzBase
    {
        protected static uint Read32(byte[] data, int index) => BitConverter.ToUInt32(data, index);

        protected static void Copy(byte[] src, int srcIndex, byte[] dst, int dstIndex, int size)
        {
            for (var i = 0; i < size; i++)
                dst[dstIndex++] = src[srcIndex++];
        }

        public static void Decompress(byte[] inData, int inIndex, byte[] outData)
        {
            var decompressedLength = outData.Length;
            var outIndex = 0;
            uint val1 = 1;

            while (true)
            {
                if (val1 == 1)
                {
                    val1 = Read32(inData, inIndex);
                    inIndex += 4;
                }

                var val2 = Read32(inData, inIndex);
                if ((val1 & 1) == 1)
                {
                    val1 >>= 1;
                    uint count;
                    if ((val2 & 3) == 0)
                    {
                        count = (val2 & 0xFF) >> 2;
                        Copy(outData, (int)(outIndex - count), outData, outIndex, 3);
                        outIndex += 3;
                        inIndex++;
                    } else if ((val2 & 2) == 0)
                    {
                        count = (val2 & 0xFFFF) >> 2;
                        Copy(outData, (int)(outIndex - count), outData, outIndex, 3);
                        outIndex += 3;
                        inIndex += 2;
                    } else
                    {
                        int size;
                        if ((val2 & 1) == 0)
                        {
                            size = (int)((val2 >> 2) & 0x0F) + 3;
                            count = (val2 & 0xFFFF) >> 6;
                            Copy(outData, (int)(outIndex - count), outData, outIndex, size);
                            outIndex += size;
                            inIndex += 2;
                        } else if ((val2 & 4) == 0)
                        {
                            size = (int)((val2 >> 3) & 0x1F) + 3;
                            count = (val2 & 0xFFFFFF) >> 8;
                            Copy(outData, (int)(outIndex - count), outData, outIndex, size);
                            outIndex += size;
                            inIndex += 3;
                        } else if ((val2 & 8) == 0)
                        {
                            count = val2 >> 15;
                            if (count != 0)
                            {
                                size = (int)((val2 >> 4) & 0x07FF) + 3;
                                inIndex += 4;
                            } else
                            {
                                size = (int)Read32(inData, inIndex + 4);
                                count = Read32(inData, inIndex + 8);
                                inIndex += 12;
                            }

                            Copy(outData, (int)(outIndex - count), outData, outIndex, size);
                            outIndex += size;
                        } else
                        {
                            var b = (byte)(val2 >> 16);
                            size = (int)(val2 >> 4) & 0x0FFF;
                            if (size == 0)
                            {
                                size = (int)Read32(inData, inIndex + 3);
                                inIndex += 7;
                            } else
                                inIndex += 3;

                            for (var i = 0; i < size; i++)
                                outData[outIndex++] = b;
                        }
                    }
                } else
                {
                    Copy(inData, inIndex, outData, outIndex, 4);
                    var index = (int)(val1 & 0x0F);
                    outIndex += IndexInc[index];
                    inIndex += IndexInc[index];
                    val1 >>= IndexInc[index];
                    if (outIndex >= decompressedLength - 4)
                        break;
                }
            }

            while (outIndex < decompressedLength)
            {
                if (val1 == 1)
                {
                    inIndex += 4;
                    val1 = 0x80000000;
                }

                outData[outIndex++] = inData[inIndex++];
                val1 >>= 1;
            }
        }

        private static readonly int[] IndexInc = { 4, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0 };
    }
}