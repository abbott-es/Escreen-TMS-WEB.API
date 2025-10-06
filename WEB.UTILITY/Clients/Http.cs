using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using LanguageExt;

namespace WEB.UTILITY.Clients
{
    public static class Http
    {
        public static async Task<TResponse> PostFromUrlEncoded<TResponse>(
            this HttpClient client,
            string uri,
            IDictionary<string, string> inputs)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new FormUrlEncodedContent(inputs)
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.SendAsync(request);
            return await Deserialize<TResponse>(response);
        }

        public static async Task<HttpResponseMessage> Post(
           this HttpClient client,
           string uri,
           params (string, string)[] headers)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.AddHeaders(headers);
            return await client.SendAsync(request);
        }

        public static async Task<HttpResponseMessage> PostJson<TRequest>(
            this HttpClient client,
            string uri,
            TRequest body,
            params (string, string)[] headers)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.AddHeaders(headers);
            return await client.SendAsync(request);
        }

        public static async Task<Either<TError, TResponse>> PostJson<TResponse, TError, TRequest>(
            this HttpClient client,
            string uri,
            TRequest body,
            params (string, string)[] headers)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.AddHeaders(headers);

            var response = await client.SendAsync(request);
            return await Deserialize<TResponse, TError>(response);
        }

        public static async Task<Either<TError, TResponse>> PostJson<TResponse, TError>(
            this HttpClient client,
            string uri,
            params (string, string)[] headers)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.AddHeaders(headers);

            var response = await client.SendAsync(request);
            return await Deserialize<TResponse, TError>(response);
        }

        public static async Task<Either<TError, TResponse>> PostJsonWithBasicAuth<TResponse, TError>(
            this HttpClient client,
            string uri, string username, string password)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var authenticationString = $"{username}:{password}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.UTF8.GetBytes(authenticationString));
            request.Headers.Add("Authorization", "Basic " + base64EncodedAuthenticationString);

            var response = await client.SendAsync(request);
            return await Deserialize<TResponse, TError>(response);
        }

        public static async Task<TResponse> PostJson<TRequest, TResponse>(
           this HttpClient client,
           string uri,
           TRequest body,
           params (string, string)[] headers)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.AddHeaders(headers);

            var response = await client.SendAsync(request);
            return await Deserialize<TResponse>(response);
        }

        public static async Task<Either<TError, TResponse>> PostJson<TRequest, TResponse, TError>(
            this HttpClient client,
            string uri,
            TRequest body,
            params (string, string)[] headers)
        {
            string bodyJson = JsonSerializer.Serialize(body, new JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            });

            var request = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(bodyJson, Encoding.UTF8, "application/json")
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.AddHeaders(headers);

            var response = await client.SendAsync(request);
            return await Deserialize<TResponse, TError>(response);
        }

        public static async Task<HttpResponseMessage> Get(
          this HttpClient client,
          string uri,
          params (string, string)[] headers)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.AddHeaders(headers);
            return await client.SendAsync(request);
        }

        public static async Task<Either<TError, TResponse>> GetJson<TResponse, TError>(
            this HttpClient client,
            string uri,
            params (string, string)[] headers)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.AddHeaders(headers);

            var response = await client.SendAsync(request);
            return await Deserialize<TResponse, TError>(response);
        }

        public static async Task<Option<TResponse>> GetOptionalJson<TResponse>(
            this HttpClient client,
            string uri,
            params (string, string)[] headers)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.AddHeaders(headers);

            var response = await client.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return Option<TResponse>.None;

            var options = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
            };
            options.Converters.Add(new JsonStringEnumConverter());
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TResponse>(options);
        }

        public static async Task<Option<Stream>> GetStream(
            this HttpClient client,
            string uri,
            params (string, string)[] headers)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
            request.AddHeaders(headers);

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return Option<Stream>.None;

            return await response.Content.ReadAsStreamAsync();
        }

        public static async Task<TResponse> PutJson<TRequest, TResponse>(
            this HttpClient client,
            string uri,
            TRequest body,
            params (string, string)[] headers)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, uri)
            {
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.AddHeaders(headers);

            var response = await client.SendAsync(request);
            return await Deserialize<TResponse>(response);
        }

        private static async Task<Either<TError, TResponse>> Deserialize<TResponse, TError>(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.NotFound ||
                response.StatusCode == HttpStatusCode.BadRequest)
                return await response.Content.ReadFromJsonAsync<TError>();

            return await Deserialize<TResponse>(response);
        }

        private static async Task<TResponse> Deserialize<TResponse>(HttpResponseMessage response)
        {
            var options = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            };
            options.Converters.Add(new JsonStringEnumConverter());
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TResponse>(options);
        }

        private static void AddHeaders(this HttpRequestMessage request, (string, string)[] headers)
        {
            headers
                .ToList()
                .ForEach(h => request.Headers.Add(h.Item1, h.Item2));
        }
    }
}
