
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RAGSERVERAPI.Models;
using RAGSERVERAPI.Repositories;

namespace RAGSERVERAPI.Services;

public interface ISearchService
{
    Task<SearchResponse> SearchAsync(SearchRequest request);
}

public class SearchService : ISearchService
{
    private readonly IVertexAIService _vertexAIService;
    private readonly IDocumentService _documentService;
    private readonly ILogger _logger;
    private readonly IConfigurationService _configurationService;
    public SearchService(IVertexAIService vertexAIService, IDocumentService documentService, ILogger logger, IConfigurationService configurationService)
    {
        _vertexAIService = vertexAIService;
        _documentService = documentService;
        _logger = logger;
        _configurationService = configurationService;
    }

    public async Task<SearchResponse> SearchAsync(SearchRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        var config = await _configurationService.GetRagConfigurationAsync();
        try
        {
            _logger.LogInfo($"Starting search for query: {request.Query}");

            // Get embedding for the query text
            var queryEmbedding = await _vertexAIService.GetEmbeddingAsync(request.Query, config.EmbeddingModel);
            _logger.LogInfo($"Generated embedding with {queryEmbedding.Length} dimensions");

            // Search for similar embeddings in the database
            var results = await _documentService.SearchByEmbeddingAsync(queryEmbedding, request);

            stopwatch.Stop();

            var response = new SearchResponse
            {
                Results = results,
                Query = request.Query,
                TotalResults = results.Count,
                QueryTime = stopwatch.Elapsed.TotalMilliseconds
            };

            _logger.LogInfo($"Search completed in {response.QueryTime}ms, found {response.TotalResults} results");

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogLocationWithException("SearchService: SearchAsync(): Error performing search", ex);
            throw;
        }
    }
}
