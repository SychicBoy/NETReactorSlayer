using System.Collections.Generic;
using de4dot.blocks;

namespace NETReactorSlayer.De4dot.Renamer.AsmModules;

public class TypeDefDict : TypeDefDict<MTypeDef>
{
    public IEnumerable<MTypeDef> GetSorted() => DictHelper.GetSorted(GetValues());

    public void Add(MTypeDef typeDef) => Add(typeDef.TypeDef, typeDef);
}