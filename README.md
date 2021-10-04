# NetReactorSlayer

A deobfuscator for [Eziriz .NET Reactor](https://www.eziriz.com/reactor_download.htm)

# Preview:
![Preview](https://user-images.githubusercontent.com/53654076/135913710-5373907f-07ba-4bb6-b7b1-2be699b85186.png)

# Currently Supported .NET Reactor Versions:
- 6.0.0.0
- 6.2.0.0
- 6.3.0.0
- 6.5.0.0
- 6.7.0.0

# Features:
- Deobfuscate Control Flow
- Restore Hidden Calls
- Remove Proxy Calls
- Decrypt Strings
- Remove Anti Tamper
- Remove Anti Debugger
- Decrypt Resource
- Dump Embedded Assemblies
- Decrypt Methods (NecroBit)
- Unpack Native

# Usage:
Just drag and drop target obfuscated assembly on it.

# Optional commands:
```
--no-necrobit     Don't decrypt methods (NecroBit).
--no-anti-tamper  Don't remove anti tamper.
--no-anti-debug   Don't remove anti debugger.
--no-hide-call    Don't restore hidden calls.
--no-str          Don't decrypt strings.
--no-rsrc         Don't decrypt assembly resources.
--no-deob         Don't deobfuscate methods.
--no-arithmetic   Don't resolve arithmetic equations.
--no-proxy-call   Don't clean proxied calls.
--no-dump         Don't dump embedded assemblies.
```
# Note:
Its free, but there is no support for it, I'll keep updating it for latest .NET Reactor version as I can.

# Credits:
- [dnlib](https://github.com/0xd4d/dnlib)
- [de4dot.blocks](https://github.com/de4dot/de4dot/tree/master/de4dot.blocks)
