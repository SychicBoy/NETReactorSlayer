namespace NETReactorSlayer.De4dot
{
    public class Tuple<T1, T2>
    {
        public override bool Equals(object obj)
        {
            var other = obj as Tuple<T1, T2>;
            if (other == null)
                return false;
            return Item1.Equals(other.Item1) && Item2.Equals(other.Item2);
        }

        public override int GetHashCode() => Item1.GetHashCode() + Item2.GetHashCode();

        public override string ToString() => "<" + Item1 + "," + Item2 + ">";

        public T1 Item1 { get; }
        public T2 Item2 { get; }
    }
}