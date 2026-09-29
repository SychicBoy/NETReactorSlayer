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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using NETReactorSlayer.Core.Abstractions;
using NETReactorSlayer.Core.Stages;
using NETReactorSlayer.De4dot.Renamer;
using MemberInfo = System.Reflection.MemberInfo;

namespace NETReactorSlayer.Core
{
    public class Options : IOptions
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
                                ProxyCallFixer = false;
                                continue;
                            case "--dec-strings":
                                RemoveStage(typeof(StringDecrypter));
                                StringDecrypter = false;
                                continue;
                            case "--dec-rsrc":
                                RemoveStage(typeof(ResourceResolver));
                                ResourceResolver = false;
                                continue;
                            case "--dec-bools":
                                RemoveStage(typeof(BooleanDecrypter));
                                BooleanDecrypter = false;
                                continue;
                            case "--deob-cflow":
                                RemoveStage(typeof(ControlFlowDeobfuscator));
                                ControlFlowDeobfuscator = false;
                                continue;
                            case "--deob-tokens":
                                RemoveStage(typeof(TokenDeobfuscator));
                                TokenDeobfuscator = false;
                                continue;
                            case "--dump-asm":
                                RemoveStage(typeof(AssemblyResolver));
                                AssemblyResolver = false;
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
                                AntiManipulationPatcher = false;
                                continue;
                            case "--rem-sn":
                                RemoveStage(typeof(StrongNamePatcher));
                                StrongNamePatcher = false;
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
                    }
                }

                switch (key)
                {
                    case "--dont-rename":
                        SymbolRenamer = false;
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
            SourcePath = Path.GetFullPath(path);
            SourceFileName = Path.GetFileNameWithoutExtension(path);
            SourceFileExt = Path.GetExtension(path);
            SourceDir = Path.GetDirectoryName(path);
            DestPath = Path.Combine(SourceDir, $"{SourceFileName}_Slayed{SourceFileExt}");
            DestFileName = $"{SourceFileName}_Slayed{SourceFileExt}";
        }

        private void RemoveStage(MemberInfo memberInfo) =>
            Stages.Remove(
                Stages.FirstOrDefault(x =>
                    x.GetType().Name == memberInfo.Name));

        public string SourceDir { get; set; }
        public string SourceFileExt { get; set; }
        public string SourceFileName { get; set; }
        public string SourcePath { get; set; }
        public string DestFileName { get; set; }
        public string DestPath { get; set; }
        public bool AntiManipulationPatcher { get; set; } = true;
        public bool AssemblyResolver { get; set; } = true;
        public bool BooleanDecrypter { get; set; } = true;
        public bool ProxyCallFixer { get; set; } = true;
        public bool ControlFlowDeobfuscator { get; set; } = true;
        public bool CosturaDumper { get; set; } = true;
        public bool MethodDecrypter { get; set; } = true;
        public bool MethodInliner { get; set; } = true;
        public bool RemoveCallsToObfuscatorTypes { get; set; } = true;
        public bool SymbolRenamer { get; set; } = true;
        public bool ResourceResolver { get; set; } = true;
        public bool StringDecrypter { get; set; } = true;
        public bool StrongNamePatcher { get; set; } = true;
        public bool TokenDeobfuscator { get; set; } = true;
        public bool RemoveJunks { get; set; } = true;
        public bool KeepObfuscatorTypes { get; set; }
        public bool KeepOldMaxStackValue { get; set; }
        public bool NoPause { get; set; }
        public bool PreserveAllMdTokens { get; set; }
        public bool RenameShort { get; set; }

        public RenamerFlags RenamerFlags { get; set; } =
            RenamerFlags.RenameNamespaces |
            RenamerFlags.RenameTypes |
            RenamerFlags.RenameEvents |
            RenamerFlags.RenameFields |
            RenamerFlags.RenameMethods |
            RenamerFlags.RenameMethodArgs |
            RenamerFlags.RenameGenericParams |
            RenamerFlags.RestoreEventsFromNames |
            RenamerFlags.RestoreEvents;

        public List<IStage> Stages { get; set; } = new()
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
            new TypeRestorer(),
            new Cleaner(),
            new SymbolRenamer()
        };
    }
}