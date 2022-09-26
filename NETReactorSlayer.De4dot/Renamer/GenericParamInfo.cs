using NETReactorSlayer.De4dot.Renamer.AsmModules;

namespace NETReactorSlayer.De4dot.Renamer;

public class GenericParamInfo : MemberInfo
{
    public GenericParamInfo(MGenericParamDef genericParamDef) : base(genericParamDef)
    {
    }
}