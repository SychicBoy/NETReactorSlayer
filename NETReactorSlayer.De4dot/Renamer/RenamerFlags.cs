using System;

namespace NETReactorSlayer.De4dot.Renamer
{
    [Flags]
    public enum RenamerFlags
    {
        RenameNamespaces = 1,
        RenameTypes = 2,
        RenameProperties = 4,
        RenameEvents = 8,
        RenameFields = 0x10,
        RenameMethods = 0x20,
        RenameMethodArgs = 0x40,
        RenameGenericParams = 0x80,
        RestoreProperties = 0x100,
        RestorePropertiesFromNames = 0x200,
        RestoreEvents = 0x400,
        RestoreEventsFromNames = 0x800,
        DontCreateNewParamDefs = 0x1000,
        DontRenameDelegateFields = 0x2000
    }
}