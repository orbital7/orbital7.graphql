using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbital7.GraphQL
{
    public class GraphQLType
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public List<GraphQLField> Fields { get; set; } = new List<GraphQLField>();

        public List<GraphQLField> InputFields { get; set; } = new List<GraphQLField>();

        public List<string> EnumValues { get; set; } = new List<string>();

        public List<string> Interfaces { get; set; } = new List<string>();

        public List<string> PossibleTypes { get; set; } = new List<string>();

        public override string ToString()
        {
            return this.Name;
        }

        public bool ContainsField(string fieldName)
        {
            return (from x in this.Fields
                    where x.Name == fieldName
                    select x).FirstOrDefault() != null;
        }
    }
}
