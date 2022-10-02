using System.Collections.Generic;
using dnlib.DotNet;

namespace NETReactorSlayer.De4dot.Renamer.AsmModules
{
    public class MEventDef : Ref
    {
        public MEventDef(EventDef eventDef, MTypeDef owner, int index)
            : base(eventDef, owner, index)
        {
        }

        public IEnumerable<MethodDef> MethodDefs()
        {
            if (EventDef.AddMethod != null)
                yield return EventDef.AddMethod;
            if (EventDef.RemoveMethod != null)
                yield return EventDef.RemoveMethod;
            if (EventDef.InvokeMethod != null)
                yield return EventDef.InvokeMethod;
            if (EventDef.OtherMethods != null)
                foreach (var m in EventDef.OtherMethods)
                    yield return m;
        }

        public bool IsVirtual()
        {
            foreach (var method in MethodDefs())
                if (method.IsVirtual)
                    return true;
            return false;
        }

        public MMethodDef AddMethod { get; set; }
        public EventDef EventDef => (EventDef)MemberRef;
        public MMethodDef RaiseMethod { get; set; }
        public MMethodDef RemoveMethod { get; set; }
    }
}