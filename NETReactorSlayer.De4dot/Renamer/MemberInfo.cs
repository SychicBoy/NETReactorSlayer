using NETReactorSlayer.De4dot.Renamer.AsmModules;

namespace NETReactorSlayer.De4dot.Renamer;

public class MemberInfo
{
    public MemberInfo(Ref memberRef)
    {
        MemberRef = memberRef;
        OldFullName = memberRef.MemberRef.FullName;
        OldName = memberRef.MemberRef.Name.String;
        NewName = memberRef.MemberRef.Name.String;
    }

    public void Rename(string newTypeName)
    {
        Renamed = true;
        NewName = newTypeName;
    }

    public bool GotNewName()
    {
        return OldName != NewName;
    }

    public override string ToString()
    {
        return $"O:{OldFullName} -- N:{NewName}";
    }

    protected Ref MemberRef;
    public string NewName;
    public string OldFullName;
    public string OldName;
    public bool Renamed;
    public string SuggestedName;
}