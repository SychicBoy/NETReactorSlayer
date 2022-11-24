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

namespace NETReactorSlayer.De4dot.Renamer
{
    public class TypeNameCreator : ITypeNameCreator
    {
        // ReSharper disable VirtualMemberCallInConstructor
        public TypeNameCreator(ExistingNames existingNames)
        {
            _existingNames = existingNames;
            _createUnknownTypeName = CreateNameCreator("Type");
            _createEnumName = CreateNameCreator("Enum");
            _createStructName = CreateNameCreator("Struct");
            _createDelegateName = CreateNameCreator("Delegate");
            _createClassName = CreateNameCreator("Class");
            _createInterfaceName = CreateNameCreator("Interface");

            var names = new[]
            {
                "Exception",
                "EventArgs",
                "Attribute",
                "Form",
                "Dialog",
                "Control",
                "Stream"
            };
            foreach (var name in names)
                _nameInfos.Add(name, CreateNameCreator(name));
        }

        public virtual NameCreator CreateNameCreator(string prefix) => new(prefix);

        private NameCreator GetNameCreator(TypeDef typeDef, string newBaseTypeName)
        {
            var nameCreator = _createUnknownTypeName;
            if (typeDef.IsEnum)
                nameCreator = _createEnumName;
            else if (typeDef.IsValueType)
                nameCreator = _createStructName;
            else if (typeDef.IsClass)
            {
                if (typeDef.BaseType != null)
                {
                    var fullName = typeDef.BaseType.FullName;
                    switch (fullName)
                    {
                        case "System.Delegate":
                        case "System.MulticastDelegate":
                            nameCreator = _createDelegateName;
                            break;
                        default:
                            nameCreator = _nameInfos.Find(newBaseTypeName ?? typeDef.BaseType.Name.String) ??
                                          _createClassName;
                            break;
                    }
                } else
                    nameCreator = _createClassName;
            } else if (typeDef.IsInterface)
                nameCreator = _createInterfaceName;

            return nameCreator;
        }

        public string Create(TypeDef typeDef, string newBaseTypeName)
        {
            var nameCreator = GetNameCreator(typeDef, newBaseTypeName);
            return _existingNames.GetName(typeDef.Name.String, nameCreator);
        }

        private readonly NameCreator _createClassName;
        private readonly NameCreator _createDelegateName;
        private readonly NameCreator _createEnumName;
        private readonly NameCreator _createInterfaceName;
        private readonly NameCreator _createStructName;
        private readonly NameCreator _createUnknownTypeName;
        private readonly ExistingNames _existingNames;
        private readonly NameInfos _nameInfos = new();
    }
}