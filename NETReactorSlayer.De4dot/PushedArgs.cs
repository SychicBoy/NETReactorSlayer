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
using dnlib.DotNet.Emit;

namespace NETReactorSlayer.De4dot {
    public class PushedArgs {
        public PushedArgs(int numArgs) {
            _nextIndex = numArgs - 1;
            _args = new List<Instruction>(numArgs);
            for (var i = 0; i < numArgs; i++)
                _args.Add(null);
        }

        public void Add(Instruction instr) => _args[_nextIndex--] = instr;

        public void Set(int i, Instruction instr) => _args[i] = instr;

        internal void Pop() => _args[++_nextIndex] = null;

        public Instruction Get(int i) {
            if (0 <= i && i < _args.Count)
                return _args[i];
            return null;
        }

        public Instruction GetEnd(int i) => Get(_args.Count - 1 - i);

        private readonly List<Instruction> _args;
        private int _nextIndex;

        public bool CanAddMore => _nextIndex >= 0;
        public int NumValidArgs => _args.Count - (_nextIndex + 1);
    }
}