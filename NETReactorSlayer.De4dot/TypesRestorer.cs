using System;
using dnlib.DotNet;

namespace NETReactorSlayer.De4dot
{
    public class TypesRestorer : TypesRestorerBase
    {
        public TypesRestorer(ModuleDef module)
            : base(module)
        {
        }

        protected override bool IsValidType(IGenericParameterProvider gpp, TypeSig type)
        {
            if (type == null)
                return false;
            if (type.IsValueType)
                return false;
            if (type.ElementType == ElementType.Object)
                return false;
            return base.IsValidType(gpp, type);
        }

        protected override bool IsUnknownType(object o)
        {
            if (o is Parameter arg)
                return arg.Type.GetElementType() == ElementType.Object;

            if (o is FieldDef field)
                return field.FieldSig.GetFieldType().GetElementType() == ElementType.Object;

            if (o is TypeSig sig)
                return sig.ElementType == ElementType.Object;

            throw new ApplicationException($"Unknown type: {o.GetType()}");
        }
    }
}