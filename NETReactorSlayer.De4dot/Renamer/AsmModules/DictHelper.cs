using System.Collections.Generic;

namespace NETReactorSlayer.De4dot.Renamer.AsmModules
{
    public static class DictHelper
    {
        public static IEnumerable<T> GetSorted<T>(IEnumerable<T> values) where T : Ref
        {
            var list = new List<T>(values);
            list.Sort((a, b) => a.Index.CompareTo(b.Index));
            return list;
        }
    }
}