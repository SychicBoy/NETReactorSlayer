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
using System.Reflection;
using dnlib.DotNet;
using dnlib.DotNet.Writer;
using dnlib.PE;
using NETReactorSlayer.Core.Helper;

namespace NETReactorSlayer.Core {
    public class Context {
        public bool Load(Options options) {
            Options = options;
            if (string.IsNullOrEmpty(Options.SourcePath)) {
                Logger.Error("No input files specified.\r\n");
                Logger.PrintUsage();
                return false;
            }

            Logger.Done($"{Options.Stages.Count}/14 Modules loaded...");

            return LoadModule();
        }

        public void Save() {
            try {
                ModuleWriterOptionsBase writer;
                if (Module.IsILOnly)
                    writer = new ModuleWriterOptions(Module);
                else
                    writer = new NativeModuleWriterOptions(Module, false);

                writer.Logger = DummyLogger.NoThrowInstance;
                if (Options.PreserveAllMdTokens)
                    writer.MetadataOptions.Flags |= MetadataFlags.PreserveAll;
                if (Options.KeepOldMaxStackValue)
                    writer.MetadataOptions.Flags |= MetadataFlags.KeepOldMaxStack;

                if (Module.IsILOnly)
                    Module.Write(Options.DestPath, (ModuleWriterOptions)writer);
                else
                    Module.NativeWrite(Options.DestPath, (NativeModuleWriterOptions)writer);

                try {
                    Module?.Dispose();
                    PeImage?.Dispose();
                } catch { }

                Logger.Done("Saved to: " + Options.DestFileName);
            } catch (Exception ex) {
                Logger.Error($"An unexpected error occurred during writing output file. {ex.Message}.");
            }
        }

        #region Private Methods

        private static bool LoadModule() {
            try {
                ModuleContext = GetModuleContext();
                AssemblyModule = new AssemblyModule(Options.SourcePath, ModuleContext);
                Module = AssemblyModule.Load();
                ModuleBytes = DeobUtils.ReadModule(Module);
                PeImage = new MyPeImage(ModuleBytes);
            } catch (Exception ex) {
                try {
                    var unpacked = new NativeUnpacker(new PEImage(Options.SourcePath)).Unpack();
                    if (unpacked == null)
                        throw;
                    Options.SourcePath = Path.Combine(Options.SourceDir, "PEImage.tmp");
                    File.WriteAllBytes(Options.SourcePath, unpacked);

                    AssemblyModule = new AssemblyModule(Options.SourcePath, ModuleContext);
                    Module = AssemblyModule.Load(unpacked);
                    PeImage = new MyPeImage(unpacked);
                    ObfuscatorInfo.NativeStub = true;
                    ModuleBytes = DeobUtils.ReadModule(Module);

                    Process.Start(new ProcessStartInfo(Process.GetCurrentProcess().MainModule?.FileName,
                            $"--del-temp {Process.GetCurrentProcess().Id} \"{Options.SourcePath}\"")
                        { WindowStyle = ProcessWindowStyle.Hidden });

                    Logger.Done("Native stub unpacked.");
                } catch {
                    Logger.Error($"Failed to load assembly. {ex.Message}.");
                    return false;
                }
            }

            ObfuscatorInfo.UsesReflaction = LoadAssembly();
            if (!ObfuscatorInfo.UsesReflaction)
                Logger.Warn("Couldn't load assembly using reflection.");

            return true;
        }

        private static bool LoadAssembly() {
            try {
                Assembly = Assembly.Load(Options.SourcePath);
                return true;
            } catch {
                try {
                    Assembly = Assembly.UnsafeLoadFrom(Options.SourcePath);
                    return true;
                } catch { }
            }

            return false;
        }

        private static ModuleContext GetModuleContext() {
            var moduleContext = new ModuleContext();
            var assemblyResolver = new AssemblyResolver(moduleContext);
            var resolver = new Resolver(assemblyResolver);
            moduleContext.AssemblyResolver = assemblyResolver;
            moduleContext.Resolver = resolver;
            assemblyResolver.DefaultModuleContext = moduleContext;
            return moduleContext;
        }

        #endregion

        #region Fields

        public static ObfuscatorInfo ObfuscatorInfo = new();

        #endregion

        #region Properties

        public static Assembly Assembly { get; set; }
        public static AssemblyModule AssemblyModule { get; set; }
        public static ModuleDefMD Module { get; set; }
        public static byte[] ModuleBytes { get; set; }
        public static ModuleContext ModuleContext { get; set; }
        public static Options Options { get; private set; }
        public static MyPeImage PeImage { get; set; }

        #endregion
    }
}