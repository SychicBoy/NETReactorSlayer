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

using System.Reflection;
using dnlib.DotNet;
using NETReactorSlayer.Core.Helper;

namespace NETReactorSlayer.Core.Abstractions
{
    public interface IContext
    {
        bool Load();
        void Save();
        IOptions Options { get; }
        ILogger Logger { get; }
        IInfo Info { get; }
        Assembly Assembly { get; set; }
        AssemblyModule AssemblyModule { get; set; }
        ModuleDefMD Module { get; set; }
        ModuleContext ModuleContext { get; set; }
        MyPeImage PeImage { get; set; }
        byte[] ModuleBytes { get; set; }
    }
}