using System.Collections.Generic;

namespace NETReactorSlayer.De4dot.Renamer.AsmModules
{
    public class MethodNameGroup
    {
        public void Add(MMethodDef method) => Methods.Add(method);

        public void Merge(MethodNameGroup other)
        {
            if (this == other)
                return;
            Methods.AddRange(other.Methods);
        }

        public bool HasNonRenamableMethod()
        {
            foreach (var method in Methods)
                if (!method.Owner.HasModule)
                    return true;
            return false;
        }

        public bool HasInterfaceMethod()
        {
            foreach (var method in Methods)
                if (method.Owner.TypeDef.IsInterface)
                    return true;
            return false;
        }

        public bool HasGetterOrSetterPropertyMethod()
        {
            foreach (var method in Methods)
            {
                if (method.Property == null)
                    continue;
                var prop = method.Property;
                if (method == prop.GetMethod || method == prop.SetMethod)
                    return true;
            }

            return false;
        }

        public bool HasAddRemoveOrRaiseEventMethod()
        {
            foreach (var method in Methods)
            {
                if (method.Event == null)
                    continue;
                var evt = method.Event;
                if (method == evt.AddMethod || method == evt.RemoveMethod || method == evt.RaiseMethod)
                    return true;
            }

            return false;
        }

        public bool HasProperty()
        {
            foreach (var method in Methods)
                if (method.Property != null)
                    return true;
            return false;
        }

        public bool HasEvent()
        {
            foreach (var method in Methods)
                if (method.Event != null)
                    return true;
            return false;
        }

        public override string ToString() => $"{Methods.Count} -- {(Methods.Count > 0 ? Methods[0].ToString() : "")}";

        public int Count => Methods.Count;

        public List<MMethodDef> Methods { get; } = new List<MMethodDef>();
    }
}