using System.Collections.Generic;
using de4dot.blocks;

namespace NETReactorSlayer.De4dot.Renamer.AsmModules
{
    public class MethodDefDict : MethodDefDict<MMethodDef>
    {
        public IEnumerable<MMethodDef> GetSorted() => DictHelper.GetSorted(GetValues());

        public void Add(MMethodDef methodDef) => Add(methodDef.MethodDef, methodDef);
    }
}