# NetReactorSlayer

An open source (GPLv3) deobfuscator for [Eziriz .NET Reactor](https://www.eziriz.com/reactor_download.htm)

# Preview:
![Preview](https://user-images.githubusercontent.com/53654076/142784856-e58ec3a2-6e07-4337-add4-9373a65638a3.png)

# Currently Supported .NET Reactor Versions:
- From 6.0.0.0 To 6.8.0.0

# Features:
- Clean Control Flow
- Restore Hidden Calls
- Remove Proxy Calls
- Decrypt Strings
- Remove Anti Tamper
- Remove Anti Debugger
- Decrypt Resources
- Dump Embedded Assemblies
- Decrypt Methods (NecroBit)
- Unpack Native
- Decrypt Tokens

# Usage:
Just drag and drop target obfuscated assembly on it.

# Optional commands:
```
--no-necrobit        Don't decrypt methods (NecroBit).
--no-anti-tamper     Don't remove anti tamper.
--no-anti-debug      Don't remove anti debugger.
--no-hide-call       Don't restore hidden calls.
--no-str             Don't decrypt strings.
--no-rsrc            Don't decrypt assembly resources.
--no-deob            Don't deobfuscate methods.
--no-arithmetic      Don't resolve arithmetic equations.
--no-proxy-call      Don't clean proxied calls.
--no-dump            Don't dump embedded assemblies.
--no-remove          Don't remove obfuscator methods, resources, etc...
--no-decrypt-token   Don't decrypt tokens.
```
# Known Issues:
- ### Strings are still encrypted after deobfuscation:
In some targets string decryptor method is virtualized, that's why NetReactorSlayer can't decrypt strings.
### How to know is string decryptor method is virtualized or not:
The normal string decryptor method should looks like this:
![image](https://user-images.githubusercontent.com/53654076/144697746-85e928dd-ad5c-412a-a56c-6b96b3d79df8.png)
And the virtualized string decryptor method should looks like one of below images:
![image](https://user-images.githubusercontent.com/53654076/144697815-dcf2cda4-90f5-4225-8e64-e9b19d9a11b8.png)

![image](https://user-images.githubusercontent.com/53654076/144697787-4b7adc2f-4dde-49ef-9949-4459d6efb10c.png)

- ### Control Flow Deobfuscator Not Working / Control Flow Deobfuscator Deleted Most OpCodes:
.NET Reactor 6.7 or above use some arithmetic equations to apply control flow:
![image](https://user-images.githubusercontent.com/53654076/144697149-da0e82b8-dcb8-4a98-90fd-defda5b172e3.png)
So if you click on the class of field, You'll see one of class methods define the fields value on runtime:
![image](https://user-images.githubusercontent.com/53654076/144697246-cc975888-64ad-4371-96d8-af402bf0f8ed.png)
So NetReactorSlayer get that fields value to deobfuscate control flow, but in some targets this method is virtualized and the method goanna looks like one of below images:
![image](https://user-images.githubusercontent.com/53654076/144697407-afcf26b2-2d95-4143-8e94-b10b84634174.png)

![image](https://user-images.githubusercontent.com/53654076/144697662-3b6d575c-b989-4efa-979d-fa6c4d6d38a2.png)
That's why NetReactorSlayer get's failed to clean controlflow because it's don't have a feature yet to devirtualize virtualized methods. 

- ### Target file not working after deobfuscation:
- Try to save deobfuscated file with Preserve all MD tokens & Keep old MaxStack options:
![image](https://user-images.githubusercontent.com/53654076/144698219-dbf8917e-c2bf-425c-b46f-cd6d50031045.png)

# Note:
Its free, but there is no support for it, I'll keep updating it for latest .NET Reactor version as I can.

# Credits:
- [dnlib](https://github.com/0xd4d/dnlib)
- [de4dot.blocks](https://github.com/de4dot/de4dot/tree/master/de4dot.blocks)
- [Harmony](https://github.com/pardeike/Harmony)
