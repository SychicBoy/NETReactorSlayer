using dnlib.DotNet;

namespace NETReactorSlayer.De4dot.Renamer.AsmModules
{
    public class MParamDef
    {
        public MParamDef(Parameter parameterDef, int index)
        {
            ParameterDef = parameterDef;
            Index = index;
        }

        public int Index { get; }
        public bool IsHiddenThisParameter => ParameterDef.IsHiddenThisParameter;
        public Parameter ParameterDef { get; set; }
    }
}