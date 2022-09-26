using System;

namespace NETReactorSlayer.De4dot;

[Flags]
public enum RenamingOptions
{
    RemoveNamespaceIfOneType = 1,
    RenameResourceKeys = 2
}