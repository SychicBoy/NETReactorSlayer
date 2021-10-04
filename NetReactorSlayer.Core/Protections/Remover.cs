using dnlib.DotNet;
using dnlib.DotNet.Emit;
using NETReactorSlayer.Core.Utils;
using System.Collections.Generic;
using System.Linq;

namespace NETReactorSlayer.Core.Protections
{
    public class Remover
    {
        public static HashSet<MethodDef> MethodsToPatch = new HashSet<MethodDef>();
        public static HashSet<Resource> ResourceToRemove = new HashSet<Resource>();
        public static void Execute()
        {
            #region Patch Methods
            Instruction Ldnull = Instruction.Create(OpCodes.Ldnull);
            Instruction Ret = Instruction.Create(OpCodes.Ret);
            CilBody cliBody;
            foreach (var method in MethodsToPatch)
            {
                try
                {
                    MethodDef methodDef = Context.Module.Find(method.DeclaringType.FullName, false).FindMethod(method.Name);
                    if (!methodDef.HasReturnType)
                    {
                        cliBody = new CilBody();
                        cliBody.Instructions.Add(Ret);
                        methodDef.Body = cliBody;
                    }
                    else
                    {
                        cliBody = new CilBody();
                        cliBody.Instructions.Add(Ldnull);
                        cliBody.Instructions.Add(Ret);
                        methodDef.Body = cliBody;
                    }
                    if (methodDef.DeclaringType.FindStaticConstructor() != null)
                    {
                        cliBody = new CilBody();
                        cliBody.Instructions.Add(Ret);
                        methodDef.DeclaringType.FindStaticConstructor().Body = cliBody;
                    }
                }
                catch { }
            }
            #endregion
            #region Remove NoInline Attributes
            foreach (var type in Context.Module.GetTypes())
            {
                foreach (var method in type.Methods.ToArray<MethodDef>())
                {
                    method.IsNoInlining = false;
                    for (int i = 0; i < method.CustomAttributes.Count; i++)
                    {
                        try
                        {
                            var cattr = method.CustomAttributes[i];
                            if (cattr.TypeFullName != "System.Runtime.CompilerServices.MethodImplAttribute")
                                continue;
                            int options = 0;
                            if (!GetMethodImplOptions(cattr, ref options))
                                continue;
                            if (options != 0 && options != (int)MethodImplAttributes.NoInlining)
                                continue;
                            method.CustomAttributes.RemoveAt(i);
                            i--;
                        }
                        catch { }
                    }
                }
            }
            #endregion
            #region Remove Unused Resources
            foreach (var rrsource in ResourceToRemove)
            {
                Context.Module.Resources.Remove(Context.Module.Resources.Find(rrsource.Name));
            }
            #endregion
        }
        static bool GetMethodImplOptions(CustomAttribute cA, ref int value)
        {
            if (cA.IsRawBlob)
                return false;
            if (cA.ConstructorArguments.Count != 1)
                return false;
            if (cA.ConstructorArguments[0].Type.ElementType != ElementType.I2 &&
                cA.ConstructorArguments[0].Type.FullName != "System.Runtime.CompilerServices.MethodImplOptions")
                return false;
            var arg = cA.ConstructorArguments[0].Value;
            if (arg is short @int)
            {
                value = @int;
                return true;
            }
            if (arg is int int1)
            {
                value = int1;
                return true;
            }
            return false;
        }
    }
}
