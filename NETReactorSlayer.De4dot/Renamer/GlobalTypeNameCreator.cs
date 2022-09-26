namespace NETReactorSlayer.De4dot.Renamer;

public class GlobalTypeNameCreator : TypeNameCreator
{
    public GlobalTypeNameCreator(ExistingNames existingNames) : base(existingNames)
    {
    }

    protected override NameCreator CreateNameCreator(string prefix)
    {
        return base.CreateNameCreator("G" + prefix);
    }
}