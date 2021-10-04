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
                    ModuleWriterOptions options = new ModuleWriterOptions(Module);
                    options.Logger = DummyLogger.NoThrowInstance;
                    options.MetadataOptions.Flags |= (MetadataFlags.PreserveTypeRefRids | MetadataFlags.PreserveTypeDefRids | MetadataFlags.PreserveFieldRids | MetadataFlags.PreserveMethodRids | MetadataFlags.PreserveParamRids | MetadataFlags.PreserveMemberRefRids | MetadataFlags.PreserveStandAloneSigRids | MetadataFlags.PreserveEventRids | MetadataFlags.PreservePropertyRids | MetadataFlags.PreserveTypeSpecRids | MetadataFlags.PreserveMethodSpecRids | MetadataFlags.PreserveUSOffsets | MetadataFlags.PreserveBlobOffsets | MetadataFlags.PreserveExtraSignatureData);
                    Module.Write(DestPath, options);
                }
                else
                {
                    NativeModuleWriterOptions options = new NativeModuleWriterOptions(Module, false);
                    options.Logger = DummyLogger.NoThrowInstance;
                    options.MetadataOptions.Flags |= (MetadataFlags.PreserveTypeRefRids | MetadataFlags.PreserveTypeDefRids | MetadataFlags.PreserveFieldRids | MetadataFlags.PreserveMethodRids | MetadataFlags.PreserveParamRids | MetadataFlags.PreserveMemberRefRids | MetadataFlags.PreserveStandAloneSigRids | MetadataFlags.PreserveEventRids | MetadataFlags.PreservePropertyRids | MetadataFlags.PreserveTypeSpecRids | MetadataFlags.PreserveMethodSpecRids | MetadataFlags.PreserveUSOffsets | MetadataFlags.PreserveBlobOffsets | MetadataFlags.PreserveExtraSignatureData);
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
