using dnlib.DotNet;

namespace NETReactorSlayer.De4dot
{
    public class DeobfuscatorContext : IDeobfuscatorContext
    {
        private static ITypeDefOrRef GetNonGenericTypeRef(ITypeDefOrRef typeRef)
        {
            var ts = typeRef as TypeSpec;
            if (ts == null)
                return typeRef;
            var gis = ts.TryGetGenericInstSig();
            return gis?.GenericType == null ? typeRef : gis.GenericType.TypeDefOrRef;
        }

        public TypeDef ResolveType(ITypeDefOrRef type)
        {
            if (type == null)
                return null;
            type = GetNonGenericTypeRef(type);

            switch (type)
            {
                case TypeDef typeDef:
                    return typeDef;
                case TypeRef tr:
                    return tr.Resolve();
                default:
                    return null;
            }
        }
    }
}