namespace NETReactorSlayer.De4dot.Renamer
{
    public interface INameChecker
    {
        bool IsValidNamespaceName(string ns);
        bool IsValidTypeName(string name);
        bool IsValidMethodName(string name);
        bool IsValidPropertyName(string name);
        bool IsValidEventName(string name);
        bool IsValidFieldName(string name);
        bool IsValidGenericParamName(string name);
        bool IsValidMethodArgName(string name);
        bool IsValidMethodReturnArgName(string name);
        bool IsValidResourceKeyName(string name);
    }
}