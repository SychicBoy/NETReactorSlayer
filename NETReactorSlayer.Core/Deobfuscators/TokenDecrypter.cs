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

using System.Linq;
using de4dot.blocks;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace NETReactorSlayer.Core.Deobfuscators;

internal class TokenDecrypter : IStage
{
    public void Execute()
    {
        TypeDef typeDef = null;
        MethodDef fieldMethod = null;
        MethodDef typeMethod = null;
        float count = 0L;
        foreach (var type in Context.Module.GetTypes().Where(x => !x.HasProperties && !x.HasEvents && x.Fields.Count != 0))
            foreach (var _ in type.Fields.Where(x => x.FieldType.FullName.Equals("System.ModuleHandle")))
            {
                foreach (var method in type.Methods.Where(x => x.MethodSig != null &&
                              x.MethodSig.Params.Count.Equals(1) &&
                              x.MethodSig.Params[0].GetElementType() == ElementType.I4)
                             .ToList())
                    if (method.MethodSig.RetType.GetFullName().Equals("System.RuntimeTypeHandle"))
                        typeMethod = method;
                    else if (method.MethodSig.RetType.GetFullName().Equals("System.RuntimeFieldHandle"))
                        fieldMethod = method;
                if (typeMethod == null || fieldMethod == null) continue;
                typeDef = type;
                goto Continue;
            }
        Continue:
        if (typeDef != null)
            foreach (var type in Context.Module.GetTypes())
                foreach (var method in type.Methods.Where(x => x.HasBody && x.Body.HasInstructions))
                {
                    var gpContext = GenericParamContext.Create(method);
                    for (var i = 0; i < method.Body.Instructions.Count; i++)
                        try
                        {
                            if (method.Body.Instructions[i].OpCode.Code.Equals(Code.Ldc_I4) &&
                                method.Body.Instructions[i + 1].OpCode.Code == Code.Call)
                            {
                                if (!(method.Body.Instructions[i + 1].Operand is IMethod iMethod) ||
                                    !default(SigComparer).Equals(typeDef, iMethod.DeclaringType)) continue;
                                var methodDef = DotNetUtils.GetMethod(Context.Module, iMethod);
                                if (methodDef == null) continue;
                                if (methodDef == typeMethod || methodDef == fieldMethod)
                                {
                                    var token = (uint)(int)method.Body.Instructions[i].Operand;
                                    method.Body.Instructions[i] = OpCodes.Nop.ToInstruction();
                                    method.Body.Instructions[i + 1] = new Instruction(OpCodes.Ldtoken,
                                        Context.Module.ResolveToken(token, gpContext) as ITokenOperand);
                                    count += 1L;
                                }
                            }
                        }
                        catch { }
                }
        if (count == 0L)
            Logger.Warn("Couldn't found any encrypted token.");
        else
            Logger.Done($"{(int)count} Tokens decrypted.");
    }
}