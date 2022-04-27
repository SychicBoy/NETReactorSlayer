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
using dnlib.DotNet;
using dnlib.PE;
using ICSharpCode.SharpZipLib.Zip.Compression;
using NETReactorSlayer.Core.Helper.De4dot;

namespace NETReactorSlayer.Core.Deobfuscators;

internal class NativeUnpacker
{
    private const int LoaderHeaderSizeV45 = 14;

    private readonly uint[] _baseOffsets =
    {
        0x1C00,
        0x1900,
        0x1B60,
        0x700
    };

    private readonly short[] _decryptMethodPattern =
    {
        0x83, 0xEC, 0x38,
        0x53,
        0xB0, -1,
        0x88, 0x44, 0x24, 0x2B,
        0x88, 0x44, 0x24, 0x2F,
        0xB0, -1,
        0x88, 0x44, 0x24, 0x30,
        0x88, 0x44, 0x24, 0x31,
        0x88, 0x44, 0x24, 0x33,
        0x55,
        0x56
    };

    private readonly MyPEImage _peImage;

    private readonly short[] _startMethodNet1XPattern =
    {
        0x55,
        0x8B, 0xEC,
        0xB9, 0x14, 0x00, 0x00, 0x00,
        0x6A, 0x00,
        0x6A, 0x00,
        0x49,
        0x75, 0xF9,
        0x53,
        0x56,
        0x57,
        0xB8, -1, -1, -1, -1,
        0xE8, -1, -1, -1, -1
    };

    private bool _isNet1X;

    public NativeUnpacker(IPEImage peImage) => _peImage = new MyPEImage(peImage);

    public byte[] Unpack()
    {
        if (_peImage.PEImage.Win32Resources == null)
            return null;
        var dataEntry = _peImage.PEImage.Win32Resources.Find(10, "__", 0);
        if (dataEntry == null)
            return null;

        var encryptedData = dataEntry.CreateReader().ToArray();

        var keyData = GetKeyData();
        if (keyData == null)
            return null;
        Decrypt(keyData, encryptedData, 0, encryptedData.Length);

        byte[] inflatedData;
        if (_isNet1X)
        {
            inflatedData = DeobUtils.Inflate(encryptedData, false);
        }
        else
        {
            var inflatedSize = BitConverter.ToInt32(encryptedData, 0);
            inflatedData = new byte[inflatedSize];
            var inflater = new Inflater(false);
            inflater.SetInput(encryptedData, 4, encryptedData.Length - 4);
            var count = inflater.Inflate(inflatedData);
            if (count != inflatedSize)
                return null;
        }

        if (BitConverter.ToInt16(inflatedData, 0) == 0x5A4D)
            return inflatedData;
        if (BitConverter.ToInt16(inflatedData, LoaderHeaderSizeV45) == 0x5A4D)
            return UnpackLoader(inflatedData);

        return null;
    }

    private byte[] UnpackLoader(byte[] loaderData)
    {
        var loaderBytes = new byte[loaderData.Length - LoaderHeaderSizeV45];
        Array.Copy(loaderData, LoaderHeaderSizeV45, loaderBytes, 0, loaderBytes.Length);
        try
        {
            using var asmLoader = ModuleDefMD.Load(loaderBytes);
            return asmLoader.Resources.Count == 0
                ? null
                : (asmLoader.Resources[0] as EmbeddedResource)?.CreateReader().ToArray();
        } catch
        {
            return null;
        }
    }

    private byte[] GetKeyData()
    {
        _isNet1X = false;
        foreach (var item in _baseOffsets)
        {
            var code = _peImage.OffsetReadBytes(item, _decryptMethodPattern.Length);
            if (DeobUtils.IsCode(_decryptMethodPattern, code))
                return GetKeyData(item);
        }

        var net1XCode = _peImage.OffsetReadBytes(0x207E0, _startMethodNet1XPattern.Length);
        if (DeobUtils.IsCode(_startMethodNet1XPattern, net1XCode))
        {
            _isNet1X = true;
            return new byte[] {0x34, 0x38, 0x63, 0x65, 0x7A, 0x35};
        }

        return null;
    }

    private byte[] GetKeyData(uint baseOffset) =>
        new[]
        {
            _peImage.OffsetReadByte(baseOffset + 5),
            _peImage.OffsetReadByte(baseOffset + 0xF),
            _peImage.OffsetReadByte(baseOffset + 0x58),
            _peImage.OffsetReadByte(baseOffset + 0x6D),
            _peImage.OffsetReadByte(baseOffset + 0x98),
            _peImage.OffsetReadByte(baseOffset + 0xA6)
        };

    private void Decrypt(byte[] keyData, byte[] data, int offset, int count)
    {
        var transform = new byte[256, 256];
        byte kb = 0;
        var keyInit = new byte[]
        {
            0x78, 0x61, 0x32, keyData[0], keyData[2],
            0x62, keyData[3], keyData[0], keyData[1], keyData[1],
            0x66, keyData[1], keyData[5], 0x33, keyData[2],
            keyData[4], 0x74, 0x32, keyData[3], keyData[2]
        };
        var key = new byte[32];
        for (var i = 0; i < 32; i++)
        {
            key[i] = (byte) (i + keyInit[i % keyInit.Length] * keyInit[((i + 0x0B) | 0x1F) % keyInit.Length]);
            kb += key[i];
        }

        var transformTemp = new ushort[256, 256];
        for (var i = 0; i < 256; i++)
        for (var j = 0; j < 256; j++)
            transformTemp[i, j] = 0x400;
        var counter = 0x0B;
        byte newByte = 0;
        var ki = 0;
        for (var i = 0; i < 256; i++)
        {
            while (true)
            {
                for (var j = key.Length - 1; j >= ki; j--)
                    newByte += (byte) (key[j] + counter);
                var done = true;
                ki = (ki + 1) % key.Length;
                for (var k = 0; k <= i; k++)
                    if (newByte == transformTemp[k, 0])
                    {
                        done = false;
                        break;
                    }

                if (done)
                    break;
                counter++;
            }

            transformTemp[i, 0] = newByte;
        }

        counter = ki = 0;
        for (var i = 1; i < 256; i++)
        {
            ki++;
            int i1;
            do
            {
                counter++;
                i1 = 1 + (key[(i + 37 + counter) % key.Length] + counter + kb) % 255;
            } while (transformTemp[0, i1] != 0x400);

            for (var i0 = 0; i0 < 256; i0++)
                transformTemp[i0, i1] = transformTemp[(i0 + ki) % 256, 0];
        }

        for (var i = 0; i < 256; i++)
        for (var j = 0; j < 256; j++)
            transform[(byte) transformTemp[i, j], j] = (byte) i;

        for (var i = 0; i < count; i += 1024, offset += 1024)
        {
            var blockLen = Math.Min(1024, count - i);

            if (blockLen == 1)
            {
                data[offset] = transform[data[offset], kb];
                continue;
            }

            for (var j = 0; j < blockLen - 1; j++)
                data[offset + j] = transform[data[offset + j], data[offset + j + 1]];
            data[offset + blockLen - 1] = transform[data[offset + blockLen - 1], kb ^ 0x55];

            for (var j = blockLen - 1; j > 0; j--)
                data[offset + j] = transform[data[offset + j], data[offset + j - 1]];
            data[offset] = transform[data[offset], kb];
        }
    }
}