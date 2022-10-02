using System;
using dnlib.DotNet;
using NETReactorSlayer.De4dot.Renamer;

namespace NETReactorSlayer.De4dot
{
    public interface IObfuscatedFile : IDisposable
    {
        IDeobfuscatorContext DeobfuscatorContext { get; }
        ObfuscatedFile.Options DeobfuscatorOptions { get; }
        ModuleDefMD ModuleDefMd { get; }
        INameChecker NameChecker { get; }
        bool RemoveNamespaceWithOneType { get; }
        bool RenameResourceKeys { get; }
        bool RenameResourcesInCode { get; }
    }
}