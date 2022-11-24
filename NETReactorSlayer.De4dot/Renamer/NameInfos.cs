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

using System.Collections.Generic;
using System.Linq;

namespace NETReactorSlayer.De4dot.Renamer
{
    public class NameInfos
    {
        public void Add(string name, NameCreator nameCreator) => _nameInfos.Add(new NameInfo(name, nameCreator));

        public NameCreator Find(string typeName) =>
            (from nameInfo in _nameInfos where typeName.Contains(nameInfo.Name) select nameInfo.NameCreator)
            .FirstOrDefault();

        private readonly IList<NameInfo> _nameInfos = new List<NameInfo>();

        private class NameInfo
        {
            public NameInfo(string name, NameCreator nameCreator)
            {
                Name = name;
                NameCreator = nameCreator;
            }

            public readonly string Name;
            public readonly NameCreator NameCreator;
        }
    }
}