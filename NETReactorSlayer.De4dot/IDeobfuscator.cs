using System;
using NETReactorSlayer.De4dot.Renamer;

namespace NETReactorSlayer.De4dot
{
    public interface IDeobfuscator : INameChecker, IDisposable
    {
        string Name { get; }
        RenamingOptions RenamingOptions { get; }
        IDeobfuscatorOptions TheOptions { get; }
    }
}