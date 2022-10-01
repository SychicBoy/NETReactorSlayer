namespace NETReactorSlayer.De4dot.Renamer;

public class NameCreator : NameCreatorCounter
{
    public NameCreator(string prefix) : this(prefix, 0)
    {
    }

    public NameCreator(string prefix, int num)
    {
        _prefix = prefix;
        Num = num;
    }

    public NameCreator Clone() => new NameCreator(_prefix, Num);

    public override string Create() => _prefix + Num++;

    private readonly string _prefix;
}