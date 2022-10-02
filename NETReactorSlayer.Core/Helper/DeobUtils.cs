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
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using dnlib.DotNet;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace NETReactorSlayer.Core.Helper
{
    public static class DeobUtils
    {
        public static void DecryptAndAddResources(ModuleDef module, Func<byte[]> decryptResource)
        {
            var decryptedResourceData = decryptResource();
            if (decryptedResourceData == null)
                throw new ApplicationException("decryptedResourceData is null");
            var resourceModule = ModuleDefMD.Load(decryptedResourceData);

            foreach (var rsrc in resourceModule.Resources) module.Resources.Add(rsrc);
        }

        public static byte[] ReadModule(ModuleDef module) => ReadFile(module.Location);

        public static bool IsCode(short[] nativeCode, byte[] code)
        {
            if (nativeCode.Length != code.Length)
                return false;
            return !nativeCode.Where((t, i) => t != -1 && (byte)t != code[i]).Any();
        }

        public static byte[] ReadFile(string filename)
        {
            const int maxBytesRead = 0x200000;

            using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var fileData = new byte[(int)fileStream.Length];

                int bytes, offset = 0, length = fileData.Length;
                while ((bytes = fileStream.Read(fileData, offset, Math.Min(maxBytesRead, length - offset))) > 0)
                    offset += bytes;
                if (offset != length)
                    throw new ApplicationException("Could not read all bytes");

                return fileData;
            }
        }

        public static byte[] AesDecrypt(byte[] data, byte[] key, byte[] iv)
        {
            #if !NETFRAMEWORK
            #pragma warning disable SYSLIB0022
            #endif
            using (var aes = new RijndaelManaged { Mode = CipherMode.CBC })
            #if !NETFRAMEWORK
            #pragma warning restore SYSLIB0022
            #endif
            using (var transform = aes.CreateDecryptor(key, iv))
            {
                return transform.TransformFinalBlock(data, 0, data.Length);
            }
        }

        public static byte[] Inflate(byte[] data, bool noHeader) => Inflate(data, 0, data.Length, noHeader);

        public static byte[] Inflate(byte[] data, int start, int len, bool noHeader) =>
            Inflate(data, start, len, new Inflater(noHeader));

        public static byte[] Inflate(byte[] data, int start, int len, Inflater inflater)
        {
            var buffer = new byte[0x1000];
            var memStream = new MemoryStream();
            inflater.SetInput(data, start, len);
            while (true)
            {
                var count = inflater.Inflate(buffer, 0, buffer.Length);
                if (count == 0)
                    break;
                memStream.Write(buffer, 0, count);
            }

            return memStream.ToArray();
        }
    }
}