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

namespace NETReactorSlayer.Core.Deobfuscators;

internal class CosturaDumper : IStage
{
    public void Execute()
    {
        var count = 0L;
        foreach (var resource in Context.Module.Resources)
        {
            if (!(resource is EmbeddedResource embeddedResource)) continue;
            if (embeddedResource.Name == "costura.metadata")
            {
                Cleaner.ResourceToRemove.Add(embeddedResource);
                continue;
            }

            if (!embeddedResource.Name.EndsWith(".compressed")) continue;
            Cleaner.ResourceToRemove.Add(embeddedResource);
            count += 1L;
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
                        $"{Context.SourceDir}\\{GetAssemblyName(memoryStream.ToArray(), false)}.dll",
                        memoryStream.ToArray());
                } catch
                {
                    File.WriteAllBytes(
                        $"{Context.SourceDir}\\{embeddedResource.Name.Replace(".compressed", "").Replace("costura.", "")}",
                        memoryStream.ToArray());
                }

                memoryStream.Close();
                deflateStream.Close();
            } catch { }
        }

        try
        {
            for (var i = 0;
                 i < Context.Module.GlobalType.FindStaticConstructor().Body.Instructions.ToList().Count;
                 i++)
                if (Context.Module.GlobalType.FindStaticConstructor().Body.Instructions[i].Operand
                    .ToString().Contains("Costura.AssemblyLoader::Attach()"))
                {
                    Context.Module.GlobalType.FindStaticConstructor().Body.Instructions.RemoveAt(i);
                    break;
                }
        } catch { }

        if (count > 0L)
            Logger.Done((int) count + " Embedded assemblies dumped (Costura.Fody).");
        else
            Logger.Warn("Couldn't find any embedded assembly (Costura.Fody).");
    }

    private string GetAssemblyName(byte[] data, bool fullName)
    {
        try
        {
            using var module = ModuleDefMD.Load(data);
            if (fullName) return module.Assembly.FullName;
            return module.Assembly.Name;
        } catch
        {
            return null;
        }
    }
}