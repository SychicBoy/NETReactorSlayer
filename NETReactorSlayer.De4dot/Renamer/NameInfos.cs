using System.Collections.Generic;

namespace NETReactorSlayer.De4dot.Renamer;

public class NameInfos
{
    public void Add(string name, NameCreator nameCreator) => _nameInfos.Add(new NameInfo(name, nameCreator));

    public NameCreator Find(string typeName)
    {
        foreach (var nameInfo in _nameInfos)
            if (typeName.Contains(nameInfo.Name))
                return nameInfo.NameCreator;

        return null;
    }

    private readonly IList<NameInfo> _nameInfos = new List<NameInfo>();

    private class NameInfo
    {
        public NameInfo(string name, NameCreator nameCreator)
        {
            Name = name;
            NameCreator = nameCreator;
        }

        public readonly string Name;
        public readonly NameCreator NameCreator;
    }
}