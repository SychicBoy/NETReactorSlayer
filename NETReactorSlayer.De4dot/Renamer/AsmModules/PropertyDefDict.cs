using System.Collections.Generic;
using de4dot.blocks;

namespace NETReactorSlayer.De4dot.Renamer.AsmModules;

public class PropertyDefDict : PropertyDefDict<MPropertyDef>
{
    public IEnumerable<MPropertyDef> GetSorted()
    {
        return DictHelper.GetSorted(GetValues());
    }

    public void Add(MPropertyDef propDef)
    {
        Add(propDef.PropertyDef, propDef);
    }
}