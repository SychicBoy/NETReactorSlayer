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

using de4dot.blocks.cflow;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace NETReactorSlayer.Core.Helper
{
    public static class ArrayFinder
    {
        public static byte[] GetInitializedByteArray(MethodDef method, int arraySize)
        {
            var newarrIndex = FindNewarr(method, arraySize);
            return newarrIndex < 0 ? null : GetInitializedByteArray(arraySize, method, ref newarrIndex);
        }

        public static byte[] GetInitializedByteArray(int arraySize, MethodDef method, ref int newarrIndex)
        {
            var resultValueArray = GetInitializedArray(arraySize, method, ref newarrIndex, Code.Stelem_I1);

            var resultArray = new byte[resultValueArray.Length];
            for (var i = 0; i < resultArray.Length; i++)
            {
                if (resultValueArray[i] is not Int32Value intValue || !intValue.AllBitsValid())
                    return null;
                resultArray[i] = (byte)intValue.Value;
            }

            return resultArray;
        }

        public static Value[] GetInitializedArray(
            int arraySize, MethodDef method, ref int newarrIndex, Code stelemOpCode)
        {
            var resultValueArray = new Value[arraySize];

            var emulator = new InstructionEmulator(method);
            var theArray = new UnknownValue();
            emulator.Push(theArray);

            var instructions = method.Body.Instructions;
            int i;
            for (i = newarrIndex + 1; i < instructions.Count; i++)
            {
                var instr = instructions[i];
                if (instr.OpCode.FlowControl != FlowControl.Next)
                    break;
                if (instr.OpCode.Code == Code.Newarr)
                    break;
                switch (instr.OpCode.Code)
                {
                    case Code.Newarr:
                    case Code.Newobj:
                        goto done;

                    case Code.Stloc:
                    case Code.Stloc_S:
                    case Code.Stloc_0:
                    case Code.Stloc_1:
                    case Code.Stloc_2:
                    case Code.Stloc_3:
                    case Code.Starg:
                    case Code.Starg_S:
                    case Code.Stsfld:
                    case Code.Stfld:
                        if (emulator.Peek() == theArray && i != newarrIndex + 1 && i != newarrIndex + 2)
                            goto done;
                        break;
                }

                if (instr.OpCode.Code == stelemOpCode)
                {
                    var value = emulator.Pop();
                    var index = emulator.Pop() as Int32Value;
                    var array = emulator.Pop();
                    if (!ReferenceEquals(array, theArray) || index == null || !index.AllBitsValid())
                        continue;
                    if (0 <= index.Value && index.Value < resultValueArray.Length)
                        resultValueArray[index.Value] = value;
                }
                else
                    emulator.Emulate(instr);
            }

            done:
            if (i != newarrIndex + 1)
                i--;
            newarrIndex = i;

            return resultValueArray;
        }

        private static int FindNewarr(MethodDef method, int arraySize)
        {
            for (var i = 0;; i++)
            {
                if (!FindNewarr(method, ref i, out var size))
                    return -1;
                if (size == arraySize)
                    return i;
            }
        }

        public static bool FindNewarr(MethodDef method, ref int i, out int size)
        {
            var instructions = method.Body.Instructions;
            for (; i < instructions.Count; i++)
            {
                var instr = instructions[i];
                if (instr.OpCode.Code != Code.Newarr || i < 1)
                    continue;
                var ldci4 = instructions[i - 1];
                if (!ldci4.IsLdcI4())
                    continue;

                size = ldci4.GetLdcI4Value();
                return true;
            }

            size = -1;
            return false;
        }
    }
}