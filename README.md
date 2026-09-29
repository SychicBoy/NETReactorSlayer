<p align="center">
  <img alt="NETReactorSlayer Logo" src="./Images/Logo-Dark.png#gh-dark-mode-only" width="1000" />
   <img alt="NETReactorSlayer Logo" src="./Images/Logo-Light.png#gh-light-mode-only" width="1000" />
</p>

# .NETReactorSlayer <br /> <a href="https://github.com/SychicBoy/NETReactorSlayer/actions"> <img src="https://github.com/SychicBoy/NETReactorSlayer/actions/workflows/build.yml/badge.svg"></img> <a href="https://github.com/SychicBoy/NETReactorSlayer/actions"> <img src="https://github.com/SychicBoy/NETReactorSlayer/actions/workflows/codeql-analysis.yml/badge.svg"></img> </a> </a> <a href="https://github.com/SychicBoy/NETReactorSlayer/releases/latest"> <img src="https://img.shields.io/github/v/release/SychicBoy/NETReactorSlayer"></img> </a> <a href="#"> <img src="https://img.shields.io/github/downloads/SychicBoy/NETReactorSlayer/total"></img> </a> <a href="#license"> <img src="https://img.shields.io/github/license/SychicBoy/NETReactorSlayer"></img> </a> <a href="https://github.com/SychicBoy/NETReactorSlayer/commits/master"> <img src="https://img.shields.io/github/last-commit/SychicBoy/NETReactorSlayer"></img> </a>

NETReactorSlayer is an open source (GPLv3) deobfuscator and unpacker for [Eziriz .NET Reactor](https://www.eziriz.com/reactor_download.htm).
<br /><br />

### Fork Changes

This fork adds reliability and error-handling improvements to the existing decryption and unpacking paths:

- Validates decrypted data lengths, offsets, patch tables, and resource references before reading or writing them.
- Hardens AES, QuickLZ, Deflate, and native-stub decompression against truncated or malformed input.
- Reports more actionable errors and warnings, including per-item failures during string, boolean, and proxy-call restoration.
- Retains related decrypter methods and resources when restoration is partial, and rolls back method-image changes if method decryption fails.
- Improves diagnostics for assembly loading and output writing.

These changes improve robustness; they do not add support for additional .NET Reactor versions or guarantee successful decryption of every protected assembly. The supported version range remains the one provided by the upstream project.

#### Modified C# Files

- `NETReactorSlayer-master/NETReactorSlayer.Core/Context.cs`
- `NETReactorSlayer-master/NETReactorSlayer.Core/Program.cs`
- `NETReactorSlayer-master/NETReactorSlayer.Core/Helper/DeobUtils.cs`
- `NETReactorSlayer-master/NETReactorSlayer.Core/Helper/EncryptedResource.cs`
- `NETReactorSlayer-master/NETReactorSlayer.Core/Helper/NativeUnpacker.cs`
- `NETReactorSlayer-master/NETReactorSlayer.Core/Helper/QuickLZ.cs`
- `NETReactorSlayer-master/NETReactorSlayer.Core/Helper/QuickLZBase.cs`
- `NETReactorSlayer-master/NETReactorSlayer.Core/Stages/BooleanDecrypter.cs`
- `NETReactorSlayer-master/NETReactorSlayer.Core/Stages/MethodDecrypter.cs`
- `NETReactorSlayer-master/NETReactorSlayer.Core/Stages/ProxyCallFixer.cs`
- `NETReactorSlayer-master/NETReactorSlayer.Core/Stages/ResourceResolver.cs`
- `NETReactorSlayer-master/NETReactorSlayer.Core/Stages/StringDecrypter.cs`

#### Code Examples

String and resource data is checked before copying it into a buffer:

```csharp
if (length < 0 || length > data.Length - offset - sizeof(int))
  throw new InvalidDataException($"The {description} length is outside the available data.");
```

QuickLZ input is checked before reading its header fields:

```csharp
if (inData == null || inData.Length < headerLength)
  throw new InvalidDataException("The QuickLZ header is incomplete.");
```

<h2 align="center">Preview</h2>

GUI             |  CLI
:-------------------------:|:-------------------------:
<img src="https://user-images.githubusercontent.com/53654076/194274327-cafb86fd-05f3-4320-9c55-83980665ada4.png" width="700">  |  <img src="https://user-images.githubusercontent.com/53654076/194274522-c8fa222d-624c-44c1-954e-6d7659259167.png" width="700">

<br />

### Binaries:
Get the latest stable version from [GitHub releases](https://github.com/SychicBoy/NETReactorSlayer/releases/latest).

### Documentation:
Check out the [Wiki](https://github.com/SychicBoy/NETReactorSlayer/wiki) for guides and information on how to use it.

### Contribution:
Want to contribute to this project? Feel free to open a [pull request](https://github.com/SychicBoy/NETReactorSlayer/pulls).

### Donation:
<a href="https://codestrikers.sellix.io/"> <img src="https://user-images.githubusercontent.com/53654076/192194815-94455dd4-5a6d-4764-877e-f51c65da7d8c.png" width="200"></img> </a>

### License:
NETReactorSlayer is licensed under [GPLv3](https://www.gnu.org/licenses/gpl-3.0.en.html).

### Credits:
- [dnlib](https://github.com/0xd4d/dnlib)
- [de4dot.blocks](https://github.com/de4dot/de4dot/tree/master/de4dot.blocks)
- [Harmony](https://github.com/pardeike/Harmony)
