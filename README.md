<p align="center">
  <img alt=".NETReactorSlayer Logo" src="Logo-Dark.png#gh-dark-mode-only" width="1000" />
   <img alt=".NETReactorSlayer Logo" src="Logo-Light.png#gh-light-mode-only" width="1000" />
</p>

# .NETReactorSlayer <br /> ![](https://img.shields.io/github/v/release/SychicBoy/NETReactorSlayer) ![](https://img.shields.io/github/downloads/SychicBoy/NETReactorSlayer/total) ![](https://img.shields.io/github/license/SychicBoy/NETReactorSlayer) ![](https://img.shields.io/github/last-commit/SychicBoy/NETReactorSlayer) ![](https://img.shields.io/badge/donate--btc-bc1qqlm856lh3xvy5sxhgjwl6ehclw9cvzsyknrzgr-yellow)

**.NETReactorSlayer** is an open source (GPLv3) deobfuscator for [Eziriz .NET Reactor](https://www.eziriz.com/reactor_download.htm)

<h1 align="center">Preview</h1>

GUI             |  CLI
:-------------------------:|:-------------------------:
<img src="https://user-images.githubusercontent.com/53654076/161821769-20cb6d1a-9530-4b95-9f23-718f086d81e5.png" width="700">  |  <img src="https://user-images.githubusercontent.com/53654076/161871010-d8b7e734-77ca-493b-ba87-29f4163c1853.png" width="700">

<br />

### Features & Commands:

| Description | Command | Default Value |
| ------ | ------ | ------ |
| Decrypt Methods (NecroBit) | `--decrypt-method` `<BOOL>` | &nbsp; &nbsp; &nbsp;&nbsp;&nbsp; True |
| Deobfuscate Control Flow | `--deobfuscate-cflow` `<BOOL>` | &nbsp; &nbsp; &nbsp;&nbsp;&nbsp; True |
| Decrypt Hidden Calls | `--decrypt-hidden-calls` `<BOOL>` | &nbsp; &nbsp; &nbsp;&nbsp;&nbsp; True |
| Remove Reference Proxies | `--remove-ref-proxies` `<BOOL>` | &nbsp; &nbsp; &nbsp;&nbsp;&nbsp; True |
| Decrypt Strings | `--decrypt-strings` `<BOOL>` | &nbsp; &nbsp; &nbsp;&nbsp;&nbsp; True |
| Remove Anti Tamper & Anti Debug | `--anti-tamper` `<BOOL>` | &nbsp; &nbsp; &nbsp;&nbsp;&nbsp; True |
| Decrypt Assembly Resources | `--decrypt-resources` `<BOOL>` | &nbsp; &nbsp; &nbsp;&nbsp;&nbsp; True |
| Dump Embedded Assemblies | `--dump-assemblies` `<BOOL>` | &nbsp; &nbsp; &nbsp;&nbsp;&nbsp; True |
| Dump Assemblies That Embedded By Costura.Fody | `--dump-costura-assemblies` `<BOOL>` | &nbsp; &nbsp; &nbsp;&nbsp;&nbsp; True |
| Decrypt Tokens | `--decrypt-tokens` `<BOOL>` | &nbsp; &nbsp; &nbsp;&nbsp;&nbsp; True |
| Unpack Original Assembly From Native Image |  |  |
| Close CLI immediately after finish deobfuscation | `--no-pause` `<BOOL>` | &nbsp; &nbsp; &nbsp;&nbsp; False |
| Preserve All MD Tokens | `--preserve-all` `<BOOL>` | &nbsp; &nbsp; &nbsp;&nbsp; False |
| Keep Old Max Stack Value | `--keep-stack` `<BOOL>` | &nbsp; &nbsp; &nbsp;&nbsp; False |
| Cleanup obfuscator leftovers | `-cleanup` `<BOOL>` | &nbsp; &nbsp; &nbsp;&nbsp;&nbsp; True |

### Usage:
Just drag and drop target obfuscated assembly on it.

### Known Issues:
- If target assembly not working after deobfuscation try using `--preserve-all` and/or `--keep-stack` command(s).

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
