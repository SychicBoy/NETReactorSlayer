namespace NETReactorSlayer.De4dot
{
    public interface IDeobfuscatorInfo
    {
        IDeobfuscator CreateDeobfuscator();
        string Name { get; }
    }
}