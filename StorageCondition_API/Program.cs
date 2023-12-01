using Azure.Storage.Blobs;
using StorageCondition_API.services;
using NLog.Web;
using NLog.Extensions.Logging;
using NLog;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    //builder.Logging.ClearProviders();
    //builder.Host.UseNLog();
    builder.Services.AddLogging(loggingBuilder =>
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.AddNLog();
    });
    builder.Services.AddScoped<ITrackRunService, TrackRunService>();
    builder.Services.AddScoped<IPromptAuditService, PromptAuditService>();
    builder.Services.AddScoped(_ =>
    {
        return new BlobServiceClient(builder.Configuration.GetConnectionString("AzureStorage"));
    });
    var app = builder.Build();



    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex);
}