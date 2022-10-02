using System;
using System.Collections.Generic;
using dnlib.DotNet;

namespace NETReactorSlayer.De4dot.Renamer
{
    public class TypeRenamerState
    {
        public TypeRenamerState()
        {
            _existingNames = new ExistingNames();
            _namespaceToNewName = new Dictionary<string, string>(StringComparer.Ordinal);
            _createNamespaceName = new NameCreator("ns");
            GlobalTypeNameCreator = new GlobalTypeNameCreator(_existingNames);
            InternalTypeNameCreator = new TypeNameCreator(_existingNames);
        }

        public void AddTypeName(string name) => _existingNames.Add(name);

        public string GetTypeName(string oldName, string newName) =>
            _existingNames.GetName(oldName, new NameCreator2(newName));

        public string CreateNamespace(TypeDef type, string ns)
        {
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