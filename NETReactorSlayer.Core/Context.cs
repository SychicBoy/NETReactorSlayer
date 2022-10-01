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

namespace NETReactorSlayer.Core
{
    public class Context
    {
        public bool Load(Options options)
        {
            Options = options;
            if (string.IsNullOrEmpty(Options.SourcePath))
            {
                Logger.Error("No input files specified.\r\n");
                Logger.PrintUsage();
                return false;
            }

            Logger.Done($"{Options.Stages.Count}/14 Modules loaded...");

            return LoadAssembly();
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
                catch
                {
                }

                Logger.Done("Saved to: " + Options.DestFileName);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to save file. " + ex.Message);
            }
        }

        #region Private Methods

        private bool LoadAssembly()
        {
            try
            {
                ModuleContext = GetModuleContext();
                AssemblyModule = new AssemblyModule(Options.SourcePath, ModuleContext);
                Module = AssemblyModule.Load();
                ModuleBytes = DeobUtils.ReadModule(Module);
                PeImage = new MyPeImage(ModuleBytes);
                try
                {
                    Assembly = Assembly.Load(Options.SourcePath);
                }
                catch
                {
                    Assembly = Assembly.UnsafeLoadFrom(Options.SourcePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                try
                {
                    var unpacked = new NativeUnpacker(new PEImage(Options.SourcePath)).Unpack();
                    if (unpacked != null)
                    {
                        #region Create A Temporary File

                        Options.SourcePath = $"{Options.SourceDir}\\PEImage.tmp";
                        File.WriteAllBytes(Options.SourcePath, unpacked);

                        #endregion

                        AssemblyModule = new AssemblyModule(Options.SourcePath, ModuleContext);
                        Module = AssemblyModule.Load(unpacked);
                        try
                        {
                            Assembly = Assembly.Load(Options.SourcePath);
                        }
                        catch
                        {
                            Assembly = Assembly.UnsafeLoadFrom(Options.SourcePath);
                        }

                        PeImage = new MyPeImage(unpacked);
                        ObfuscatorInfo.NativeStub = true;
                        Process.Start(new ProcessStartInfo(Process.GetCurrentProcess().MainModule?.FileName,
                                $"--del-temp {Process.GetCurrentProcess().Id} \"{Options.SourcePath}\"")
                        { WindowStyle = ProcessWindowStyle.Hidden });
                        Logger.Done("Native stub unpacked.");
                        ModuleBytes = DeobUtils.ReadModule(Module);
                        return true;
                    }

                    if (ex is FileLoadException)
                    {
                        Logger.Error(Environment.Is64BitProcess
                            ? "Failed to load assembly. Try x86 version."
                            : "Failed to load assembly. Try x64 version.");
                    }
                    else
                        Logger.Error("Failed to load assembly. " + ex.Message);

                    return false;
                }
                catch (Exception ex1)
                {
                    Logger.Error("Failed to load assembly. " + ex1.Message);
                    return false;
                }
            }
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

        #endregion

        #region Fields

        public static ObfuscatorInfo ObfuscatorInfo = new ObfuscatorInfo();

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