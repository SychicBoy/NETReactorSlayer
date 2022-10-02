using System.Collections.Generic;
using dnlib.DotNet;

namespace NETReactorSlayer.De4dot.Renamer.AsmModules
{
    public class MGenericParamDef : Ref
    {
        public MGenericParamDef(GenericParam genericParameter, int index)
            : base(genericParameter, null, index)
        {
        }

        public static List<MGenericParamDef> CreateGenericParamDefList(IEnumerable<GenericParam> parameters)
        {
            var list = new List<MGenericParamDef>();
            if (parameters == null)
                return list;
            var i = 0;
            foreach (var param in parameters)
                list.Add(new MGenericParamDef(param, i++));
            return list;
        }

        public GenericParam GenericParam => (GenericParam)MemberRef;
    }
}