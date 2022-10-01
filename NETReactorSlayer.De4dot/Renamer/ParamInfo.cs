using NETReactorSlayer.De4dot.Renamer.AsmModules;

namespace NETReactorSlayer.De4dot.Renamer;

public class ParamInfo
{
    public ParamInfo(MParamDef paramDef)
    {
        OldName = paramDef.ParameterDef.Name;
        NewName = paramDef.ParameterDef.Name;
    }

    public bool GotNewName() => OldName != NewName;

    public string NewName;
    public string OldName;
}