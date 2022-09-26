using System;
using System.Collections.Generic;

namespace NETReactorSlayer.De4dot.Renamer;

public class VariableNameCreator : TypeNames
{
    static VariableNameCreator()
    {
        OurFullNameToShortName = new Dictionary<string, string>(StringComparer.Ordinal)
        {
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
        OurFullNameToShortNamePrefix = new Dictionary<string, string>(StringComparer.Ordinal)
        {
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

    public VariableNameCreator()
    {
        FullNameToShortName = OurFullNameToShortName;
        FullNameToShortNamePrefix = OurFullNameToShortNamePrefix;
    }

    private static string LowerLeadingChars(string name)
    {
        var s = "";
        for (var i = 0; i < name.Length; i++)
        {
            var c = char.ToLowerInvariant(name[i]);
            if (c == name[i])
                return s + name.Substring(i);
            s += c;
        }

        return s;
    }

    protected override string FixName(string prefix, string name)
    {
        name = LowerLeadingChars(name);
        if (prefix == "")
            return name;
        return prefix + UpperFirst(name);
    }

    private static readonly Dictionary<string, string> OurFullNameToShortName;
    private static readonly Dictionary<string, string> OurFullNameToShortNamePrefix;
}