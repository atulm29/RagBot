using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using RAGSERVERAPI.DTOs;
using System.Text;
using System.Net;

namespace RAGSERVERAPI.Services;

public interface IVertexAIService
{
    Task<VertexAIGenerateResponse> GenerateTextAsync(VertexAIGenerateRequest request, string textModel);
    Task<float[]> GenerateEmbeddingAsync(string text, string embeddingModel);
    IAsyncEnumerable<string> GenerateTextStreamAsync(VertexAIGenerateRequest request, string textModel);
    Task<float[]> GetEmbeddingAsync(string text, string embeddingModel);
}

public class VertexAIService : IVertexAIService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly string _projectId;
    private readonly string _location;
    private readonly SemaphoreSlim _rateLimitSemaphore;
    private readonly int _maxRetries;
    private readonly int _initialRetryDelayMs;
    private readonly HttpClient _httpClient;

    public VertexAIService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _projectId = configuration["GCP:ProjectId"]!;
        _location = configuration["GCP:_Location"] ?? "us-central1";

        // Rate limiting configuration
        var maxConcurrentRequests = int.Parse(configuration["GCP:RateLimits:MaxConcurrentRequests"] ?? "1");
        _maxRetries = int.Parse(configuration["GCP:RateLimits:MaxRetries"] ?? "8");
        _initialRetryDelayMs = int.Parse(configuration["GCP:RateLimits:InitialRetryDelayMs"] ?? "2000");

        _rateLimitSemaphore = new SemaphoreSlim(maxConcurrentRequests, maxConcurrentRequests);
        _httpClient = new HttpClient();
    }

    private async Task AddAuthHeaderAsync(HttpClient client)
    {
        string accessToken = await GoogleCredential
            .GetApplicationDefault()
            .CreateScoped("https://www.googleapis.com/auth/cloud-platform")
            .UnderlyingCredential
            .GetAccessTokenForRequestAsync();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
    }

    // ⭐ FIXED: Use text generation model, not embedding model
    public async Task<VertexAIGenerateResponse> GenerateTextAsync(VertexAIGenerateRequest request, string textModel)
    {
        var client = _httpClientFactory.CreateClient("VertexAI");
        await AddAuthHeaderAsync(client);

        var endpoint =
            $"https://{_location}-aiplatform.googleapis.com/v1/projects/{_projectId}/locations/{_location}/publishers/google/models/{textModel}:generateContent";

        var response = await client.PostAsJsonAsync(endpoint, request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<VertexAIGenerateResponse>()
            ?? throw new Exception("Empty response from Vertex AI");
    }

    public async IAsyncEnumerable<string> GenerateTextStreamAsync(VertexAIGenerateRequest request, string textModel)
    {
        var endpoint =
            $"https://{_location}-aiplatform.googleapis.com/v1/projects/{_projectId}/locations/{_location}/publishers/google/models/{textModel}:streamGenerateContent";

        var client = _httpClientFactory.CreateClient("VertexAI");
        await AddAuthHeaderAsync(client);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = JsonContent.Create(request)
        };

        var response = await client.SendAsync(httpRequest);
        response.EnsureSuccessStatusCode();

        // Read the entire response as string
        var responseContent = await response.Content.ReadAsStringAsync();
        response.Dispose();

        // Parse as JSON array
        List<VertexAIGenerateResponse> chunks = null;

        try
        {
            chunks = JsonSerializer.Deserialize<List<VertexAIGenerateResponse>>(responseContent);
        }
        catch (JsonException ex)
        {
            //_logger.LogError(ex, "Failed to parse streaming response: {Content}", responseContent);
            yield break;
        }

        if (chunks == null || chunks.Count == 0)
        {
            yield break;
        }

        // Yield each text chunk
        foreach (var chunk in chunks)
        {
            var text = chunk?.Candidates?.FirstOrDefault()?
                .Content?.Parts?.FirstOrDefault()?.Text;

            if (!string.IsNullOrWhiteSpace(text))
            {
                yield return text;
            }
        }
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, string embeddingModel)
    {
        await _rateLimitSemaphore.WaitAsync();

        try
        {
            return await GenerateEmbeddingWithRetryAsync(text, embeddingModel);
        }
        finally
        {
            _ = Task.Delay(1000).ContinueWith(_ => _rateLimitSemaphore.Release());
        }
    }

    public async Task<float[]> GetEmbeddingAsync(string text, string embeddingModel)
    {
        try
        {
            var accessToken = await GetAccessTokenAsync();
            var endpoint = $"https://{_location}-aiplatform.googleapis.com/v1/projects/{_projectId}/locations/{_location}/publishers/google/models/{embeddingModel}:predict";

            var requestBody = new
            {
                instances = new[]
                {
                    new { content = text }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Add("Authorization", $"Bearer {accessToken}");
            request.Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonDocument.Parse(responseContent);

            var predictions = jsonResponse.RootElement.GetProperty("predictions");
            var embeddings = predictions[0].GetProperty("embeddings");
            var values = embeddings.GetProperty("values");

            var embedding = new float[values.GetArrayLength()];
            int i = 0;
            foreach (var value in values.EnumerateArray())
            {
                embedding[i++] = value.GetSingle();
            }

            return embedding;
        }
        catch (Exception ex)
        {
            _logger.LogLocationWithException("Error getting embedding from Vertex AI", ex);
            throw;
        }
    }

    private async Task<float[]> GenerateEmbeddingWithRetryAsync(string text, string embeddingModel)
    {
        int retryCount = 0;

        while (retryCount <= _maxRetries)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("VertexAI");
                await AddAuthHeaderAsync(client);

                var endpoint =
                    $"https://{_location}-aiplatform.googleapis.com/v1/projects/{_projectId}/locations/{_location}/publishers/google/models/{embeddingModel}:predict";

                var request = new
                {
                    instances = new[]
                    {
                        new { content = text }
                    }
                };

                var response = await client.PostAsJsonAsync(endpoint, request);

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    retryCount++;

                    if (retryCount > _maxRetries)
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        throw new RateLimitExceededException($"Max retries ({_maxRetries}) exceeded due to rate limiting. Last error: {err}");
                    }

                    var delayMs = CalculateRetryDelay(retryCount);
                    _logger.LogInfo($"Rate limit hit (429). Retrying in {delayMs}ms (attempt {retryCount}/{_maxRetries})");

                    await Task.Delay(delayMs);
                    continue;
                }

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Vertex AI API error: {response.StatusCode} - {err}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<EmbeddingResponse>(responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? throw new Exception("Empty embedding response");

                if (result.Predictions == null || result.Predictions.Count == 0)
                    throw new Exception($"Predictions missing. Full response: {responseContent}");

                var prediction = result.Predictions[0];

                if (prediction.Embeddings?.Values == null || prediction.Embeddings.Values.Count == 0)
                    throw new Exception($"Embedding vector missing. Full response: {responseContent}");

                return prediction.Embeddings.Values.Select(v => (float)v).ToArray();
            }
            catch (RateLimitExceededException)
            {
                throw;
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("429") || ex.Message.Contains("TooManyRequests"))
            {
                retryCount++;

                if (retryCount > _maxRetries)
                {
                    throw new RateLimitExceededException($"Max retries ({_maxRetries}) exceeded due to rate limiting", ex);
                }

                var delayMs = CalculateRetryDelay(retryCount);
                _logger.LogInfo($"Rate limit exception. Retrying in {delayMs}ms (attempt {retryCount}/{_maxRetries})");

                await Task.Delay(delayMs);
            }
        }

        throw new Exception($"Failed to generate embedding after {_maxRetries} retries");
    }

    private int CalculateRetryDelay(int retryCount)
    {
        var exponentialDelay = _initialRetryDelayMs * Math.Pow(2, retryCount - 1);
        var jitter = new Random().Next(0, 1000);
        return (int)Math.Min(exponentialDelay + jitter, 30000);
    }

    private async Task<string> GetAccessTokenAsync()
    {
        try
        {
            var credential = await GoogleCredential.GetApplicationDefaultAsync();
            var scopedCredential = credential.CreateScoped("https://www.googleapis.com/auth/cloud-platform");
            var token = await ((ITokenAccess)scopedCredential).GetAccessTokenForRequestAsync();
            return token;
        }
        catch (Exception ex)
        {
            _logger.LogLocationWithException("Error getting access token", ex);
            throw;
        }
    }
}
