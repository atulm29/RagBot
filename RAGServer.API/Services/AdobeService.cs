using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using RAGSERVERAPI.Models;

namespace RAGSERVERAPI.Services;

public interface IAdobeService
{
    Task<string> GetAccessToken();
    Task<RepoResponse<CreateAsset>> CreateAsset();
    Task<RepoResponse<PdfUploadResponse>> UploadPdf(string fileUrl, string fileName, string directoryPath);
}

public class AdobeService : IAdobeService
{
    private readonly HttpClient _http;
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;

    public AdobeService(HttpClient httpClient, ILogger logger, IConfiguration configuration)
    {
        _http = httpClient;
        _http.Timeout = TimeSpan.FromMinutes(60);
        _logger = logger;
        _configuration = configuration;
    }
    public async Task<string> GetAccessToken()
    {
        try
        {
            var url = "https://pdf-services-ue1.adobe.io/token";
            var body = $"&client_id={_configuration["AppSettings:AdobeApiKey"]}&client_secret={_configuration["AppSettings:AdobeClientSecretKey"]}";
            var content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");
            var resp = await _http.PostAsync(url, content);
            if (!resp.IsSuccessStatusCode)
                return string.Empty;
            var json = await resp.Content.ReadAsStringAsync();
            var token = JsonConvert.DeserializeObject<AdobeTokenResponse>(json);
            return token?.AccessToken ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogLocationWithException("AdobeService:GetAccessToken: ", ex);
            return string.Empty;
        }
    }
    public async Task<RepoResponse<CreateAsset>> CreateAsset()
    {
        var result = new RepoResponse<CreateAsset>();
        try
        {
            var url = "https://pdf-services-ue1.adobe.io/assets";
            var payload = new { mediaType = "application/pdf" };
            var json = JsonConvert.SerializeObject(payload);
            var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");
            req.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                await GetAccessToken()
            );
            req.Headers.Add("x-api-key", _configuration["AppSettings:AdobeApiKey"]);
            var resp = await _http.SendAsync(req);
            if (!resp.IsSuccessStatusCode)
            {
                return Error<CreateAsset>("Adobe Create Asset");
            }
            var data = await resp.Content.ReadAsStringAsync();
            result.Data = JsonConvert.DeserializeObject<CreateAsset>(data);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogLocationWithException("AdobeService:CreateAsset: ", ex);
            return Error<CreateAsset>("Adobe Create Asset");
        }
    }
    private async Task<RepoResponse<bool>> CheckFileUploadStatus(string assetId)
    {
        var result = new RepoResponse<bool>();
        try
        {
            var url = $"https://pdf-services-ue1.adobe.io/assets/{assetId}";
            var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessToken());
            req.Headers.Add("x-api-key", _configuration["AppSettings:AdobeApiKey"]);
            var resp = await _http.SendAsync(req);
            if (!resp.IsSuccessStatusCode)
                return Error<bool>("Adobe CheckFileUploadStatus");
            var json = await resp.Content.ReadAsStringAsync();
            dynamic obj = JsonConvert.DeserializeObject(json);
            result.Data = obj.downloadUri != null;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogLocationWithException("AdobeService:CheckFileUploadStatus: ", ex);
            return Error<bool>("Adobe CheckFileUploadStatus");
        }
    }
    private async Task<RepoResponse<string>> ExtractPDFContent(string assetId)
    {
        var result = new RepoResponse<string>();
        try
        {
            var payload = new
            {
                assetID = assetId,
                getCharBounds = false,
                includeStyling = false,
                elementsToExtract = new[] { "text", "tables" },
                tableOutputFormat = "xlsx",
                renditionsToExtract = new[] { "tables", "figures" },
            };

            var json = JsonConvert.SerializeObject(payload);
            var req = new HttpRequestMessage(HttpMethod.Post, "https://pdf-services-ue1.adobe.io/operation/extractpdf");

            req.Content = new StringContent(json, Encoding.UTF8, "application/json");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessToken());
            req.Headers.Add("x-api-key", _configuration["AppSettings:ExtractPDFContent"]);
            var resp = await _http.SendAsync(req);
            if (resp.StatusCode != HttpStatusCode.Created)
                return Error<string>("ExtractPDFContent");
            var location = resp.Headers.Location?.ToString();
            result.Data = location;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogLocationWithException("AdobeService:GetAccessToken: ", ex);
            return Error<string>("ExtractPDFContent");
        }
    }
    private async Task<RepoResponse<CheckPdfExtractStatusResponse>> CheckPdfExtractStatus(string location)
    {
        var result = new RepoResponse<CheckPdfExtractStatusResponse>();
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Get, location);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessToken());
            req.Headers.Add("x-api-key", _configuration["AppSettings:AdobeApiKey"]);
            var resp = await _http.SendAsync(req);
            if (!resp.IsSuccessStatusCode)
                return Error<CheckPdfExtractStatusResponse>("CheckPdfExtractStatus");
            var json = await resp.Content.ReadAsStringAsync();
            result.Data = JsonConvert.DeserializeObject<CheckPdfExtractStatusResponse>(json);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogLocationWithException("AdobeService:CheckPdfExtractStatus: ", ex);
            return Error<CheckPdfExtractStatusResponse>("CheckPdfExtractStatus");
        }
    }
    public async Task<RepoResponse<PdfUploadResponse>> UploadPdf(string fileUrl, string fileName, string directoryPath)
    {
        var result = new RepoResponse<PdfUploadResponse>();
        try
        {
            // 1. Upload File
            var upload = await UploadPdfToAdobe(fileUrl);
            if (upload.IsRepoError)
                return Error<PdfUploadResponse>("Adobe Upload");
            string assetId = upload.Data.AssetID;
            // 2. Wait for upload status
            RepoResponse<bool> status;
            do
            {
                status = await CheckFileUploadStatus(assetId);
                if (status.IsRepoError)
                    return Error<PdfUploadResponse>("Upload File Status");
            } while (!status.Data);

            // 3. Extract Content
            var extractReq = await ExtractPDFContent(assetId);
            if (extractReq.IsRepoError)
                return Error<PdfUploadResponse>("Extract Content");

            string location = extractReq.Data;

            // 4. Wait extraction status
            RepoResponse<CheckPdfExtractStatusResponse> extractStatus;
            do
            {
                extractStatus = await CheckPdfExtractStatus(location);
                if (extractStatus.IsRepoError)
                    return Error<PdfUploadResponse>("Check Extraction Status");
            } while (extractStatus.Data.Status != "done");

            // 5. Download JSON
            string downloadUrl = extractStatus.Data.Content.DownloadUri;
            string dir = Path.Combine(directoryPath, "Resource");

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string jsonFilePath = Path.Combine(
                dir,
                Path.GetFileNameWithoutExtension(fileName) + ".json"
            );

            new WebClient().DownloadFile(downloadUrl, jsonFilePath);

            var final = new PdfUploadResponse();
            final.PdfJson = File.ReadAllText(jsonFilePath);

            var pdfJson = JsonConvert.DeserializeObject<PDFJson>(final.PdfJson);

            final.PdfText = string.Join("\n", pdfJson.Elements.Select(x => x.Text));

            result.Data = final;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogLocationWithException("AdobeService:UploadPdf: ", ex);
            return Error<PdfUploadResponse>("UploadPdf");
        }
    }

    // 🔥 Helper for error response
    private RepoResponse<T> Error<T>(string name)
    {
        return new RepoResponse<T>
        {
            IsRepoError = true,
            ErrorDetails = new List<CustomErrorDetails>
            {
                new CustomErrorDetails { Name = name, Reason = "Something went wrong. Try again." },
            },
        };
    }

    // 🔥 Upload PDF (PUT)
    private async Task<RepoResponse<CreateAsset>> UploadPdfToAdobe(string filePath)
    {
        var result = new RepoResponse<CreateAsset>();
        try
        {
            var asset = await CreateAsset();
            if (asset.IsRepoError)
                return asset;

            string uploadUrl = asset.Data.UploadUri;
            byte[] fileBytes = File.ReadAllBytes(filePath);
            var req = new HttpRequestMessage(HttpMethod.Put, uploadUrl);
            req.Content = new ByteArrayContent(fileBytes);
            req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            var resp = await _http.SendAsync(req);
            if (!resp.IsSuccessStatusCode)
                return Error<CreateAsset>("UploadPdfToAdobe");
            result.Data = new CreateAsset { AssetID = asset.Data.AssetID };
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogLocationWithException("AdobeService:UploadPdfToAdobe: ", ex);
            return Error<CreateAsset>("UploadPdfToAdobe");
        }
    }
}
