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

using System.IO;
using System.IO.Compression;
using System.Linq;
using dnlib.DotNet;
using NETReactorSlayer.Core.Abstractions;

namespace NETReactorSlayer.Core.Stages
{
    internal class CosturaDumper : IStage
    {
        public void Run(IContext context)
        {
            long count = 0;
            foreach (var resource in context.Module.Resources)
            {
                if (resource is not EmbeddedResource embeddedResource)
                    continue;
                if (embeddedResource.Name == "costura.metadata")
                {
                    Cleaner.AddResourceToBeRemoved(embeddedResource);
                    continue;
                }

                if (!embeddedResource.Name.EndsWith(".compressed"))
                    continue;
                Cleaner.AddResourceToBeRemoved(embeddedResource);
                count++;
                try
                {
                    using var resourceStream = embeddedResource.CreateReader().AsStream();
                    using var deflateStream = new DeflateStream(resourceStream, CompressionMode.Decompress);
                    using var memoryStream = new MemoryStream();
                    deflateStream.CopyTo(memoryStream);
                    try
                    {
                        memoryStream.Position = 0L;
                        File.WriteAllBytes(
                            $"{context.Options.SourceDir}\\{GetAssemblyName(memoryStream.ToArray(), false)}.dll",
                            memoryStream.ToArray());
                    }
                    catch
                    {
                        File.WriteAllBytes(
                            $"{context.Options.SourceDir}\\{embeddedResource.Name.Replace(".compressed", "").Replace("costura.", "")}",
                            memoryStream.ToArray());
                    }

                    memoryStream.Close();
                    deflateStream.Close();
                }
                catch { }
            }

            try
            {
                var cctor = context.Module.GlobalType.FindStaticConstructor();
                if (cctor.HasBody && cctor.Body.HasInstructions)
                    for (var i = 0; i < cctor.Body.Instructions.ToList().Count; i++)
                    {
                        if (cctor.Body.Instructions[i].Operand == null || !cctor.Body.Instructions[i].Operand
                                .ToString()!.Contains("Costura.AssemblyLoader::Attach()"))
                            continue;
                        cctor.Body.Instructions.RemoveAt(i);
                        break;
                    }
            }
            catch { }

            if (count > 0)
                context.Logger.Info(count + " Embedded assemblies dumped (Costura.Fody).");
        }

        private static string GetAssemblyName(byte[] data, bool fullName)
        {
            try
            {
                using var module = ModuleDefMD.Load(data);
                if (fullName)
                    return module.Assembly.FullName;
                return module.Assembly.Name;
            }
            catch { return null; }
        }
    }
}