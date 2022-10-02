using System.Collections.Generic;
using dnlib.DotNet.Emit;

namespace NETReactorSlayer.De4dot
{
    public class PushedArgs
    {
        public PushedArgs(int numArgs)
        {
            _nextIndex = numArgs - 1;
            _args = new List<Instruction>(numArgs);
            for (var i = 0; i < numArgs; i++)
                _args.Add(null);
        }

        public void Add(Instruction instr) => _args[_nextIndex--] = instr;

        public void Set(int i, Instruction instr) => _args[i] = instr;

        internal void Pop() => _args[++_nextIndex] = null;

        public Instruction Get(int i)
        {
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