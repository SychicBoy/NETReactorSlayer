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
using dnlib.DotNet;

namespace NETReactorSlayer.Core.Helper {
    public class AssemblyModule {
        public AssemblyModule(string filename, ModuleContext moduleContext) {
            _filename = Path.GetFullPath(filename);
            _moduleContext = moduleContext;
        }

        public ModuleDefMD Load() {
            var options = new ModuleCreationOptions(_moduleContext) { TryToLoadPdbFromDisk = false };
            return SetModule(ModuleDefMD.Load(_filename, options));
        }

        public ModuleDefMD Load(byte[] fileData) {
            var options = new ModuleCreationOptions(_moduleContext) { TryToLoadPdbFromDisk = false };
            return SetModule(ModuleDefMD.Load(fileData, options));
        }

        private ModuleDefMD SetModule(ModuleDefMD newModule) {
            _module = newModule;
            TheAssemblyResolver.Instance.AddModule(_module);
            _module.EnableTypeDefFindCache = true;
            _module.Location = _filename;
            return _module;
        }

        public ModuleDefMD Reload(
            byte[] newModuleData, DumpedMethodsRestorer dumpedMethodsRestorer, IStringDecrypter stringDecrypter) {
            TheAssemblyResolver.Instance.Remove(_module);
            var options = new ModuleCreationOptions(_moduleContext) { TryToLoadPdbFromDisk = false };
            var mod = ModuleDefMD.Load(newModuleData, options);
            if (dumpedMethodsRestorer != null)
                dumpedMethodsRestorer.Module = mod;
            mod.StringDecrypter = stringDecrypter;
            mod.MethodDecrypter = dumpedMethodsRestorer;
            mod.TablesStream.ColumnReader = dumpedMethodsRestorer;
            mod.TablesStream.MethodRowReader = dumpedMethodsRestorer;
            return SetModule(mod);
        }

        public override string ToString() => _filename;

        private readonly string _filename;
        private ModuleDefMD _module;
        private readonly ModuleContext _moduleContext;
    }
}