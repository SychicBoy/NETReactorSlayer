using System;

namespace NETReactorSlayer.Core.Helper;

[Serializable]
public class InvalidMethodBody : Exception
{
    public InvalidMethodBody()
    {
    }

    public InvalidMethodBody(string msg)
        : base(msg)
    {
    }
}