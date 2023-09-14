// See manage GitHub Token https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens
// https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens#keeping-your-personal-access-tokens-secure

using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Abstractions;
using System.Net.Http.Headers;

using GraphQL.Client.Http;

using Microsoft.Extensions.DependencyInjection;
using GraphQL.Client.Serializer.SystemTextJson;
using System.Text;
using GraphQL;
using System.Text.Json;

Console.WriteLine("Registration");

const string GITHUB_API_URL = "https://api.github.com/graphql";
string TOKEN_PLAN = Environment.GetEnvironmentVariable("GITHUB_API_TOKEN") ?? string.Empty;
string TOKEN = Convert.ToBase64String(
            Encoding.UTF8.GetBytes(TOKEN_PLAN));

var serviceCollection = new ServiceCollection();


var options = new GraphQLHttpClientOptions
{
    EndPoint = new Uri(GITHUB_API_URL),
};

string QUERY = @"
                {
  repository(owner: ""weknow-network"", name: ""Event-Source-Backbone"") {
    createdAt
    forkCount
  }
}";

GraphQLRequest QUERY_REQUEST = new GraphQLRequest
{
    Query = QUERY,
    Variables = new
    {
        owner = "weknow-network",
        name = "Event-Source-Backbone"
    }
};

serviceCollection
    .AddSingleton<IGraphQLWebsocketJsonSerializer, SystemTextJsonSerializer>()
    .AddSingleton(options)
    .AddSingleton<IGraphQLClient>(sp =>
    {
        var serializer = sp.GetRequiredService<IGraphQLWebsocketJsonSerializer>();
        var opt = sp.GetRequiredService<GraphQLHttpClientOptions>();
        var graphClient = new GraphQLHttpClient(opt, serializer);
        var auth = new AuthenticationHeaderValue("Basic", TOKEN);
        graphClient.HttpClient.DefaultRequestHeaders.Authorization = auth;
        return graphClient;
    });

IServiceProvider services = serviceCollection.BuildServiceProvider();
IGraphQLClient client = services.GetRequiredService<IGraphQLClient>();

GraphQLResponse<JsonElement> r = await client.SendQueryAsync<JsonElement>(QUERY, variables: new
{
    owner = "weknow-network",
    name = "Event-Source-Backbone"
});

Console.WriteLine(r.Data.AsIndentString());

Console.ReadKey(true);
