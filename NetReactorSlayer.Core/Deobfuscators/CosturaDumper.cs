/*
    Copyright (C) 2021 CodeStrikers.org
    This file is part of NetReactorSlayer.
    NetReactorSlayer is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    NetReactorSlayer is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with NetReactorSlayer.  If not, see <http://www.gnu.org/licenses/>.
*/
using dnlib.DotNet;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace NETReactorSlayer.Core.Deobfuscators
{
    class CosturaDumper : IDeobfuscator
    {
        public void Execute()
        {
            long count = 0L;
            foreach (Resource resource in DeobfuscatorContext.Module.Resources)
            {
                if (!(resource is EmbeddedResource embeddedResource)) continue;
                if (!embeddedResource.Name.EndsWith(".compressed")) continue;
                Cleaner.ResourceToRemove.Add(embeddedResource);
                count += 1L;
                try
                {
                    using Stream resourceStream = embeddedResource.CreateReader().AsStream();
                    using DeflateStream deflateStream = new DeflateStream(resourceStream, CompressionMode.Decompress);
                    using MemoryStream memoryStream = new MemoryStream();
                    deflateStream.CopyTo(memoryStream);
                    try
                    {
                        memoryStream.Position = 0L;
                        File.WriteAllBytes($"{DeobfuscatorContext.SourceDir}\\{GetAssemblyName(memoryStream.ToArray(), false)}.dll", memoryStream.ToArray());
                    }
                    catch
                    {
                        File.WriteAllBytes($"{DeobfuscatorContext.SourceDir}\\{embeddedResource.Name.Replace(".compressed", "").Replace("costura.", "")}", memoryStream.ToArray());
                    }
                    memoryStream.Close();
                    deflateStream.Close();
                }
                catch { }
            }
            try
            {
                for (int i = 0; i < DeobfuscatorContext.Module.GlobalType.FindStaticConstructor().Body.Instructions.ToList().Count; i++)
                {
                    if (DeobfuscatorContext.Module.GlobalType.FindStaticConstructor().Body.Instructions[i].Operand.ToString().Contains("Costura.AssemblyLoader::Attach()"))
                    {
                        DeobfuscatorContext.Module.GlobalType.FindStaticConstructor().Body.Instructions.RemoveAt(i);
                        break;
                    }
                }
            }
            catch { }
            if (count > 0L)
                Logger.Done((int)count + " Embedded assemblies dumped (Costura.Fody).");
            else
                Logger.Warn("Couldn't find any embedded assembly (Costura.Fody).");
        }

        string GetAssemblyName(byte[] data, bool FullName)
        {
            try
            {
                using ModuleDefMD module = ModuleDefMD.Load(data);
                if (FullName) return module.Assembly.FullName;
                else return module.Assembly.Name;
            }
            catch
            {
                return null;
            }
        }
    }
}
