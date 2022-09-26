namespace NETReactorSlayer.De4dot.Renamer;

public class GenericParamNameCreator : NameCreatorCounter
{
    public override string Create()
    {
        if (Num < Names.Length)
            return Names[Num++];
        return $"T{Num++}";
    }

    private static readonly string[] Names = { "T", "U", "V", "W", "X", "Y", "Z" };
}