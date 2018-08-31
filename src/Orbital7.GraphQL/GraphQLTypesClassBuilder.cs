using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbital7.GraphQL
{
    // Useful: https://jsoneditoronline.org/
    //         https://github.com/graphql/graphql-js/blob/master/src/type/introspection.js
    //         http://graphql.org/learn/

    public class GraphQLTypesClassBuilder
    {
        public List<GraphQLType> ParseTypes(
            string schemaQueryResult)
        {
            var list = new List<GraphQLType>();

            dynamic model = JsonConvert.DeserializeObject(schemaQueryResult);
            foreach (var modelType in model.data.__schema.types)
            {
                var type = new GraphQLType()
                {
                    Name = modelType.name,
                    Description = modelType.description,
                };

                if (modelType.fields != null)
                {
                    foreach (var modelField in modelType.fields)
                    {
                        var field = new GraphQLField()
                        {
                            Name = modelField.name,
                            TypeName = modelField.type.name,
                            TypeKind = modelField.type.kind,
                        };

                        if (modelField.type.ofType != null)
                        {
                            field.OfTypeName = modelField.type.ofType.name;
                            field.OfTypeKind = modelField.type.ofType.kind;
                        }

                        type.Fields.Add(field);
                    }

                    type.Fields = (from x in type.Fields
                                   orderby x.Name
                                   select x).ToList();
                }

                if (modelType.inputFields != null)
                {
                    foreach (var modelField in modelType.inputFields)
                    {
                        var field = new GraphQLField()
                        {
                            Name = modelField.name,
                            TypeName = modelField.type.name,
                            TypeKind = modelField.type.kind,
                        };

                        if (modelField.type.ofType != null)
                        {
                            field.OfTypeName = modelField.type.ofType.name;
                            field.OfTypeKind = modelField.type.ofType.kind;
                        }

                        type.InputFields.Add(field);
                    }

                    type.InputFields = (from x in type.InputFields
                                        orderby x.Name
                                        select x).ToList();
                }

                if (modelType.enumValues != null)
                {
                    foreach (var modelEnumValue in modelType.enumValues)
                        type.EnumValues.Add(modelEnumValue.name.ToString());
                }

                if (modelType.interfaces != null)
                {
                    foreach (var modelInterface in modelType.interfaces)
                        type.Interfaces.Add(modelInterface.name.ToString());
                }

                if (modelType.possibleTypes != null)
                {
                    foreach (var modelPossibleType in modelType.possibleTypes)
                        type.PossibleTypes.Add(modelPossibleType.name.ToString());
                }

                list.Add(type);
            }

            var numRange = (from x in list
                            where x.Name == "Numrange"
                            select x).FirstOrDefault();
            numRange.Fields = new List<GraphQLField>()
            {
                new GraphQLField()
                {
                    Name = "start",
                    TypeName = "Decimal",
                    TypeKind = "SCALAR",
                },
                new GraphQLField()
                {
                    Name = "end",
                    TypeName = "Decimal",
                    TypeKind = "SCALAR",
                },
            };

            // For items with possible types, create interfaces.
            var containsPossibleTypesList = (from x in list
                                             where x.PossibleTypes.Count > 0
                                             select x).ToList();
            foreach (var type in containsPossibleTypesList)
            {
                foreach (var possibleTypeName in type.PossibleTypes)
                {
                    var targetType = (from x in list
                                      where x.Name == possibleTypeName
                                      select x).FirstOrDefault();

                    if (!targetType.Interfaces.Contains(type.Name))
                        targetType.Interfaces.Add(type.Name);
                }
            }

            return (from x in list
                    orderby x.Name
                    select x).ToList();
        }

        public GraphQLObjectModel CoalesceToModel(
            List<GraphQLType> types)
        {
            var model = new GraphQLObjectModel();

            var interfaceNames = (from x in types
                                  from y in x.Interfaces
                                  orderby y
                                  select y).Distinct().ToList();

            var intrinsicTypeNames = (from x in types
                                      where x.Fields.Count == 0 &&
                                            x.InputFields.Count == 0 &&
                                            x.EnumValues.Count == 0 &&
                                            x.PossibleTypes.Count == 0
                                      orderby x.Name
                                      select x.Name).ToList();

            var inputTypes = (from x in types
                               where x.InputFields.Count > 0
                               orderby x.Name
                               select x).ToList();

            var inputTypesWithFields = (from x in inputTypes
                                         where x.Fields.Count > 0
                                         select x).ToList();
            if (inputTypesWithFields.Count > 0)
                throw new Exception("Unexpected input type with data fields");

            // Update Interface names to start with "I".
            var typesContainingInterfaces = (from x in types
                                             where x.Interfaces.Count > 0
                                             select x).ToList();
            foreach (var type in typesContainingInterfaces)
                type.Interfaces = (from x in type.Interfaces
                                   let y = "I" + x
                                   select y).ToList();

            // Update field names.
            foreach (var field in (from x in types
                                   from y in x.Fields
                                   where interfaceNames.Contains(y.TypeName) ||
                                         interfaceNames.Contains(y.OfTypeName)
                                   select y).ToList())
            {
                if (interfaceNames.Contains(field.TypeName))
                    field.TypeName = "I" + field.TypeName;

                if (interfaceNames.Contains(field.OfTypeName))
                    field.OfTypeName = "I" + field.OfTypeName;
            }

            // Update field names.
            foreach (var field in (from x in types
                                   from y in x.InputFields
                                   where interfaceNames.Contains(y.TypeName) ||
                                         interfaceNames.Contains(y.OfTypeName)
                                   select y).ToList())
            {
                if (interfaceNames.Contains(field.TypeName))
                    field.TypeName = "I" + field.TypeName;

                if (interfaceNames.Contains(field.OfTypeName))
                    field.OfTypeName = "I" + field.OfTypeName;
            }

            foreach (var type in types)
            {
                if (!type.Name.StartsWith("__"))
                {
                    if (type.EnumValues.Count > 0)
                    {
                        model.Enums.Add(type);
                    }
                    else if (interfaceNames.Contains(type.Name))
                    {
                        type.Name = "I" + type.Name;
                        model.Interfaces.Add(type);
                    }
                    else if (type.InputFields.Count > 0)
                    {
                        model.InputObjects.Add(type);
                    }
                    else if (!intrinsicTypeNames.Contains(type.Name))
                    {
                        model.Objects.Add(type);
                    }
                }
            }

            return model;
        }

        public string WriteModelToClass(
            GraphQLObjectModel model, 
            string typesNamespace)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using Newtonsoft.Json;");
            sb.AppendLine();
            sb.AppendFormat("namespace {0}", typesNamespace).AppendLine();
            sb.AppendLine("{");

            WriteEnumTypes(model.Enums, sb);
            WriteInterfaceTypes(model.Interfaces, model.Enums, sb);
            WriteObjectTypes(model.Objects, model.Enums, sb);

            sb.AppendLine("}");
            return sb.ToString();
        }

        private void WriteEnumTypes(
            List<GraphQLType> types, 
            StringBuilder sb)
        {
            foreach (var type in types)
            {
                WriteDescription(type.Description, sb);
                sb.AppendFormat("\tpublic enum {0}", type.Name).AppendLine();
                sb.AppendLine("\t{");
                foreach (var value in type.EnumValues)
                    sb.AppendFormat("\t\t{0},", value).AppendLine();
                sb.AppendLine("\t}");
                sb.AppendLine();
            }
        }

        private void WriteInterfaceTypes(
            List<GraphQLType> interfaces, 
            List<GraphQLType> enumTypes,
            StringBuilder sb)
        {
            foreach (var type in interfaces)
            {
                WriteDescription(type.Description, sb);
                sb.AppendFormat("\tpublic interface {0}", type.Name).AppendLine();
                sb.AppendLine("\t{");
                WriteTypeFields(type.Fields, enumTypes, sb);
                sb.AppendLine("\t}");
                sb.AppendLine();
            }
        }

        private void WriteObjectTypes(
            List<GraphQLType> objects, 
            List<GraphQLType> enumTypes,
            StringBuilder sb)
        {
            foreach (var type in objects)
            {
                WriteDescription(type.Description, sb);

                string interfaces = null;
                if (type.Interfaces.Count > 0)
                {
                    bool isFirst = true;
                    interfaces = " : ";
                    foreach (var interfaceName in type.Interfaces)
                    {
                        if (!isFirst)
                            interfaces += ", ";
                        interfaces += interfaceName;
                        isFirst = false;
                    }
                }

                sb.AppendFormat("\tpublic class {0}{1}", type.Name, interfaces).AppendLine();
                sb.AppendLine("\t{");
                WriteTypeFields(type.Fields, enumTypes, sb, "public ");

                if (type.ContainsField("name"))
                    WriteToString("Name", sb);
                else if (type.ContainsField("value"))
                    WriteToString("Value", sb);
                else if (type.ContainsField("id"))
                    WriteToString("Id", sb);

                sb.AppendLine("\t}");
                sb.AppendLine();
            }
        }

        private void WriteToString(
            string propertyName, 
            StringBuilder sb)
        {
            sb.AppendLine();
            sb.AppendLine("\t\tpublic override string ToString()");
            sb.AppendLine("\t\t{");
            sb.AppendFormat("\t\t\treturn this.{0};", propertyName).AppendLine();
            sb.AppendLine("\t\t}");
        }

        private void WriteTypeFields(
            List<GraphQLField> fields, 
            List<GraphQLType> enumTypes,
            StringBuilder sb, 
            string modifier = null)
        {
            bool isFirst = true;

            foreach (var field in fields)
            {
                if (!isFirst)
                    sb.AppendLine();

                var fieldType = GetFieldDatatype(field.TypeName, enumTypes);
                if (String.IsNullOrEmpty(fieldType))
                {
                    if (field.TypeKind == "LIST")
                        fieldType = String.Format("List<{0}>", GetFieldDatatype(field.OfTypeName, enumTypes));
                }
                sb.AppendFormat("\t\t[JsonProperty(\"{0}\")]", field.Name).AppendLine();
                sb.AppendFormat("\t\t{0}{1} {2}", modifier, fieldType, field.Name.CapitalizeFirstLetter()).AppendLine(" { get; set; }");

                isFirst = false;
            }
        }

        private string GetFieldDatatype(
            string fieldTypeKind,
            List<GraphQLType> enumTypes)
        {
            if (String.IsNullOrEmpty(fieldTypeKind))
                return null;

            switch (fieldTypeKind)
            {
                case "ID":
                case "JSON":
                case "String":
                    return "string";

                case "Int":
                    return "int?";

                case "Price":
                case "Decimal":
                    return "decimal?";

                case "Boolean":
                    return "bool?";

                case "Date":
                case "DateTime":
                case "NaiveDateTime":
                    return "DateTime?";

                default:
                    var isEnumType = (from x in enumTypes
                                      where x.Name == fieldTypeKind
                                      select x).FirstOrDefault() != null;
                    if (isEnumType)
                        return fieldTypeKind + "?";
                    else
                        return fieldTypeKind;
            }
        }

        private void WriteDescription(
            string description, 
            StringBuilder sb)
        {
            if (!String.IsNullOrEmpty(description))
            {
                sb.AppendLine("\t/// <summary>");
                sb.AppendFormat("\t/// {0}", description.Replace("\r", "").Replace("\n", "")).AppendLine();
                sb.AppendLine("\t/// </summary>");
            }
        }
    }
}
