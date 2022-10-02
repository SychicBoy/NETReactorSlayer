namespace NETReactorSlayer.De4dot
{
    public abstract class Option
    {
        protected Option(string shortName, string longName, string description)
        {
            if (shortName != null)
                ShortName = ShortnamePrefix + shortName;
            if (longName != null)
                LongName = LongnamePrefix + longName;
            Description = description;
        }

        public abstract bool Set(string val, out string error);
        private const string LongnamePrefix = "--";
        private const string ShortnamePrefix = "-";
        public object Default { get; protected set; }
        public string Description { get; }
        public string LongName { get; }

        public string ShortName { get; }
    }
}