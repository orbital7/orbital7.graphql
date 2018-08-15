using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Orbital7.GraphQL
{
    public class GraphQLSchemaInterrogator
    {
        public async Task<string> ExecuteGraphQLSchemaQueryAsync(
            string url,
            string apiToken)
        {
            return await ExecuteGraphQLQueryAsync(
                url,
                apiToken,
                Properties.Resources.GraphQLSchemaQuery);
        }

        private async Task<string> ExecuteGraphQLQueryAsync(
            string url,
            string apiToken,
            string graphQLQuery)
        {
            // Create the request.
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.Headers["Authorization"] = apiToken;
            request.KeepAlive = true;

            // Set the query.
            var encoding = new UTF8Encoding();
            byte[] queryBytes = encoding.GetBytes(graphQLQuery);
            request.ContentLength = queryBytes.Length;
            var requestStream = request.GetRequestStream();
            requestStream.Write(queryBytes, 0, queryBytes.Length);

            try
            {
                using (var webResponse = await request.GetResponseAsync())
                {
                    return await webResponse.ReadAsStringAsync();
                }
            }
            catch (WebException webException)
            {
                throw new Exception(await webException.Response.ReadAsStringAsync());
            }
        }
    }
}
