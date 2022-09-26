using dnlib.DotNet;

namespace NETReactorSlayer.De4dot;

public class DeobfuscatorInfo : DeobfuscatorInfoBase
{
    public DeobfuscatorInfo(ModuleDefMD moduleDefMd, bool renameShort)
        : base(DefaultRegex)
    {
        _module = moduleDefMd;
        _renameShort = renameShort;
    }

    public override IDeobfuscator CreateDeobfuscator()
    {
        return new Deobfuscator(_module, new Deobfuscator.Options
        {
            ValidNameRegex = ValidNameRegex.Get(),
            RestoreTypes = true,
            RemoveNamespaces = true,
            RenameShort = _renameShort
        });
    }

    private readonly ModuleDefMD _module;
    private readonly bool _renameShort;

    private const string DefaultRegex = DeobfuscatorBase.DefaultAsianValidNameRegex;
    public const string ShortNameRegex = @"!^[A-Za-z0-9]{2,3}$";

    public const string TheName = ".NET Reactor";

    public override string Name => TheName;
}