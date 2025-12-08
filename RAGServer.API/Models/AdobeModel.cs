using System.ComponentModel;
using Newtonsoft.Json;

namespace RAGSERVERAPI.Models;

public class PdfUpload
{
    public int UserId { get; set; }
    public int ClientId { get; set; }

    public IFormFile PdfFile { get; set; }
    public string FileName { get; set; }
    public string PdfName { get; set; }
    public string TemplateIds { get; set; }
    public int FormId { get; set; }
}

public class CreateAsset
{
    [JsonProperty("uploadUri")]
    public string UploadUri { get; set; }

    [JsonProperty("assetID")]
    public string AssetID { get; set; }
}

public class GetAssets
{
    public GetAssets() { }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("size")]
    public int Size { get; set; }

    [JsonProperty("downloadUri")]
    public string DownloadUri { get; set; }
}

public class AdobeTokenResponse
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }

    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonProperty("token_type")]
    public string TokenType { get; set; }
}

public class PdfUploadResponse
{
    public string PdfJson { get; set; }
    public string PdfText { get; set; }

    public string ZipFilePath { get; set; }
}

public class PdfExtractContent
{
    public string PdfJson { get; set; }
    public string Text { get; set; }
    public string PDfZipFilePath { get; set; }
    public string ZipFilePath { get; set; }
}

public class CheckPdfExtractStatusResponse
{
    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("content")]
    public Content Content { get; set; }

    [JsonProperty("resource")]
    public Resource Resource { get; set; }
}

public class Content
{
    [JsonProperty("assetID")]
    public string AssetID { get; set; }

    [JsonProperty("downloadUri")]
    public string DownloadUri { get; set; }
}

public class Resource
{
    [JsonProperty("assetID")]
    public string AssetID { get; set; }

    [JsonProperty("downloadUri")]
    public string DownloadUri { get; set; }
}

class PDFJson
{
    [JsonProperty("elements")]
    public List<Element> Elements { get; set; }
}

public class Element
{
    [JsonProperty("attributes")]
    public Attributes Attributes { get; set; }

    [JsonProperty("Bounds")]
    public List<double> Bounds { get; set; }

    [JsonProperty("filePaths")]
    public List<string> FilePaths { get; set; }

    [JsonProperty("ObjectID")]
    public int ObjectID { get; set; }

    [JsonProperty("Page")]
    public int Page { get; set; }

    [JsonProperty("Path")]
    public string Path { get; set; }

    [JsonProperty("Text")]
    public string Text { get; set; }

    [JsonProperty("TextSize")]
    public double? TextSize { get; set; }
}

public class Attributes
{
    [JsonProperty("BBox")]
    public List<double> BBox { get; set; }

    [JsonProperty("Placement")]
    public string Placement { get; set; }
    // Add other properties as needed
}

public class KnowledgeBaseForm
{
    public int FormId { get; set; }
    public int PDFDataId { get; set; }
    public string FormName { get; set; }
}

public class KnowledgeBaseFormListResponse
{
    public List<KnowledgeBaseForm> knowledgeBaseForms { get; set; }
}

public class RepoResponse<T>
{
    private bool _name;

    [DefaultValue(false)]
    public bool IsRepoError
    {
        get { return _name; }
        set { _name = value; }
    }
    public string ExceptionMessage { get; set; }
    public T Data { get; set; }
    public List<CustomErrorDetails> ErrorDetails { get; set; } = new List<CustomErrorDetails>();
}

public class CustomErrorDetails
{
    public string Name { get; set; } = "Exception";
    public string Reason { get; set; }
}
