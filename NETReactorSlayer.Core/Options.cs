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
using NETReactorSlayer.Core.Deobfuscators;
using NETReactorSlayer.De4dot.Renamer;

namespace NETReactorSlayer.Core;

public class Options
{
    public Options(IReadOnlyList<string> args)
    {
        var path = string.Empty;
        for (var i = 0; i < args.Count; i++)
        {
            var key = args[i];
            if (File.Exists(key))
            {
                path = key;
                continue;
            }
            if (args.Count < i + 2)
                break;
            var value = args[i + 1];
            if (bool.TryParse(value, out var flag))
            {
                i++;
                if (!flag)
                    switch (key)
                    {
                        case "--dec-methods":
                            RemoveStage(typeof(MethodDecrypter));
                            MethodDecrypter = false;
                            continue;
                        case "--fix-proxy":
                            RemoveStage(typeof(ProxyCallFixer));
                            CallDecrypter = false;
                            continue;
                        case "--dec-strings":
                            RemoveStage(typeof(StringDecrypter));
                            StrDecrypter = false;
                            continue;
                        case "--dec-rsrc":
                            RemoveStage(typeof(ResourceResolver));
                            RsrcDecrypter = false;
                            continue;
                        case "--dec-bools":
                            RemoveStage(typeof(BooleanDecrypter));
                            BoolDecrypter = false;
                            continue;
                        case "--deob-cflow":
                            RemoveStage(typeof(ControlFlowDeobfuscator));
                            CFlowDeob = false;
                            continue;
                        case "--deob-tokens":
                            RemoveStage(typeof(TokenDeobfuscator));
                            TokenDecrypter = false;
                            continue;
                        case "--dump-asm":
                            RemoveStage(typeof(AssemblyResolver));
                            AsmDumper = false;
                            continue;
                        case "--dump-costura":
                            RemoveStage(typeof(CosturaDumper));
                            CosturaDumper = false;
                            continue;
                        case "--inline-methods":
                            RemoveStage(typeof(MethodInliner));
                            MethodInliner = false;
                            continue;
                        case "--rem-antis":
                            RemoveStage(typeof(AntiManipulationPatcher));
                            AntiTd = false;
                            continue;
                        case "--rem-sn":
                            RemoveStage(typeof(StrongNamePatcher));
                            StrongName = false;
                            continue;
                        case "--rem-calls":
                            RemoveCallsToObfuscatorTypes = false;
                            continue;
                        case "--rem-junks":
                            RemoveJunks = false;
                            continue;
                    }

                switch (key)
                {
                    case "--keep-types":
                        KeepObfuscatorTypes = flag;
                        break;
                    case "--preserve-all":
                        PreserveAllMdTokens = flag;
                        break;
                    case "--keep-max-stack":
                        KeepOldMaxStackValue = flag;
                        break;
                    case "--no-pause":
                        NoPause = flag;
                        break;
                    case "--verbose":
                        Verbose = flag;
                        break;
                }
            }

            switch (key)
            {
                case "--dont-rename":
                    Rename = false;
                    RemoveStage(typeof(SymbolRenamer));
                    break;
                case "--rename":
                {
                    var chars = value.ToCharArray();
                    RenamerFlags = RenamerFlags.RenameMethodArgs | RenamerFlags.RenameGenericParams;
                    foreach (var @char in chars)
                        switch (@char)
                        {
                            case 'n':
                                RenamerFlags |= RenamerFlags.RenameNamespaces;
                                break;
                            case 't':
                                RenamerFlags |= RenamerFlags.RenameTypes;
                                break;
                            case 'm':
                                RenamerFlags |= RenamerFlags.RenameMethods;
                                break;
                            case 'f':
                                RenamerFlags |= RenamerFlags.RenameFields;
                                break;
                            case 'p':
                                RenamerFlags |= RenamerFlags.RenameProperties | RenamerFlags.RestoreProperties |
                                                RenamerFlags.RestorePropertiesFromNames;
                                break;
                            case 'e':
                                RenamerFlags |= RenamerFlags.RenameEvents | RenamerFlags.RestoreEvents |
                                                RenamerFlags.RestoreEventsFromNames;
                                break;
                        }

                    break;
                }
            }
        }

        if (path == string.Empty)
            return;
        SourcePath = path;
        SourceFileName = Path.GetFileNameWithoutExtension(path);
        SourceFileExt = Path.GetExtension(path);
        SourceDir = Path.GetDirectoryName(path);
        DestPath = SourceDir + "\\" + SourceFileName + "_Slayed" + SourceFileExt;
        DestFileName = SourceFileName + "_Slayed" + SourceFileExt;
    }

    private void RemoveStage(Type type) =>
        Stages.Remove(
            Stages.FirstOrDefault(x =>
                x.GetType().Name == type.Name));

    public readonly bool AntiTd = true;
    public readonly bool AsmDumper = true;
    public readonly bool BoolDecrypter = true;
    public readonly bool CallDecrypter = true;
    public readonly bool CFlowDeob = true;
    public readonly bool CosturaDumper = true;
    public readonly bool KeepObfuscatorTypes;
    public readonly bool KeepOldMaxStackValue;
    public readonly bool MethodDecrypter = true;
    public readonly bool MethodInliner = true;
    public readonly bool NoPause;
    public readonly bool PreserveAllMdTokens;
    public readonly bool RemoveCallsToObfuscatorTypes = true;
    public readonly bool RemoveJunks = true;
    public readonly bool Rename = true;

    public readonly RenamerFlags RenamerFlags =
        RenamerFlags.RenameNamespaces |
        RenamerFlags.RenameTypes |
        RenamerFlags.RenameEvents |
        RenamerFlags.RenameFields |
        RenamerFlags.RenameMethods |
        RenamerFlags.RenameMethodArgs |
        RenamerFlags.RenameGenericParams |
        RenamerFlags.RestoreEventsFromNames |
        RenamerFlags.RestoreEvents;

    public readonly bool RenameShort = false;
    public readonly bool RsrcDecrypter = true;

    public readonly List<IStage> Stages = new()
    {
        new MethodDecrypter(),
        new ControlFlowDeobfuscator(),
        new AntiManipulationPatcher(),
        new MethodInliner(),
        new ProxyCallFixer(),
        new StringDecrypter(),
        new ResourceResolver(),
        new AssemblyResolver(),
        new CosturaDumper(),
        new TokenDeobfuscator(),
        new BooleanDecrypter(),
        new StrongNamePatcher(),
        new Cleaner(),
        new SymbolRenamer()
    };

    public readonly bool StrDecrypter = true;
    public readonly bool StrongName = true;
    public readonly bool TokenDecrypter = true;
    public readonly bool Verbose;
    public string DestFileName;
    public string DestPath;
    public string SourceDir;
    public string SourceFileExt;
    public string SourceFileName;
    public string SourcePath;
}