using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbital7.GraphQL
{
    public class GraphQLObjectModel
    {
        public List<GraphQLType> Enums { get; set; } = new List<GraphQLType>();

        public List<GraphQLType> Interfaces { get; set; } = new List<GraphQLType>();

        public List<GraphQLType> Objects { get; set; } = new List<GraphQLType>();

        public List<GraphQLType> InputObjects { get; set; } = new List<GraphQLType>();
    }
}
