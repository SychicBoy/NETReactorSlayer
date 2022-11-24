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

namespace NETReactorSlayer.De4dot.Renamer.AsmModules
{
    public class MGenericParamDef : Ref
    {
        public MGenericParamDef(IMemberRef memberRef, int index)
            : base(memberRef, null, index) { }

        public static List<MGenericParamDef> CreateGenericParamDefList(IEnumerable<GenericParam> parameters)
        {
            var list = new List<MGenericParamDef>();
            if (parameters == null)
                return list;
            var i = 0;
            list.AddRange(parameters.Select(param => new MGenericParamDef(param, i++)));
            return list;
        }

        public GenericParam GenericParam => (GenericParam)MemberRef;
    }
}