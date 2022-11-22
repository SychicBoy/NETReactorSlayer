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

namespace NETReactorSlayer.De4dot.Renamer {
    public class VariableNameCreator : TypeNames {
        static VariableNameCreator() {
            OurFullNameToShortName = new Dictionary<string, string>(StringComparer.Ordinal) {
                { "System.Boolean", "bool" },
                { "System.Byte", "byte" },
                { "System.Char", "char" },
                { "System.Double", "double" },
                { "System.Int16", "short" },
                { "System.Int32", "int" },
                { "System.Int64", "long" },
                { "System.IntPtr", "intptr" },
                { "System.SByte", "sbyte" },
                { "System.Single", "float" },
                { "System.String", "string" },
                { "System.UInt16", "ushort" },
                { "System.UInt32", "uint" },
                { "System.UInt64", "ulong" },
                { "System.UIntPtr", "uintptr" },
                { "System.Decimal", "decimal" }
            };
            OurFullNameToShortNamePrefix = new Dictionary<string, string>(StringComparer.Ordinal) {
                { "System.Boolean", "Bool" },
                { "System.Byte", "Byte" },
                { "System.Char", "Char" },
                { "System.Double", "Double" },
                { "System.Int16", "Short" },
                { "System.Int32", "Int" },
                { "System.Int64", "Long" },
                { "System.IntPtr", "IntPtr" },
                { "System.SByte", "SByte" },
                { "System.Single", "Float" },
                { "System.String", "String" },
                { "System.UInt16", "UShort" },
                { "System.UInt32", "UInt" },
                { "System.UInt64", "ULong" },
                { "System.UIntPtr", "UIntPtr" },
                { "System.Decimal", "Decimal" }
            };
        }

        public VariableNameCreator() {
            FullNameToShortName = OurFullNameToShortName;
            FullNameToShortNamePrefix = OurFullNameToShortNamePrefix;
        }

        private static string LowerLeadingChars(string name) {
            var s = "";
            for (var i = 0; i < name.Length; i++) {
                var c = char.ToLowerInvariant(name[i]);
                if (c == name[i])
                    return s + name.Substring(i);
                s += c;
            }

            return s;
        }

        protected override string FixName(string prefix, string name) {
            name = LowerLeadingChars(name);
            if (prefix == "")
                return name;
            return prefix + UpperFirst(name);
        }

        private static readonly Dictionary<string, string> OurFullNameToShortName;
        private static readonly Dictionary<string, string> OurFullNameToShortNamePrefix;
    }
}