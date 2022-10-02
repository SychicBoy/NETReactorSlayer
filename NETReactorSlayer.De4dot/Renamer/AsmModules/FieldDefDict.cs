using System.Collections.Generic;
using de4dot.blocks;

namespace NETReactorSlayer.De4dot.Renamer.AsmModules
{
    public class FieldDefDict : FieldDefDict<MFieldDef>
    {
        public IEnumerable<MFieldDef> GetSorted() => DictHelper.GetSorted(GetValues());

        public void Add(MFieldDef fieldDef) => Add(fieldDef.FieldDef, fieldDef);
    }
}