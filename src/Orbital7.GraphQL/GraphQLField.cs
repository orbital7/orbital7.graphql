using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbital7.GraphQL
{
    public class GraphQLField
    {
        public string Name { get; set; }

        public string TypeKind { get; set; }

        public string TypeName { get; set; }

        public string OfTypeKind { get; set; }

        public string OfTypeName { get; set; }

        public override string ToString()
        {
            return this.Name + " (" + (this.TypeName ?? this.TypeKind + "[" + (this.OfTypeName ?? this.OfTypeKind) + "]") + ")";
        }
    }
}
