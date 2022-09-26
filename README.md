<p align="center">
  <img alt=".NETReactorSlayer Logo" src="./Images/Logo-Dark.png#gh-dark-mode-only" width="1000" />
   <img alt=".NETReactorSlayer Logo" src="./Images/Logo-Light.png#gh-light-mode-only" width="1000" />
</p>

# .NETReactorSlayer <br /> <a href="https://github.com/SychicBoy/NETReactorSlayer/actions"> <img src="https://github.com/SychicBoy/NETReactorSlayer/actions/workflows/build.yml/badge.svg"></img> </a> <a href="https://github.com/SychicBoy/NETReactorSlayer/releases/latest"> <img src="https://img.shields.io/github/v/release/SychicBoy/NETReactorSlayer"></img> </a> <a href="#"> <img src="https://img.shields.io/github/downloads/SychicBoy/NETReactorSlayer/total"></img> </a> <a href="#license"> <img src="https://img.shields.io/github/license/SychicBoy/NETReactorSlayer"></img> </a> <a href="https://github.com/SychicBoy/NETReactorSlayer/commits/master"> <img src="https://img.shields.io/github/last-commit/SychicBoy/NETReactorSlayer"></img> </a> <a href="https://commerce.coinbase.com/checkout/1d7bb9ea-4853-4271-95c0-8996d2bdf3c6"><img src="https://img.shields.io/badge/donate-crypto-f46db0" alt="Donate crypto"></img></a>

**.NETReactorSlayer** is an open source (GPLv3) deobfuscator and unpacker for [Eziriz .NET Reactor](https://www.eziriz.com/reactor_download.htm).
<br /><br /><br />
<h2 align="center">Preview</h2>

GUI             |  CLI
:-------------------------:|:-------------------------:
<img src="https://user-images.githubusercontent.com/53654076/192174151-dee6134c-9ac8-46e1-8ae7-96ff1432666b.png" width="700">  |  <img src="https://user-images.githubusercontent.com/53654076/192175117-87533ec8-117d-4470-8ce3-53dc4f4f6a33.png" width="700">

<br />

### Features & Commands:

| Command | Description | Default Value |
| ------ | ------ | ------ |
|  | Unpack native stub |  |
| `--dec-methods` | Decrypt methods body | &nbsp; &nbsp; &nbsp;&nbsp;&nbsp; True |
| `--fix-proxy` | Fix proxied calls | &nbsp; &nbsp; &nbsp;&nbsp;&nbsp; True |
| `--dec-strings` | Decrypt strings | &nbsp; &nbsp; &nbsp;&nbsp;&nbsp; True |
| `--dec-rsrc` | Decrypt assembly resources | &nbsp; &nbsp; &nbsp;&nbsp;&nbsp; True |
| `--dec-bools` | Decrypt booleans | &nbsp; &nbsp; &nbsp;&nbsp;&nbsp; True |
| `--deob-cflow` | Deobfuscate control flow | &nbsp; &nbsp; &nbsp;&nbsp;&nbsp; True |
| `--deob-tokens` | Decrypt tokens | &nbsp; &nbsp; &nbsp;&nbsp;&nbsp; True |
| `--dump-asm` | Dump embedded assemblies | &nbsp; &nbsp; &nbsp;&nbsp;&nbsp; True |
| `--dump-costura` | Dump assemblies that embedded by Costura.Fody | &nbsp; &nbsp; &nbsp;&nbsp;&nbsp; True |
| `--inline-methods` | Inline short methods | &nbsp; &nbsp; &nbsp;&nbsp;&nbsp; True |
| `--rename` | Rename n(amespaces), t(ypes), m(ethods), p(rops), e(vents), f(ields) | &nbsp; &nbsp; &nbsp;&nbsp; ntmfe |
| `--rename-short` | Remove short names | &nbsp; &nbsp; &nbsp;&nbsp; False |
| `--dont-rename` | Don't rename classes, methods, etc.. | &nbsp; &nbsp; &nbsp;&nbsp; False |
| `--rem-antis` | Remove anti tamper & anti debugger | &nbsp; &nbsp; &nbsp;&nbsp;&nbsp; True |
| `--rem-sn` | Remove strong name removal protection | &nbsp; &nbsp; &nbsp;&nbsp;&nbsp; True |
| `--rem-calls` | Remove calls to obfuscator methods | &nbsp; &nbsp; &nbsp;&nbsp;&nbsp; True |
| `--rem-junks` | Remove junk types, methods, fields, etc... | &nbsp; &nbsp; &nbsp;&nbsp;&nbsp; True |
| `--keep-types` | Keep obfuscator types, methods, fields, etc... | &nbsp; &nbsp; &nbsp;&nbsp; False |
| `--preserve-all` | Preserve all metadata tokens | &nbsp; &nbsp; &nbsp;&nbsp; False |
| `--keep-max-stack` | Keep old max stack value | &nbsp; &nbsp; &nbsp;&nbsp; False |
| `--no-pause` | Close CLI immediately after deobfuscation | &nbsp; &nbsp; &nbsp;&nbsp; False |
| `--verbose` | Verbose mode | &nbsp; &nbsp; &nbsp;&nbsp; False |

### Known Issues:
- If target file is not working after deobfuscation try using `--preserve-all` and/or `--keep-max-stack` command(s).

- Since **.NETReactorSlayer** does not yet have the ability to de-virtualize virtualized functions, if the target protected assembly contains virtualized functions, **.NETReactorSlayer** may fail to de-obfuscate some protections such as string encryption and control flow.

<details>
  <summary><b>➡️Click to see </b>few example of comparing virtualized functions with normal functions</summary>

Normal             |  Virtualized
:-------------------------:|:-------------------------:
<img src="https://user-images.githubusercontent.com/53654076/144697746-85e928dd-ad5c-412a-a56c-6b96b3d79df8.png" width="600">  |  <img src="https://user-images.githubusercontent.com/53654076/144697815-dcf2cda4-90f5-4225-8e64-e9b19d9a11b8.png" width="300"><br />Or<br /><img src="https://user-images.githubusercontent.com/53654076/144697787-4b7adc2f-4dde-49ef-9949-4459d6efb10c.png" width="300">

Normal             |  Virtualized
:-------------------------:|:-------------------------:
<img src="https://user-images.githubusercontent.com/53654076/144697246-cc975888-64ad-4371-96d8-af402bf0f8ed.png" width="600">  |  <img src="https://user-images.githubusercontent.com/53654076/144697407-afcf26b2-2d95-4143-8e94-b10b84634174.png" width="300"><br />Or<br /><img src="https://user-images.githubusercontent.com/53654076/144697662-3b6d575c-b989-4efa-979d-fa6c4d6d38a2.png" width="300">
</details><br />

### Contribution:
Want to contribute to this project? Feel free to open a [pull request](https://github.com/SychicBoy/NETReactorSlayer/pulls).

### License:
**.NETReactorSlayer** is licensed under [GPLv3](https://www.gnu.org/licenses/gpl-3.0.en.html).

### Credits:
- [dnlib](https://github.com/0xd4d/dnlib)
- [de4dot.blocks](https://github.com/de4dot/de4dot/tree/master/de4dot.blocks)
- [Harmony](https://github.com/pardeike/Harmony)
