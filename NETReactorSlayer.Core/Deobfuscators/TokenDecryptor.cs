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
using de4dot.blocks;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Linq;

namespace NETReactorSlayer.Core.Deobfuscators
{
    class TokenDecryptor : IDeobfuscator
    {
        public void Execute()
        {
            float count = 0L;
            Find();
            if (typeDef == null || typeMethod == null || fieldMethod == null)
            {
                Logger.Warn("Couldn't found any encrypted token.");
                return;
            }
            foreach (TypeDef type in DeobfuscatorContext.Module.GetTypes())
            {
                foreach (MethodDef method in (from x in type.Methods where x.HasBody && x.Body.HasInstructions select x))
                    count += Deobfuscate(method);
            }
            if (count == 0L)
                Logger.Warn("Couldn't found any encrypted token.");
            else
                Logger.Done($"{(int)count} Tokens decrypted.");
        }

        private void Find()
        {
            foreach (TypeDef type in (from x in DeobfuscatorContext.Module.GetTypes() where !x.HasProperties && !x.HasEvents && x.Fields.Count != 0 select x))
            {
                foreach (FieldDef field in (from x in type.Fields where x.FieldType.FullName.Equals("System.ModuleHandle") select x))
                {
                    MethodDef FieldMethod = null;
                    MethodDef TypeMethod = null;
                    foreach (MethodDef method in (from x in type.Methods where x.MethodSig != null && x.MethodSig.Params.Count.Equals(1) && x.MethodSig.Params[0].GetElementType() == ElementType.I4 select x).ToArray<MethodDef>())
                    {
                        if (method.MethodSig.RetType.GetFullName().Equals("System.RuntimeTypeHandle"))
                            TypeMethod = method;
                        else if (method.MethodSig.RetType.GetFullName().Equals("System.RuntimeFieldHandle"))
                            FieldMethod = method;
                    }
                    if (TypeMethod == null || FieldMethod == null) continue;
                    typeDef = type;
                    typeMethod = TypeMethod;
                    fieldMethod = FieldMethod;
                    break;
                }
            }
        }

        public long Deobfuscate(MethodDef myMethod)
        {
            long count = 0L;
            if (typeDef == null) return 0L;
            GenericParamContext gpContext = GenericParamContext.Create(myMethod);
            for (int i = 0; i < myMethod.Body.Instructions.Count; i++)
            {
                if (myMethod.Body.Instructions[i].OpCode.Code.Equals(Code.Ldc_I4) && myMethod.Body.Instructions[i + 1].OpCode.Code == Code.Call)
                {
                    if (!(myMethod.Body.Instructions[i + 1].Operand is IMethod method) || !default(SigComparer).Equals(typeDef, method.DeclaringType)) continue;
                    MethodDef methodDef = DotNetUtils.GetMethod(DeobfuscatorContext.Module, method);
                    if (methodDef == null) continue;
                    if (methodDef == typeMethod || methodDef == fieldMethod)
                    {
                        uint token = (uint)((int)myMethod.Body.Instructions[i].Operand);
                        myMethod.Body.Instructions[i] = OpCodes.Nop.ToInstruction();
                        myMethod.Body.Instructions[i + 1] = new Instruction(OpCodes.Ldtoken, DeobfuscatorContext.Module.ResolveToken(token, gpContext) as ITokenOperand);
                        count += 1L;
                    }
                }
            }
            return count;
        }

        private TypeDef typeDef;
        private MethodDef typeMethod;
        private MethodDef fieldMethod;
    }
}
