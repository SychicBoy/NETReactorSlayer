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

using de4dot.blocks;
using dnlib.DotNet;

namespace NETReactorSlayer.De4dot.Renamer.AsmModules
{
    public class TypeInfo
    {
        public TypeInfo(ITypeDefOrRef typeRef, MTypeDef typeDef)
        {
            TypeRef = typeRef;
            TypeDef = typeDef;
        }

        public TypeInfo(TypeInfo other, GenericInstSig git)
        {
            TypeRef = GenericArgsSubstitutor.Create(other.TypeRef, git);
            TypeDef = other.TypeDef;
        }

        public override int GetHashCode() => TypeDef.GetHashCode() + new SigComparer().GetHashCode(TypeRef);

        public override bool Equals(object obj)
        {
            if (obj is not TypeInfo other)
                return false;
            return TypeDef == other.TypeDef &&
                   new SigComparer().Equals(TypeRef, other.TypeRef);
        }

        public override string ToString() => TypeRef.ToString();

        public readonly MTypeDef TypeDef;
        public readonly ITypeDefOrRef TypeRef;
    }
}