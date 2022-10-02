namespace NETReactorSlayer.De4dot.Renamer
{
    public abstract class NameCreatorCounter : INameCreator
    {
        public NameCreatorCounter Merge(NameCreatorCounter other)
        {
            if (Num < other.Num)
                Num = other.Num;
            return this;
        }

        public abstract string Create();

        protected int Num;
    }
}