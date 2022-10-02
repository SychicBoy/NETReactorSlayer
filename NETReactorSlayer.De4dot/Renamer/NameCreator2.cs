namespace NETReactorSlayer.De4dot.Renamer
{
    public class NameCreator2 : NameCreatorCounter
    {
        public NameCreator2(string prefix)
            : this(prefix, 0)
        {
        }

        public NameCreator2(string prefix, int num)
        {
            _prefix = prefix;
            Num = num;
        }

        public override string Create()
        {
            string rv;
            if (Num == 0)
                rv = _prefix;
            else
                rv = _prefix + Separator + Num;
            Num++;
            return rv;
        }

        private readonly string _prefix;

        private const string Separator = "_";
    }
}