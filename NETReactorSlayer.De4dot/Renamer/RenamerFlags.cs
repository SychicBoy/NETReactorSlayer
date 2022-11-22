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

namespace NETReactorSlayer.De4dot.Renamer {
    [Flags]
    public enum RenamerFlags {
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