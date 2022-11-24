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

using NETReactorSlayer.De4dot.Renamer.AsmModules;

namespace NETReactorSlayer.De4dot.Renamer
{
    public class MemberInfo
    {
        public MemberInfo(Ref memberRef)
        {
            MemberRef = memberRef;
            OldFullName = memberRef.MemberRef.FullName;
            OldName = memberRef.MemberRef.Name.String;
            NewName = memberRef.MemberRef.Name.String;
        }

        public void Rename(string newTypeName)
        {
            Renamed = true;
            NewName = newTypeName;
        }

        public bool GotNewName() => OldName != NewName;

        public override string ToString() => $"O:{OldFullName} -- N:{NewName}";

        protected Ref MemberRef;
        public string NewName;
        public string OldFullName;
        public string OldName;
        public bool Renamed;
        public string SuggestedName;
    }
}