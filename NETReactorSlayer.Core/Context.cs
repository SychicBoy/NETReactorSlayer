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

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using dnlib.DotNet;
using dnlib.DotNet.Writer;
using dnlib.PE;
using NETReactorSlayer.Core.Deobfuscators;
using NETReactorSlayer.Core.Helper.De4dot;

namespace NETReactorSlayer.Core;

public class Context
{
    public static bool IsNative;

    public static bool KeepTypes, RemoveCalls = true, RemoveJunks = true;

    private bool _preserveAll, _keepOldMaxStack;

    public bool NoPause;
    public static string SourceFileName { get; set; }
    public static string SourceFileExt { get; set; }
    public static string SourceDir { get; set; }
    public static string SourcePath { get; set; }
    public static string DestPath { get; set; }
    public static string DestFileName { get; set; }
    public static byte[] ModuleBytes { get; set; }
    public static ModuleDefMD Module { get; set; }
    public static Assembly Assembly { get; set; }
    public static AssemblyModule AssemblyModule { get; set; }
    public static ModuleContext ModuleContext { get; set; }
    public static MyPEImage PeImage { get; set; }
    public Options DeobfuscatorOptions { get; set; }

    public bool Parse(string[] args)
    {
        #region Parse Arguments

        var isValid = false;
        var path = string.Empty;
        DeobfuscatorOptions = new Options();
        for (var i = 0; i < args.Length; i++)
            if (File.Exists(args[i]) && !isValid)
            {
                isValid = true;
                path = args[i];
            }
            else
            {
                if (DeobfuscatorOptions.Arguments.Contains(args[i]) && bool.TryParse(args[i + 1], out var value))
                {
                    var key = args[i];
                    switch (key)
                    {
                        case "--keep-max-stack":
                            _keepOldMaxStack = value;
                            continue;
                        case "--preserve-all":
                            _preserveAll = value;
                            continue;
                        case "--no-pause":
                            NoPause = value;
                            continue;
                        case "--keep-types":
                            KeepTypes = value;
                            continue;
                        case "--rem-calls":
                            RemoveCalls = value;
                            continue;
                        case "--rem-junks":
                            RemoveJunks = value;
                            continue;
                        case "--verbose":
                            Logger.Verbose = value;
                            continue;
                    }

                    if (key.StartsWith("--"))
                        key = key.Substring(2, key.Length - 2);
                    else if (key.StartsWith("-"))
                        key = key.Substring(1, key.Length - 1);
                    else
                        continue;

                    if (!DeobfuscatorOptions.Dictionary.TryGetValue(key, out var deobfuscator)) continue;
                    switch (value)
                    {
                        case true when DeobfuscatorOptions.Stages.All(x =>
                            x.GetType().Name != deobfuscator.GetType().Name):
                            DeobfuscatorOptions.Stages.Add(deobfuscator);
                            break;
                        case true:
                            break;
                        default:
                            DeobfuscatorOptions.Stages.Remove(
                                DeobfuscatorOptions.Stages.FirstOrDefault(x =>
                                    x.GetType().Name == deobfuscator.GetType().Name));
                            break;
                    }
                }
            }

        #endregion

        if (isValid)
        {
            Logger.Done(
                $"{DeobfuscatorOptions.Stages.Count}/13 Modules loaded...");

            #region Get Assembly Infos

            SourcePath = path;
            SourceFileName = Path.GetFileNameWithoutExtension(path);
            SourceFileExt = Path.GetExtension(path);
            SourceDir = Path.GetDirectoryName(path);
            DestPath = SourceDir + "\\" + SourceFileName + "_Slayed" + SourceFileExt;
            DestFileName = SourceFileName + "_Slayed" + SourceFileExt;
            ModuleContext = GetModuleContext();
            AssemblyModule = new AssemblyModule(SourcePath, ModuleContext);

            #endregion

            #region Load Assembly

            try
            {
                Module = AssemblyModule.Load();
                ModuleBytes = DeobUtils.ReadModule(Module);
                PeImage = new MyPEImage(ModuleBytes);
                try
                {
                    Assembly = Assembly.Load(SourcePath);
                } catch
                {
                    Assembly = Assembly.UnsafeLoadFrom(SourcePath);
                }

                return true;
            } catch (Exception ex)
            {
                try
                {
                    if (new NativeUnpacker(new PEImage(SourcePath)).Unpack() is { } unpacked)
                    {
                        #region Create A Temporary File

                        SourcePath = $"{SourceDir}\\PEImage.tmp";
                        while (true)
                            try
                            {
                                File.WriteAllBytes(SourcePath, unpacked);
                                break;
                            } catch (UnauthorizedAccessException)
                            {
                                var saveFileDialog = new SaveFileDialog
                                {
                                    Filter = "Temporary File (*.tmp)| *.tmp",
                                    Title = "Save Temporary File",
                                    FileName = "PEImage.tmp",
                                    RestoreDirectory = true
                                };
                                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                                    SourcePath = saveFileDialog.FileName;
                                else throw;
                            }

                        #endregion

                        AssemblyModule = new AssemblyModule(SourcePath, ModuleContext);
                        Module = AssemblyModule.Load(unpacked);
                        try
                        {
                            Assembly = Assembly.Load(SourcePath);
                        } catch
                        {
                            Assembly = Assembly.UnsafeLoadFrom(SourcePath);
                        }

                        PeImage = new MyPEImage(unpacked);
                        IsNative = true;
                        Process.Start(new ProcessStartInfo(Process.GetCurrentProcess().MainModule?.FileName,
                                $"--delete-native-image {Process.GetCurrentProcess().Id} \"{SourcePath}\"")
                            {WindowStyle = ProcessWindowStyle.Hidden});
                        Logger.Done("Native image unpacked.");
                        ModuleBytes = DeobUtils.ReadModule(Module);
                        return true;
                    }

                    Logger.Error("Failed to load assembly. " + ex.Message);
                    return false;
                } catch (Exception ex1)
                {
                    Logger.Error("Failed to load assembly. " + ex1.Message);
                    return false;
                }
            }

            #endregion
        }

        Logger.Error("No input files specified.\r\n");
        Logger.PrintUsage();
        return false;
    }

    private static ModuleContext GetModuleContext()
    {
        var moduleContext = new ModuleContext();
        var assemblyResolver = new AssemblyResolver(moduleContext);
        var resolver = new Resolver(assemblyResolver);
        moduleContext.AssemblyResolver = assemblyResolver;
        moduleContext.Resolver = resolver;
        assemblyResolver.DefaultModuleContext = moduleContext;
        return moduleContext;
    }

    public void Save()
    {
        try
        {
            ModuleWriterOptionsBase writer = Module.IsILOnly
                ? new ModuleWriterOptions(Module)
                : new NativeModuleWriterOptions(Module, false);
            writer.Logger = DummyLogger.NoThrowInstance;
            if (_preserveAll)
                writer.MetadataOptions.Flags |= MetadataFlags.PreserveAll;
            if (_keepOldMaxStack)
                writer.MetadataOptions.Flags |= MetadataFlags.KeepOldMaxStack;

            if (Module.IsILOnly)
                Module.Write(DestPath, (ModuleWriterOptions) writer);
            else
                Module.NativeWrite(DestPath, (NativeModuleWriterOptions) writer);

            try
            {
                Module?.Dispose();
                PeImage?.Dispose();
            } catch { }

            Logger.Done("Saved to: " + DestFileName);
        } catch (UnauthorizedAccessException ex)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Assembly (*.exe,*.dll)| *.exe;*.dll",
                Title = "Save Assembly",
                FileName = DestFileName,
                RestoreDirectory = true
            };
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                DestPath = saveFileDialog.FileName;
                DestFileName = Path.GetFileName(saveFileDialog.FileName);
                Save();
                return;
            }

            Logger.Error("Failed to save file. " + ex.Message);
        } catch (Exception ex)
        {
            Logger.Error("Failed to save file. " + ex.Message);
        }
    }
}