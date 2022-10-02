using System;
using System.Collections.Generic;

namespace NETReactorSlayer.De4dot.Renamer.AsmModules
{
    public class MethodNameGroups
    {
        public void Same(MMethodDef a, MMethodDef b) => Merge(Get(a), Get(b));

        public void Add(MMethodDef methodDef) => Get(methodDef);

        public MethodNameGroup Get(MMethodDef method)
        {
            if (!method.IsVirtual())
                throw new ApplicationException("Not a virtual method");
            if (!_methodGroups.TryGetValue(method, out var group))
            {
                _methodGroups[method] = group = new MethodNameGroup();
                group.Add(method);
            }

            return group;
        }

        private void Merge(MethodNameGroup a, MethodNameGroup b)
        {
            if (a == b)
                return;

            if (a.Count < b.Count) (a, b) = (b, a);

            a.Merge(b);
            foreach (var methodDef in b.Methods)
                _methodGroups[methodDef] = a;
        }

        public IEnumerable<MethodNameGroup> GetAllGroups() => Utils.Unique(_methodGroups.Values);

        private readonly Dictionary<MMethodDef, MethodNameGroup> _methodGroups =
            new Dictionary<MMethodDef, MethodNameGroup>();
    }
}