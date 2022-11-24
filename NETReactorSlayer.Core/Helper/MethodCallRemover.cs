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

using System.Collections.Generic;
using System.Linq;
using de4dot.blocks;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace NETReactorSlayer.Core.Helper
{
    internal class MethodCallRemover
    {
        public static long RemoveCalls(MethodDef methodToRem)
        {
            _methodRefInfos = new MethodDefAndDeclaringTypeDict<MethodDefAndDeclaringTypeDict<bool>>();
            foreach (var methodDef in Context.Module.GetTypes()
                         .SelectMany(type => type.Methods.Where(x => x.HasBody && x.Body.HasInstructions)))
                Add(methodDef, methodToRem);

            return Context.Module.GetTypes()
                .Sum(type => type.Methods.Where(x => x.HasBody && x.Body.HasInstructions)
                    .Sum(method => RemoveCalls(method, _methodRefInfos.Find(method))));
        }

        public static long RemoveCalls(List<MethodDef> methods)
        {
            _methodRefInfos = new MethodDefAndDeclaringTypeDict<MethodDefAndDeclaringTypeDict<bool>>();
            foreach (var type in Context.Module.GetTypes())
            foreach (var method in type.Methods.Where(x => x.HasBody && x.Body.HasInstructions))
            foreach (var methodToRem in methods)
                Add(method, methodToRem);

            return Context.Module.GetTypes().Sum(type =>
                type.Methods.Where(x => x.HasBody && x.Body.HasInstructions)
                    .Sum(method => RemoveCalls(method, _methodRefInfos.Find(method))));
        }

        #region Fields

        private static MethodDefAndDeclaringTypeDict<MethodDefAndDeclaringTypeDict<bool>> _methodRefInfos;

        #endregion

        #region Private Methods

        private static long RemoveCalls(MethodDef method, MethodDefDictBase<bool> info)
        {
            long count = 0;
            foreach (var instr in method.Body.Instructions)
                try
                {
                    if (instr.OpCode != OpCodes.Call)
                        continue;
                    if (instr.Operand is not IMethod destMethod)
                        continue;
                    if (!info.Find(destMethod))
                        continue;
                    instr.OpCode = OpCodes.Nop;
                    count++;
                } catch { }

            return count;
        }

        private static void Add(MethodDef method, MethodDef methodToBeRemoved)
        {
            if (method == null || methodToBeRemoved == null || !CheckMethod(methodToBeRemoved))
                return;
            var dict = _methodRefInfos.Find(method);
            if (dict == null)
                _methodRefInfos.Add(method, dict = new MethodDefAndDeclaringTypeDict<bool>());
            dict.Add(methodToBeRemoved, true);
        }

        private static bool CheckMethod(IMethod methodToBeRemoved) =>
            methodToBeRemoved.MethodSig.Params.Count == 0 &&
            methodToBeRemoved.MethodSig.RetType.ElementType ==
            ElementType.Void;

        #endregion
    }
}