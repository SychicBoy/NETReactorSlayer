/*
    Copyright (C) 2021 CodeStrikers.org
    This file is part of NetReactorSlayer.
    NetReactorSlayer is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    NetReactorSlayer is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with NetReactorSlayer.  If not, see <http://www.gnu.org/licenses/>.
*/
using System.Collections.Generic;
using System.Reflection;

namespace NETReactorSlayer.Core.Utils
{
    public class Variables
    {
        public static readonly string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static readonly string[] supportedVersions = { "6.0", "6.2", "6.3", "6.5", "6.7", "6.8" };
        public static readonly string[] arguments ={
            "--no-necrobit", "     Don't decrypt methods (NecroBit).",
            "--no-anti-tamper", "  Don't remove anti tamper.",
            "--no-anti-debug", "   Don't remove anti debugger.",
            "--no-hide-call", "    Don't restore hidden calls.",
            "--no-str", "          Don't decrypt strings.",
            "--no-rsrc", "         Don't decrypt assembly resources.",
            "--no-deob", "         Don't deobfuscate methods.",
            "--no-arithmetic", "   Don't resolve arithmetic equations.",
            "--no-proxy-call", "   Don't clean proxied calls.",
            "--no-dump", "         Don't dump embedded assemblies",
            "--no-remove", "       Don't remove obfuscator methods, resources, etc..."};
        public static Dictionary<string, bool> options = new Dictionary<string, bool>()
        {
            ["necrobit"] = true,
            ["antitamper"] = true,
            ["antidebug"] = true,
            ["hidecall"] = true,
            ["str"] = true,
            ["rsrc"] = true,
            ["deob"] = true,
            ["arithmetic"] = true,
            ["proxycall"] = true,
            ["dump"] = true,
            ["remove"] = true
        };
    }
}
