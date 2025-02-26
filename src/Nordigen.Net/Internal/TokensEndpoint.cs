﻿namespace Nordigen.Net.Internal;

using Responses;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

internal class TokensEndpoint : ITokensEndpoint, IEndpoint
{
    internal const string HttpClientName = "tokens";
    private readonly ISerializer _serializer;
    private readonly HttpClient _client;
    private readonly NordigenApiOptions _options;

    public TokensEndpoint(IHttpClientFactory factory, ISerializer serializer, NordigenApiOptions options)
    {
        _client = factory.CreateClient(HttpClientName);
        _options = options;
        _serializer = serializer;
    }

    public async Task<NOneOf<Token, Error>> Get(CancellationToken cancellationToken)
    {
        // var credentials = new { secret_id = _options.SecretId, secret_key = _options.SecretKey };
        var credentials = new Dictionary<string, string>
        {
            ["secret_id"] = _options.SecretId, ["secret_key"] = _options.SecretKey,
        };

        var content = new FormUrlEncodedContent(credentials);
        var message = await _client.PostAsync("api/v2/token/new/", content, cancellationToken);
        return message.IsSuccessStatusCode
            ? (NOneOf<Token, Error>)_serializer.Deserialize<Token>(await message.Content.ReadAsStringAsync())
            : _serializer.Deserialize<Error>(await message.Content.ReadAsStringAsync());
    }

    public async Task<NOneOf<Token, Error>> Refresh(string refresh, CancellationToken cancellationToken = default)
    {
        var credentials = new { refresh };

        var content = new StringContent(_serializer.Serialize(credentials), Encoding.UTF8, Constants.ContentMediaType);

        var message = await _client.PostAsync("api/v2/token/refresh/", content, cancellationToken);
        return message.IsSuccessStatusCode
            ? (NOneOf<Token, Error>)_serializer.Deserialize<Token>(await message.Content.ReadAsStringAsync())
            : _serializer.Deserialize<Error>(await message.Content.ReadAsStringAsync());
    }
}
