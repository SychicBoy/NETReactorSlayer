using dnlib.DotNet;

namespace NETReactorSlayer.De4dot.Renamer.AsmModules
{
    public abstract class Ref
    {
        protected Ref(IMemberRef memberRef, MTypeDef owner, int index)
        {
            MemberRef = memberRef;
            Owner = owner;
            Index = index;
        }

        public override string ToString() => MemberRef?.ToString() ?? string.Empty;

        public readonly IMemberRef MemberRef;

        public int Index { get; set; }
        public MTypeDef Owner { get; set; }
    }
}