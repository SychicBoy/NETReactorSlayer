using System.Linq;
using dnlib.DotNet;
using System.IO;
using System.IO.Compression;
using NETReactorSlayer.Core.Utils;

namespace NETReactorSlayer.Core.Protections
{
    class CosturaFody
    {
        public static void Execute()
        {
            long count = 0L;
			foreach (Resource resource in Context.Module.Resources)
			{
                try
                {
					if (!(resource is EmbeddedResource embeddedResource)) continue;
					if (!embeddedResource.Name.EndsWith(".compressed")) continue;
					Remover.ResourceToRemove.Add(embeddedResource);
					count += 1L;
					using (Stream resourceStream = embeddedResource.CreateReader().AsStream())
					{
						using (DeflateStream deflateStream = new DeflateStream(resourceStream, CompressionMode.Decompress))
						{
							using (MemoryStream memoryStream = new MemoryStream())
							{
								deflateStream.CopyTo(memoryStream);
								try
								{
									memoryStream.Position = 0L;
									File.WriteAllBytes($"{Context.FileDir}\\{GetAssemblyName(memoryStream.ToArray(), false)}.dll", memoryStream.ToArray());
								}
								catch
								{
									File.WriteAllBytes($"{Context.FileDir}\\{embeddedResource.Name.Replace(".compressed", "").Replace("costura.", "")}", memoryStream.ToArray());
								}
								memoryStream.Close();
								deflateStream.Close();
							}
						}
					}
                }
                catch { }
			}
            try
            {
                for (int i = 0; i < Context.Module.GlobalType.FindStaticConstructor().Body.Instructions.ToList().Count; i++)
                {
					if(Context.Module.GlobalType.FindStaticConstructor().Body.Instructions[i].Operand.ToString().Contains("Costura.AssemblyLoader::Attach()"))
                    {
						Context.Module.GlobalType.FindStaticConstructor().Body.Instructions.RemoveAt(i);
						break;
					}
				}
            }
            catch { }
			if(count > 0L)
				Logger.Info((int)count + " Embedded assemblies dumped (Costura.Fody).");
			else
				Logger.Warn("Couldn't find any embedded assembly (Costura.Fody).");
		}

		public static string GetAssemblyName(byte[] data, bool FullName)
		{
			try
			{
				using (ModuleDefMD module = ModuleDefMD.Load(data))
				{
					if (FullName) return module.Assembly.FullName;
					else return module.Assembly.Name;
				}
			}
			catch
			{
				return null;
			}
		}
	}
}
