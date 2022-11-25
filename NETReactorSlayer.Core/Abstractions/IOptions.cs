/*
    Copyright (C) 2021 CodeStrikers.org
    This file is part of NETReactorSlayer.
    NETReactorSlayer is free software: you can redistribute it and/or modify
    it under the terms of the GNU General License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    NETReactorSlayer is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General License for more details.
    You should have received a copy of the GNU General License
    along with NETReactorSlayer.  If not, see <http://www.gnu.org/licenses/>.
*/

using System.Collections.Generic;
using NETReactorSlayer.De4dot.Renamer;

namespace NETReactorSlayer.Core.Abstractions
{
    public interface IOptions
    {
        string SourceDir { get; set; }
        string SourceFileExt { get; set; }
        string SourceFileName { get; set; }
        string SourcePath { get; set; }
        string DestFileName { get; set; }
        string DestPath { get; set; }
        bool AntiManipulationPatcher { get; set; }
        bool AssemblyResolver { get; set; }
        bool BooleanDecrypter { get; set; }
        bool ProxyCallFixer { get; set; }
        bool ControlFlowDeobfuscator { get; set; }
        bool CosturaDumper { get; set; }
        bool MethodDecrypter { get; set; }
        bool MethodInliner { get; set; }
        bool RemoveCallsToObfuscatorTypes { get; set; }
        bool SymbolRenamer { get; set; }
        bool ResourceResolver { get; set; }
        bool StringDecrypter { get; set; }
        bool StrongNamePatcher { get; set; }
        bool TokenDeobfuscator { get; set; }
        bool RemoveJunks { get; set; }
        bool KeepObfuscatorTypes { get; set; }
        bool KeepOldMaxStackValue { get; set; }
        bool NoPause { get; set; }
        bool PreserveAllMdTokens { get; set; }
        bool RenameShort { get; set; }
        RenamerFlags RenamerFlags { get; set; }
        List<IStage> Stages { get; }
    }
}