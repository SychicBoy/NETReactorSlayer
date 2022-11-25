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
using NETReactorSlayer.Core.Abstractions;
using NETReactorSlayer.Core.Helper;
using ILogger = NETReactorSlayer.Core.Abstractions.ILogger;

namespace NETReactorSlayer.Core
{
    public class Context : IContext
    {
        public Context(IOptions options, ILogger logger)
        {
            Options = options;
            Logger = logger;
        }

        public bool Load()
        {
            if (string.IsNullOrEmpty(Options.SourcePath))
            {
                Logger.Error("No input files specified.\r\n");
                Logger.PrintUsage();
                return false;
            }

            Logger.Info($"{Options.Stages.Count}/15 Modules loaded...");

            try
            {
                ModuleContext = GetModuleContext();
                AssemblyModule = new AssemblyModule(Options.SourcePath, ModuleContext);
                Module = AssemblyModule.Load();
                ModuleBytes = DeobUtils.ReadModule(Module);
                PeImage = new MyPeImage(ModuleBytes);
            }
            catch (Exception ex)
            {
                try
                {
                    var unpacked = new NativeUnpacker(new PEImage(Options.SourcePath)).Unpack();
                    if (unpacked == null)
                        throw;
                    Options.SourcePath = Path.Combine(Options.SourceDir, "PEImage.tmp");
                    File.WriteAllBytes(Options.SourcePath, unpacked);

                    AssemblyModule = new AssemblyModule(Options.SourcePath, ModuleContext);
                    Module = AssemblyModule.Load(unpacked);
                    PeImage = new MyPeImage(unpacked);
                    Info.NativeStub = true;
                    ModuleBytes = DeobUtils.ReadModule(Module);

                    Process.Start(new ProcessStartInfo(Process.GetCurrentProcess().MainModule?.FileName,
                            $"--del-temp {Process.GetCurrentProcess().Id} \"{Options.SourcePath}\"")
                        { WindowStyle = ProcessWindowStyle.Hidden });

                    Logger.Info("Native stub unpacked.");
                }
                catch
                {
                    Logger.Error($"Failed to load assembly. {ex.Message}.");
                    return false;
                }
            }

            Info.UsesReflection = LoadAssembly();
            if (!Info.UsesReflection)
                Logger.Warn("Couldn't load assembly using reflection.");

            return true;
        }

        public void Save()
        {
            try
            {
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

                try
                {
                    Module?.Dispose();
                    PeImage?.Dispose();
                }
                catch { }

                Logger.Info("Saved to: " + Options.DestFileName);
            }
            catch (Exception ex)
            {
                Logger.Error($"An unexpected error occurred during writing output file. {ex.Message}.");
            }
        }

        private bool LoadAssembly()
        {
            try
            {
                Assembly = Assembly.Load(Options.SourcePath);
                return true;
            }
            catch
            {
                try
                {
                    Assembly = Assembly.UnsafeLoadFrom(Options.SourcePath);
                    return true;
                }
                catch { }
            }

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

        public IOptions Options { get; }
        public IInfo Info { get; } = new Info();
        public ILogger Logger { get; }
        public Assembly Assembly { get; set; }
        public AssemblyModule AssemblyModule { get; set; }
        public ModuleDefMD Module { get; set; }
        public ModuleContext ModuleContext { get; set; }
        public MyPeImage PeImage { get; set; }
        public byte[] ModuleBytes { get; set; }
    }
}