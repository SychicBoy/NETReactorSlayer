using System.Collections.Generic;
using System.Reflection;

namespace NETReactorSlayer.Core.Utils
{
    public class Variables
    {
        public static readonly string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static readonly string[] supportedVersions = { "6.0", "6.2", "6.3", "6.5", "6.7" };
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
            "--no-dump", "         Don't dump embedded assemblies"};
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
            ["dump"] = true
        };
    }
}
