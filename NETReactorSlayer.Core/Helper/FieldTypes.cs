using System.Collections.Generic;
using dnlib.DotNet;

namespace NETReactorSlayer.Core.Helper
{
    public class FieldTypes : StringCounts
    {
        public FieldTypes(IEnumerable<FieldDef> fields) => Initialize(fields);

        private void Initialize(IEnumerable<FieldDef> fields)
        {
            if (fields == null)
                return;
            foreach (var field in fields)
            {
                var type = field.FieldSig.GetFieldType();
                if (type != null)
                    Add(type.FullName);
            }
        }
    }
}