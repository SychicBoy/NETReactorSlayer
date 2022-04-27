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
using NETReactorSlayer.Core.Deobfuscators;
using System.Collections.Generic;

namespace NETReactorSlayer.Core
{
    public class DeobfuscatorOptions
    {
        public Dictionary<string, IDeobfuscator> Dictionary = new Dictionary<string, IDeobfuscator>()
        {
            ["decrypt-methods"] = new MethodDecryptor(),
            ["deobfuscate-cflow"] = new ControlFlowDeobfuscator(),
            ["anti-tamper"] = new AntiTamperPatcher(),
            ["remove-ref-proxies"] = new ProxyCleaner(),
            ["decrypt-hidden-calls"] = new HideCallDecryptor(),
            ["decrypt-strings"] = new StringDecryptor(),
            ["decrypt-resources"] = new ResourceDecryptor(),
            ["dump-assemblies"] = new AssemblyDumper(),
            ["dump-costura-assemblies"] = new CosturaDumper(),
            ["decrypt-tokens"] = new TokenDecryptor(),
            ["decrypt-booleans"] = new BooleanDecryptor(),
            ["cleanup"] = new Cleaner()
        };
        public HashSet<IDeobfuscator> Stages = new HashSet<IDeobfuscator>()
        {
            new MethodDecryptor(),
            new ControlFlowDeobfuscator(),
            new AntiTamperPatcher(),
            new ProxyCleaner(),
            new HideCallDecryptor(),
            new StringDecryptor(),
            new ResourceDecryptor(),
            new AssemblyDumper(),
            new CosturaDumper(),
            new TokenDecryptor(),
            new BooleanDecryptor(),
            new Cleaner()
        };
        public readonly List<string> Arguments = new List<string>()
        {
            "--decrypt-methods",  "          Decrypt methods that encrypted by Necrobit (True)",
            "--deobfuscate-cflow",  "        Deobfuscate control flow (True)",
            "--anti-tamper",  "              Patch anti tamper (True)",
            "--remove-ref-proxies",  "       Remove reference proxies (True)",
            "--decrypt-hidden-calls",  "     Decrypt hidden calls (True)",
            "--decrypt-strings",  "          Decrypt strings (True)",
            "--decrypt-resources",  "        Decrypt assembly resources (True)",
            "--dump-assemblies",  "          Dump embedded assemblies (True)",
            "--dump-costura-assemblies",  "  Dump assemblies that embedded by Costura.Fody (True)",
            "--decrypt-tokens",  "           Decrypt obfuscated tokens (True)",
            "--decrypt-booleans",  "         Decrypt booleans (True)",
            "--preserve-all",  "             PreserveAll MDTokens (False)",
            "--keep-stack",  "               KeepOldMaxStack (False)",
            "--no-pause",  "                 Exit immediately after finish deobfuscation (False)",
            "-cleanup",  "                   Cleanup obfuscator leftovers (True)"
        };
    }
}
