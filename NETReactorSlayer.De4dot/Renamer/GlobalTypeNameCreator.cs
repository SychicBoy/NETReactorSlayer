namespace NETReactorSlayer.De4dot.Renamer;

public class GlobalTypeNameCreator : TypeNameCreator
{
    public GlobalTypeNameCreator(ExistingNames existingNames) : base(existingNames)
    {
    }

    public override NameCreator CreateNameCreator(string prefix) => base.CreateNameCreator("G" + prefix);
}