using System;

namespace NETReactorSlayer.De4dot
{
    public class NameRegexOption : Option
    {
        public NameRegexOption(string shortName, string longName, string description, string val)
            : base(shortName, longName, description) =>
            Default = _val = new NameRegexes(val);

        public override bool Set(string newVal, out string error)
        {
            try
            {
                var regexes = new NameRegexes();
                regexes.Set(newVal);
                _val = regexes;
            }
            catch (ArgumentException)
            {
                error = $"Could not parse regex '{newVal}'";
                return false;
            }

            error = "";
            return true;
        }

        public NameRegexes Get() => _val;

        private NameRegexes _val;
    }
}