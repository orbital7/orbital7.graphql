# orbital7.graphql
.NET Standard 2.0 library for creating C# model classes for use with an external GraphQL API

Example usage:

// Deterine the GraphQL types schema.
var schemaIntrospector = new GraphQLSchemaInterrogator();
string schema = await schemaIntrospector.ExecuteGraphQLSchemaQueryAsync(
  "https://api.something.com/graphql",
  "MY_API_KEY");

// Create a C# class file for the GraphQL types.
var classBuilder = new GraphQLTypesClassBuilder();
var types = classBuilder.ParseTypes(schema);
var model = classBuilder.CoalesceToModel(types);
var classContents = classBuilder.WriteModelToClass(model, "Something.Api.Models");
System.IO.File.WriteAllText(@"C:\Temp\SomethingApiModels.cs", classContents);
