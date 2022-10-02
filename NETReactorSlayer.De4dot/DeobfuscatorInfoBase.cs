namespace NETReactorSlayer.De4dot
{
    public abstract class DeobfuscatorInfoBase : IDeobfuscatorInfo
    {
        protected DeobfuscatorInfoBase(string nameRegex) =>
            ValidNameRegex = new NameRegexOption(null, MakeArgName("name"), "Valid name regex pattern",
                nameRegex ?? DeobfuscatorBase.DefaultValidNameRegex);

        protected string MakeArgName(string name) => name;

        public abstract IDeobfuscator CreateDeobfuscator();

        protected NameRegexOption ValidNameRegex;

        public abstract string Name { get; }
    }
}