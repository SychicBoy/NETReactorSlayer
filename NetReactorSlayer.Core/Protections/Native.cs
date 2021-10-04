using dnlib.DotNet;
using dnlib.PE;
using ICSharpCode.SharpZipLib.Zip.Compression;
using NetReactorSlayer.Core.Utils.de4dot;
using System;

namespace NETReactorSlayer.Core.Protections
{
    class Native
    {
        readonly MyPEImage peImage;
        bool isNet1x;
        const int loaderHeaderSizeV45 = 14;

        public Native(IPEImage peImage)
        {
            this.peImage = new MyPEImage(peImage);
        }
        public byte[] Unpack()
        {
            if (peImage.PEImage.Win32Resources == null)
                return null;
            var dataEntry = peImage.PEImage.Win32Resources.Find(10, "__", 0);
            if (dataEntry == null)
                return null;

            var encryptedData = dataEntry.CreateReader().ToArray();

            var keyData = GetKeyData();
            if (keyData == null)
                return null;
            Decrypt(keyData, encryptedData, 0, encryptedData.Length);

            byte[] inflatedData;
            if (isNet1x)
                inflatedData = DeobUtils.Inflate(encryptedData, false);
            else
            {
                int inflatedSize = BitConverter.ToInt32(encryptedData, 0);
                inflatedData = new byte[inflatedSize];
                var inflater = new Inflater(false);
                inflater.SetInput(encryptedData, 4, encryptedData.Length - 4);
                int count = inflater.Inflate(inflatedData);
                if (count != inflatedSize)
                    return null;
            }
            if (BitConverter.ToInt16(inflatedData, 0) == 0x5A4D)
                return inflatedData;
            if (BitConverter.ToInt16(inflatedData, loaderHeaderSizeV45) == 0x5A4D)
                return UnpackLoader(inflatedData);

            return null;
        }

        byte[] UnpackLoader(byte[] loaderData)
        {
            var loaderBytes = new byte[loaderData.Length - loaderHeaderSizeV45];
            Array.Copy(loaderData, loaderHeaderSizeV45, loaderBytes, 0, loaderBytes.Length);
            try
            {
                using (var asmLoader = ModuleDefMD.Load(loaderBytes))
                {
                    if (asmLoader.Resources.Count == 0)
                        return null;
                    if (asmLoader.Resources[0] as EmbeddedResource == null)
                        return null;

                    return (asmLoader.Resources[0] as EmbeddedResource).CreateReader().ToArray();
                }
            }
            catch
            {
                return null;
            }
        }

        readonly uint[] baseOffsets = new uint[] {
            0x1C00,
            0x1900,
            0x1B60,
            0x700,
        };
        readonly short[] decryptMethodPattern = new short[] {
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
            0x56,
        };
        readonly short[] startMethodNet1xPattern = new short[] {
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
            0xE8, -1, -1, -1, -1,
        };
        byte[] GetKeyData()
        {
            isNet1x = false;
            for (int i = 0; i < baseOffsets.Length; i++)
            {
                var code = peImage.OffsetReadBytes(baseOffsets[i], decryptMethodPattern.Length);
                if (DeobUtils.IsCode(decryptMethodPattern, code))
                    return GetKeyData(baseOffsets[i]);
            }

            var net1xCode = peImage.OffsetReadBytes(0x207E0, startMethodNet1xPattern.Length);
            if (DeobUtils.IsCode(startMethodNet1xPattern, net1xCode))
            {
                isNet1x = true;
                return new byte[6] { 0x34, 0x38, 0x63, 0x65, 0x7A, 0x35 };
            }

            return null;
        }

        byte[] GetKeyData(uint baseOffset) =>
            new byte[6] {
                peImage.OffsetReadByte(baseOffset + 5),
                peImage.OffsetReadByte(baseOffset + 0xF),
                peImage.OffsetReadByte(baseOffset + 0x58),
                peImage.OffsetReadByte(baseOffset + 0x6D),
                peImage.OffsetReadByte(baseOffset + 0x98),
                peImage.OffsetReadByte(baseOffset + 0xA6),
            };
        void Decrypt(byte[] keyData, byte[] data, int offset, int count)
        {
            byte[,] transform = new byte[256, 256];
            byte kb = 0;
            byte[] key;
            var keyInit = new byte[] {
                0x78, 0x61, 0x32, keyData[0], keyData[2],
                0x62, keyData[3], keyData[0], keyData[1], keyData[1],
                0x66, keyData[1], keyData[5], 0x33, keyData[2],
                keyData[4], 0x74, 0x32, keyData[3], keyData[2],
            };
            key = new byte[32];
            for (int i = 0; i < 32; i++)
            {
                key[i] = (byte)(i + keyInit[i % keyInit.Length] * keyInit[((i + 0x0B) | 0x1F) % keyInit.Length]);
                kb += key[i];
            }

            var transformTemp = new ushort[256, 256];
            for (int i = 0; i < 256; i++)
                for (int j = 0; j < 256; j++)
                    transformTemp[i, j] = 0x400;
            int counter = 0x0B;
            byte newByte = 0;
            int ki = 0;
            for (int i = 0; i < 256; i++)
            {
                while (true)
                {
                    for (int j = key.Length - 1; j >= ki; j--)
                        newByte += (byte)(key[j] + counter);
                    bool done = true;
                    ki = (ki + 1) % key.Length;
                    for (int k = 0; k <= i; k++)
                    {
                        if (newByte == transformTemp[k, 0])
                        {
                            done = false;
                            break;
                        }
                    }
                    if (done)
                        break;
                    counter++;
                }
                transformTemp[i, 0] = newByte;
            }

            counter = ki = 0;
            for (int i = 1; i < 256; i++)
            {
                ki++;
                int i1;
                do
                {
                    counter++;
                    i1 = 1 + (key[(i + 37 + counter) % key.Length] + counter + kb) % 255;
                } while (transformTemp[0, i1] != 0x400);
                for (int i0 = 0; i0 < 256; i0++)
                    transformTemp[i0, i1] = transformTemp[(i0 + ki) % 256, 0];
            }

            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 256; j++)
                    transform[(byte)transformTemp[i, j], j] = (byte)i;
            }

            for (int i = 0; i < count; i += 1024, offset += 1024)
            {
                int blockLen = Math.Min(1024, count - i);

                if (blockLen == 1)
                {
                    data[offset] = transform[data[offset], kb];
                    continue;
                }

                for (int j = 0; j < blockLen - 1; j++)
                    data[offset + j] = transform[data[offset + j], data[offset + j + 1]];
                data[offset + blockLen - 1] = transform[data[offset + blockLen - 1], kb ^ 0x55];

                for (int j = blockLen - 1; j > 0; j--)
                    data[offset + j] = transform[data[offset + j], data[offset + j - 1]];
                data[offset] = transform[data[offset], kb];
            }
        }
    }
}
