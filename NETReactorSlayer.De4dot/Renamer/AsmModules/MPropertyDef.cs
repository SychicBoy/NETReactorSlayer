using System.Collections.Generic;
using dnlib.DotNet;

namespace NETReactorSlayer.De4dot.Renamer.AsmModules
{
    public class MPropertyDef : Ref
    {
        public MPropertyDef(PropertyDef propertyDef, MTypeDef owner, int index)
            : base(propertyDef, owner, index)
        {
        }

        public IEnumerable<MethodDef> MethodDefs()
        {
            if (PropertyDef.GetMethod != null)
                yield return PropertyDef.GetMethod;
            if (PropertyDef.SetMethod != null)
                yield return PropertyDef.SetMethod;
            if (PropertyDef.OtherMethods != null)
                foreach (var m in PropertyDef.OtherMethods)
                    yield return m;
        }

        public bool IsVirtual()
        {
            foreach (var method in MethodDefs())
                if (method.IsVirtual)
                    return true;
            return false;
        }

        public bool IsItemProperty()
        {
            if (GetMethod != null && GetMethod.VisibleParameterCount >= 1)
                return true;
            if (SetMethod != null && SetMethod.VisibleParameterCount >= 2)
                return true;
            return false;
        }

        public MMethodDef GetMethod { get; set; }
        public PropertyDef PropertyDef => (PropertyDef)MemberRef;
        public MMethodDef SetMethod { get; set; }
    }
}