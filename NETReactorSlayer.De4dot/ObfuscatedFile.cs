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

using dnlib.DotNet;
using NETReactorSlayer.De4dot.Renamer;

namespace NETReactorSlayer.De4dot {
    public class ObfuscatedFile : IObfuscatedFile {
        public ObfuscatedFile(ModuleDefMD module, IDeobfuscator deobfuscator) {
            Deobfuscator = deobfuscator;
            ModuleDefMd = module;
            DeobfuscatorOptions = new Options();
        }

        public void Dispose() {
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

        public class Options {
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