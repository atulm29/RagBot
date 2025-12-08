using System.Text;
using Dapper;
using FluentValidation;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.AIPlatform.V1;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;
using NLog;
using RAGSERVERAPI.Middleware;
using RAGSERVERAPI.Models;
using RAGSERVERAPI.Repositories;
using RAGSERVERAPI.Services;
using RAGSERVERAPI.Validators;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";

if (builder.Environment.IsDevelopment())
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(int.Parse(port));
    });
}

ConfigurationManager configuration = builder.Configuration;
var jwtTokenConfig = builder.Configuration.GetSection("JwtTokenConfig").Get<JwtTokenConfig>() ?? throw new InvalidOperationException("TokenConfig section is missing in configuration.");
LogManager.Setup().LoadConfigurationFromFile(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
var gcpKeyPath = builder.Configuration["GCP:KeyFilePath"] ?? "./service-account.json";
Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", gcpKeyPath);
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var keyPath = config["GCP:KeyFilePath"] ?? "./service-account.json";
    var location = config["GCP:Location"] ?? "us-central1";
    var credential = GoogleCredential.FromFile(keyPath).CreateScoped("https://www.googleapis.com/auth/cloud-platform");
    var builderClient = new PredictionServiceClientBuilder
    {
        Credential = credential,
        Endpoint = $"{location}-aiplatform.googleapis.com",
    };
    return builderClient.Build();
});


// Get connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection is missing in configuration.");

// Register Npgsql data source with pgvector support
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.UseVector(); // Critical for pgvector support
var dataSource = dataSourceBuilder.Build();

// Register the data source
builder.Services.AddSingleton(dataSource);

// Register Dapper type handler for Vector
Dapper.SqlMapper.AddTypeHandler(new PgVectorHandler());
Dapper.SqlMapper.AddTypeHandler(new FloatArrayToVectorHandler());

// Register Dapper Context
builder.Services.AddSingleton<DataContext>();
builder.Services.AddSingleton(jwtTokenConfig);
builder.Services.AddSingleton<RAGSERVERAPI.Services.ILogger, LoggerService>();

// Register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IDocumentChunkRepository, DocumentChunkRepository>();
builder.Services.AddScoped<IEmbeddingRepository, EmbeddingRepository>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();
builder.Services.AddScoped<IConfigurationRepository, ConfigurationRepository>();

// Register Services
builder.Services.AddHttpClient<IAdobeService, AdobeService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IVertexAIService, VertexAIService>();
builder.Services.AddScoped<IGcsService, GcsService>();
builder.Services.AddScoped<IEmbeddingService, EmbeddingService>();
builder.Services.AddScoped<IRagService, RagService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();


// Register Validators
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();


builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks();

builder
    .Services.AddAuthentication(op =>
    {
        op.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        op.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtTokenConfig.Secret)),
            ValidateIssuer = true,
            ValidIssuer = jwtTokenConfig.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtTokenConfig.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
// Configure Kestrel server limits
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 104857600; // 100 MB
});
builder.Services.AddHttpClient("VertexAI", client =>
{
    client.Timeout = TimeSpan.FromMinutes(5);
});
var app = builder.Build();
app.Use(
    async (context, next) =>
    {
        var maxRequestBodySizeFeature = context.Features.Get<IHttpMaxRequestBodySizeFeature>();
        if (maxRequestBodySizeFeature != null)
        {
            maxRequestBodySizeFeature.MaxRequestBodySize = 104857600; // 100 MB
        }
        await next.Invoke();
    }
);
app.UseCors(builder =>
{
    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
});

//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();

//}
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.MapControllers();
app.UseAuthentication();
app.UseAuthorization();

// Health check endpoints
app.MapHealthChecks("/api/health");

app.MapGet(
    "/api/health/db",
    async (DataContext dataContext) =>
    {
        try
        {
            using (var con = dataContext.CreateConnection())
            {
                var result = await con.QueryFirstOrDefaultAsync<int>("SELECT 1");
                return Results.Ok(
                    new
                    {
                        status = "OK",
                        database = "Connected",
                        message = "RAG Server is running!",
                        timestamp = DateTime.UtcNow,
                    }
                );
            }
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, title: "Database connection failed");
        }
    }
);

// Statistics endpoint
app.MapGet(
    "/api/stats",
    async (DataContext dataContext) =>
    {
        try
        {
            using (var con = dataContext.CreateConnection())
            {
                var documentCount = await con.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM documents"
                );
                var chunkCount = await con.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM chunks");
                var embeddingCount = await con.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM embeddings"
                );

                var statusCounts = await con.QueryAsync<dynamic>(
                    "SELECT status, COUNT(*) as count FROM documents GROUP BY status"
                );

                return Results.Ok(
                    new
                    {
                        totalDocuments = documentCount,
                        totalChunks = chunkCount,
                        totalEmbeddings = embeddingCount,
                        documentsByStatus = statusCounts,
                        timestamp = DateTime.UtcNow,
                    }
                );
            }
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, title: "Failed to get statistics");
        }
    }
);

Console.WriteLine($"✅ Server starting...");
Console.WriteLine($"📊 Environment: {app.Environment.EnvironmentName}");
Console.WriteLine(
    $"🔗 Connection String: {builder.Configuration.GetConnectionString("DefaultConnection")?.Substring(0, 30)}..."
);
Console.WriteLine($"☁️  GCP Project: {builder.Configuration["GCP:ProjectId"]}");
Console.WriteLine($"📦 GCP Bucket: {builder.Configuration["GCP:BucketName"]}");

app.Run();
