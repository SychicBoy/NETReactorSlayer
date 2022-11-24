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
using System.IO;
using System.Linq;
using System.Text;

namespace NETReactorSlayer.De4dot
{
    public static class Utils
    {
        public static IEnumerable<T> Unique<T>(IEnumerable<T> values)
        {
            var dict = new Dictionary<T, bool>();
            foreach (var val in values)
                dict[val] = true;
            return dict.Keys;
        }

        public static string ToCsharpString(string s)
        {
            var sb = new StringBuilder(s.Length + 2);
            sb.Append('"');
            foreach (var c in s)
                if (c < 0x20)
                    switch (c)
                    {
                        case '\a':
                            AppendEscape(sb, 'a');
                            break;
                        case '\b':
                            AppendEscape(sb, 'b');
                            break;
                        case '\f':
                            AppendEscape(sb, 'f');
                            break;
                        case '\n':
                            AppendEscape(sb, 'n');
                            break;
                        case '\r':
                            AppendEscape(sb, 'r');
                            break;
                        case '\t':
                            AppendEscape(sb, 't');
                            break;
                        case '\v':
                            AppendEscape(sb, 'v');
                            break;
                        default:
                            sb.Append($@"\u{(int)c:X4}");
                            break;
                    }
                else if (c is '\\' or '"')
                    AppendEscape(sb, c);
                else
                    sb.Append(c);

            sb.Append('"');
            return sb.ToString();
        }

        private static void AppendEscape(StringBuilder sb, char c)
        {
            sb.Append('\\');
            sb.Append(c);
        }

        public static string GetBaseName(string name)
        {
            var index = name.LastIndexOf(Path.DirectorySeparatorChar);
            return index < 0 ? name : name.Substring(index + 1);
        }

        public static bool StartsWith(string left, string right, StringComparison stringComparison) =>
            left.Length >= right.Length && left.Substring(0, right.Length).Equals(right, stringComparison);

        public static bool Compare(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            return !a.Where((t, i) => t != b[i]).Any();
        }
    }
}