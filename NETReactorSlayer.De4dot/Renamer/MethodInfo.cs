using NETReactorSlayer.De4dot.Renamer.AsmModules;

namespace NETReactorSlayer.De4dot.Renamer
{
    public class MethodInfo : MemberInfo
    {
        public MethodInfo(MMethodDef methodDef) : base(methodDef)
        {
        }

        public MMethodDef MethodDef => (MMethodDef)MemberRef;
    }
}