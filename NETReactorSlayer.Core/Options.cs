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
using NETReactorSlayer.Core.Deobfuscators;

namespace NETReactorSlayer.Core;

public interface IStage
{
    public void Execute();
}

public class Options
{
    public readonly List<string> Arguments = new()
    {
        "--dec-methods", "              Decrypt methods that encrypted by Necrobit (True)",
        "--dec-calls", "                Decrypt hidden calls (True)",
        "--dec-strings", "              Decrypt strings (True)",
        "--dec-rsrc", "                 Decrypt assembly resources (True)",
        "--dec-tokens", "               Decrypt tokens (True)",
        "--dec-bools", "                Decrypt booleans (True)",
        "--deob-cflow", "               Deobfuscate control flow (True)",
        "--dump-asm", "                 Dump embedded assemblies (True)",
        "--dump-costura", "             Dump assemblies that embedded by Costura.Fody (True)",
        "--inline-methods", "           Inline short methods (True)",

        "--rem-antis", "                Remove anti tamper & anti debugger (True)",
        "--rem-sn", "                   Remove strong name removal protection (True)",
        "--rem-calls", "                Remove calls to obfuscator methods (True)",
        "--rem-junks", "                Remove Junk Types, Methods, Fields, etc... (True)",
        "--keep-types", "               Keep obfuscator Types, Methods, Fields, etc... (False)",
        "--preserve-all", "             Preserve All MDTokens (False)",
        "--keep-max-stack", "           Keep Old MaxStack Value (False)",
        "--no-pause", "                 Exit immediately after finish deobfuscation (False)",
        "--verbose", "                  Verbose Mode (False)"
    };

    public Dictionary<string, IStage> Dictionary = new()
    {
        ["dec-methods"] = new MethodDecrypter(),
        ["deob-cflow"] = new CFlowDeob(),
        ["rem-antis"] = new AntiTD(),
        ["inline-methods"] = new MethodInliner(),
        ["dec-calls"] = new CallDecrypter(),
        ["dec-strings"] = new StrDecryptor(),
        ["dec-rsrc"] = new RsrcDecrypter(),
        ["dump-asm"] = new AsmDumper(),
        ["dump-costura"] = new CosturaDumper(),
        ["dec-tokens"] = new TokenDecrypter(),
        ["dec-bools"] = new BoolDecrypter(),
        ["rem-sn"] = new StrongName()
    };

    public HashSet<IStage> Stages = new()
    {
        new MethodDecrypter(),
        new CFlowDeob(),
        new AntiTD(),
        new MethodInliner(),
        new CallDecrypter(),
        new StrDecryptor(),
        new RsrcDecrypter(),
        new AsmDumper(),
        new CosturaDumper(),
        new TokenDecrypter(),
        new BoolDecrypter(),
        new StrongName(),
        new Cleaner()
    };
}