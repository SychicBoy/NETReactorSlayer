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
using dnlib.DotNet.Emit;

namespace NETReactorSlayer.Core.Helper
{
    internal class CodeVirtualizationUtils
    {
        public static bool Detect()
        {
            var array = new string[]
            {
                "System.String",
                "System.Byte",
                "System.SByte",
                "System.Int16",
                "System.UInt16",
                "System.Int32",
                "System.UInt32",
                "System.Int64",
                "System.UInt64",
                "System.Single",
                "System.Double",
                "System.Boolean",
                "System.IntPtr",
                "System.UIntPtr",
                "System.Char"
            };

            foreach (var type in Context.Module.GetTypes())
            {
                foreach (var method in type.Methods.Where(x=> x.HasBody && x.Body.HasInstructions))
                {
                    try
                    {
                        if (method.Body.Instructions.Count(x => x.OpCode.Equals(OpCodes.Switch)) != 2)
                            continue;
                        if (method.Body.Instructions.Count(x => x.OpCode.Equals(OpCodes.Ldtoken)) < 15)
                            continue;
                        var operands = method.Body.Instructions
                            .Where(x => x.OpCode.Equals(OpCodes.Ldtoken) && x.Operand != null).Select(x => x.Operand.ToString()).ToList();
                        if (operands.Except(array).Any())
                            return true;
                    }
                    catch { }
                }
            }
            return false;
        }
    }
}
