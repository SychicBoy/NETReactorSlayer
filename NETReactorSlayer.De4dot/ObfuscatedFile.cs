using dnlib.DotNet;
using NETReactorSlayer.De4dot.Renamer;

namespace NETReactorSlayer.De4dot
{
    public class ObfuscatedFile : IObfuscatedFile
    {
        public ObfuscatedFile(ModuleDefMD module, IDeobfuscator deobfuscator)
        {
            Deobfuscator = deobfuscator;
            ModuleDefMd = module;
            DeobfuscatorOptions = new Options();
        }

        public void Dispose()
        {
            ModuleDefMd?.Dispose();
            Deobfuscator?.Dispose();
            ModuleDefMd = null;
            Deobfuscator = null;
        }

        public IDeobfuscator Deobfuscator { get; private set; }
        public IDeobfuscatorContext DeobfuscatorContext => new DeobfuscatorContext();
        public Options DeobfuscatorOptions { get; }

        public ModuleDefMD ModuleDefMd { get; private set; }
        public INameChecker NameChecker => Deobfuscator;

        public bool RemoveNamespaceWithOneType =>
            (Deobfuscator.RenamingOptions & RenamingOptions.RemoveNamespaceIfOneType) != 0;

        public bool RenameResourceKeys => (Deobfuscator.RenamingOptions & RenamingOptions.RenameResourceKeys) != 0;
        public bool RenameResourcesInCode => Deobfuscator.TheOptions.RenameResourcesInCode;

        public class Options
        {
            public RenamerFlags RenamerFlags =
                RenamerFlags.RenameNamespaces |
                RenamerFlags.RenameTypes |
                RenamerFlags.RenameEvents |
                RenamerFlags.RenameFields |
                RenamerFlags.RenameMethods |
                RenamerFlags.RenameMethodArgs |
                RenamerFlags.RenameGenericParams |
                RenamerFlags.RestoreEventsFromNames |
                RenamerFlags.RestoreEvents;
        }
    }
}