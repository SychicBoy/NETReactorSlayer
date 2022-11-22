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

using System;
using System.Collections.Generic;
using dnlib.DotNet;

namespace NETReactorSlayer.De4dot.Renamer {
    public class TypeRenamerState {
        public TypeRenamerState() {
            _existingNames = new ExistingNames();
            _namespaceToNewName = new Dictionary<string, string>(StringComparer.Ordinal);
            _createNamespaceName = new NameCreator("ns");
            GlobalTypeNameCreator = new GlobalTypeNameCreator(_existingNames);
            InternalTypeNameCreator = new TypeNameCreator(_existingNames);
        }

        public void AddTypeName(string name) => _existingNames.Add(name);

        public string GetTypeName(string oldName, string newName) =>
            _existingNames.GetName(oldName, new NameCreator2(newName));

        public string CreateNamespace(TypeDef type, string ns) {
            var asmFullName = type.Module.Assembly != null ? type.Module.Assembly.FullName : "<no assembly>";

            var key = $" [{type.Module.Location}] [{asmFullName}] [{type.Module.Name}] [{ns}] ";
            if (_namespaceToNewName.TryGetValue(key, out var newName))
                return newName;
            return _namespaceToNewName[key] = _createNamespaceName.Create();
        }

        private readonly NameCreator _createNamespaceName;
        private readonly ExistingNames _existingNames;
        private readonly Dictionary<string, string> _namespaceToNewName;
        public ITypeNameCreator GlobalTypeNameCreator;
        public ITypeNameCreator InternalTypeNameCreator;
    }
}