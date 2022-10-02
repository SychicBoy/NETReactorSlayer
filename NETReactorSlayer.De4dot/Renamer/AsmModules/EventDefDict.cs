using System.Collections.Generic;
using de4dot.blocks;

namespace NETReactorSlayer.De4dot.Renamer.AsmModules
{
    public class EventDefDict : EventDefDict<MEventDef>
    {
        public IEnumerable<MEventDef> GetSorted() => DictHelper.GetSorted(GetValues());

        public void Add(MEventDef eventDef) => Add(eventDef.EventDef, eventDef);
    }
}