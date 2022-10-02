using System.Collections.Generic;

namespace NETReactorSlayer.De4dot
{
    public class NameRegexes
    {
        public NameRegexes() : this("")
        {
        }

        public NameRegexes(string regex) => Set(regex);

        public void Set(string regexesString)
        {
            Regexes = new List<NameRegex>();
            if (regexesString != "")
                foreach (var regex in regexesString.Split(RegexSeparatorChar))
                    Regexes.Add(new NameRegex(regex));
        }

        public bool IsMatch(string s)
        {
            foreach (var regex in Regexes)
                if (regex.IsMatch(s))
                    return regex.MatchValue;

            return DefaultValue;
        }

        public override string ToString()
        {
            var s = "";
            for (var i = 0; i < Regexes.Count; i++)
            {
                if (i > 0)
                    s += RegexSeparatorChar;
                s += Regexes[i].ToString();
            }

            return s;
        }

        public const char RegexSeparatorChar = '&';

        public bool DefaultValue { get; set; }
        public IList<NameRegex> Regexes { get; private set; }
    }
}