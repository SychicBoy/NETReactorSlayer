/*
    Copyright (C) 2021 CodeStrikers.org
    This file is part of NetReactorSlayer.
    NetReactorSlayer is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    NetReactorSlayer is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with NetReactorSlayer.  If not, see <http://www.gnu.org/licenses/>.
*/
using dnlib.DotNet;
using dnlib.DotNet.Writer;
using dnlib.PE;
using NetReactorSlayer.Core.Utils.de4dot;
using NETReactorSlayer.Core.Protections;
using System;
using System.IO;
using System.Reflection;

namespace NETReactorSlayer.Core.Utils
{
    public class Context
    {
        public static bool Parse(string[] args)
        {
            #region Parse Arguments
            bool isValid = false;
            string path = string.Empty;
            foreach (var item in args)
            {
                if (File.Exists(item) && !isValid)
                {
                    isValid = true;
                    path = item;
                }
                else
                {
                    foreach (var opt in Variables.options)
                    {
                        if (item.ToLower().Replace("--no-", string.Empty).Replace("-", string.Empty) == opt.Key)
                        {
                            Variables.options[opt.Key] = false;
                            break;
                        }
                    }
                }
            }
            #endregion
            if (isValid)
            {
                #region Get Assembly Infos
                FilePath = path;
                FileName = Path.GetFileNameWithoutExtension(path);
                FileExt = Path.GetExtension(path);
                FileDir = Path.GetDirectoryName(path);
                DestPath = FileDir + "\\" + FileName + "_Slayed" + FileExt;
                DestName = FileName + "_Slayed" + FileExt;
                ModuleContext = GetModuleContext();
                AssemblyModule = new AssemblyModule(FilePath, ModuleContext);
                #endregion
                #region Load Assembly
                try
                {
                    Context.Module = AssemblyModule.Load();
                    PEImage = new MyPEImage(DeobUtils.ReadModule(Context.Module));
                    try { Assembly = Assembly.Load(FilePath); } catch { Assembly = Assembly.UnsafeLoadFrom(FilePath); }
                    return true;
                }
                catch (Exception ex)
                {
                    try
                    {
                        byte[] unpacked = new Native(new PEImage(FilePath)).Unpack();
                        if (unpacked != null)
                        {
                            #region Save
                            FilePath = FileDir + "\\" + "_native" + ".tmp";
                            File.WriteAllBytes(FilePath, unpacked);
                            #endregion
                            AssemblyModule = new AssemblyModule(FilePath, ModuleContext);
                            Context.Module = AssemblyModule.Load(unpacked);
                            try { Assembly = Assembly.Load(FilePath); } catch { Assembly = Assembly.UnsafeLoadFrom(FilePath); }
                            PEImage = new MyPEImage(unpacked);
                            IsNative = true;
                            Logger.Info("Native image unpacked.");
                            return true;
                        }
                        else
                        {
                            Logger.Error("Failed to load assembly. " + ex.Message);
                            return false;
                        }
                    }
                    catch (Exception ex1)
                    {
                        Logger.Error("Failed to load assembly. " + ex1.Message);
                        return false;
                    }
                }
                #endregion
            }
            else
            {
                Logger.Error("No input files specified.");
                Logger.PrintUsage();
                return false;
            }
        }

        public static ModuleContext GetModuleContext()
        {
            ModuleContext moduleContext = new ModuleContext();
            AssemblyResolver assemblyResolver = new AssemblyResolver(moduleContext);
            Resolver resolver = new Resolver(assemblyResolver);
            moduleContext.AssemblyResolver = assemblyResolver;
            moduleContext.Resolver = resolver;
            assemblyResolver.DefaultModuleContext = moduleContext;
            return moduleContext;
        }

        public static void Save()
        {
            try
            {
                if (Module.IsILOnly)
                {
                    ModuleWriterOptions options = new ModuleWriterOptions(Module) { Logger = DummyLogger.NoThrowInstance };
                    if (Logger.Prompt("Do you want to preserve all MD tokens? (Y/n): "))
                        options.MetadataOptions.Flags = MetadataFlags.PreserveAll;
                    if (Logger.Prompt("Do you want to keep old MaxStack value? (Y/n): "))
                        options.MetadataOptions.Flags |= MetadataFlags.KeepOldMaxStack;
                    Module.Write(DestPath, options);
                }
                else
                {
                    NativeModuleWriterOptions options = new NativeModuleWriterOptions(Module, false) { Logger = DummyLogger.NoThrowInstance };
                    if (Logger.Prompt("Do you want to preserve all MD tokens? (Y/n): "))
                        options.MetadataOptions.Flags = MetadataFlags.PreserveAll;
                    if (Logger.Prompt("Do you want to keep old MaxStack value? (Y/n): "))
                        options.MetadataOptions.Flags |= MetadataFlags.KeepOldMaxStack;
                    Module.NativeWrite(DestPath, options);
                }
                Logger.Info("Saved to: " + DestName);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to save file. " + ex.Message);
            }
        }

        public static bool IsNative = false;
        public static string FileName { get; set; }
        public static string FileExt { get; set; }
        public static string FileDir { get; set; }
        public static string FilePath { get; set; }
        public static string DestPath { get; set; }
        public static string DestName { get; set; }
        public static ModuleDefMD Module { get; set; }
        public static Assembly Assembly { get; set; }
        public static AssemblyModule AssemblyModule { get; set; }
        public static ModuleContext ModuleContext { get; set; }
        public static MyPEImage PEImage { get; set; }
    }
}
