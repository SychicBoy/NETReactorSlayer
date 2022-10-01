/*
    Copyright (C) 2011-2015 de4dot@gmail.com

    This file is part of de4dot.

    de4dot is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    de4dot is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with de4dot.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using dnlib.DotNet;

namespace NETReactorSlayer.De4dot;

public static class Utils
{
    public static IEnumerable<T> Unique<T>(IEnumerable<T> values)
    {
        // HashSet is only available in .NET 3.5 and later.
        var dict = new Dictionary<T, bool>();
        foreach (var val in values)
            dict[val] = true;
        return dict.Keys;
    }

    public static string ToCsharpString(UTF8String s) => ToCsharpString(UTF8String.ToSystemStringOrEmpty(s));

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
            else if (c == '\\' || c == '"')
                AppendEscape(sb, c);
            else
                sb.Append(c);

        sb.Append('"');
        return sb.ToString();
    }

    public static string ShellEscape(string s)
    {
        var sb = new StringBuilder(s.Length + 2);
        sb.Append('"');
        foreach (var c in s)
            if (c == '"')
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

    public static string RemoveNewlines(object o) => RemoveNewlines(o.ToString());

    public static string RemoveNewlines(string s) => s.Replace('\n', ' ').Replace('\r', ' ');

    public static string GetFullPath(string path)
    {
        try
        {
            return Path.GetFullPath(path);
        }
        catch (Exception)
        {
            return path;
        }
    }

    public static string RandomName(int min, int max)
    {
        var numChars = Random.Next(min, max + 1);
        var sb = new StringBuilder(numChars);
        var numLower = 0;
        for (var i = 0; i < numChars; i++)
        {
            if (numLower == 0)
                sb.Append((char)('A' + Random.Next(26)));
            else
                sb.Append((char)('a' + Random.Next(26)));

            if (numLower == 0)
                numLower = Random.Next(1, 5);
            else
                numLower--;
        }

        return sb.ToString();
    }

    public static string GetBaseName(string name)
    {
        var index = name.LastIndexOf(Path.DirectorySeparatorChar);
        if (index < 0)
            return name;
        return name.Substring(index + 1);
    }

    public static string GetDirName(string name) => Path.GetDirectoryName(name);

    public static string GetOurBaseDir()
    {
        if (_ourBaseDir != null)
            return _ourBaseDir;
        return _ourBaseDir = GetDirName(typeof(Utils).Assembly.Location);
    }

    public static string GetPathOfOurFile(string filename) => Path.Combine(GetOurBaseDir(), filename);

    // This fixes a mono (tested 2.10.5) String.StartsWith() bug. NB: stringComparison must be
    // Ordinal or OrdinalIgnoreCase!
    public static bool StartsWith(string left, string right, StringComparison stringComparison)
    {
        if (left.Length < right.Length)
            return false;
        return left.Substring(0, right.Length).Equals(right, stringComparison);
    }

    public static string GetAssemblySimpleName(string name)
    {
        var i = name.IndexOf(',');
        if (i < 0)
            return name;
        return name.Substring(0, i);
    }

    public static bool PathExists(string path)
    {
        try
        {
            return new DirectoryInfo(path).Exists;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static bool FileExists(string path)
    {
        try
        {
            return new FileInfo(path).Exists;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static bool Compare(byte[] a, byte[] b)
    {
        if (a.Length != b.Length)
            return false;
        for (var i = 0; i < a.Length; i++)
            if (a[i] != b[i])
                return false;
        return true;
    }

    public static byte[] ReadFile(string filename)
    {
        // If the file is on the network, and we read more than 2MB, we'll read from the wrong
        // offset in the file! Tested: VMware 8, Win7 x64.
        const int maxBytesRead = 0x200000;

        using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            var fileData = new byte[(int)fileStream.Length];

            int bytes, offset = 0, length = fileData.Length;
            while ((bytes = fileStream.Read(fileData, offset, Math.Min(maxBytesRead, length - offset))) > 0)
                offset += bytes;
            if (offset != length)
                throw new ApplicationException("Could not read all bytes");

            return fileData;
        }
    }

    private static string _ourBaseDir;

    private static readonly Random Random = new Random();
}