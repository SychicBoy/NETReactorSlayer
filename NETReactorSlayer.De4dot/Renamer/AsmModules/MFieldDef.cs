using dnlib.DotNet;

namespace NETReactorSlayer.De4dot.Renamer.AsmModules
{
    public class MFieldDef : Ref
    {
        public MFieldDef(FieldDef fieldDef, MTypeDef owner, int index) : base(fieldDef, owner, index)
        {
        }

        public FieldDef FieldDef => (FieldDef)MemberRef;
    }
}