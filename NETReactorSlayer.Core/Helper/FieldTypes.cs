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
using dnlib.DotNet;

namespace NETReactorSlayer.Core.Helper {
    public class FieldTypes : StringCounts {
        public FieldTypes(IEnumerable<FieldDef> fields) => Initialize(fields);

        private void Initialize(IEnumerable<FieldDef> fields) {
            if (fields == null)
                return;
            foreach (var type in fields.Select(field => field.FieldSig.GetFieldType()).Where(type => type != null))
                Add(type.FullName);
        }
    }
}