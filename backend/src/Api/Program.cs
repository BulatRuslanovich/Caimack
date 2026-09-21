using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Startup;
using Infrastructure;
using App;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddApiOpenApi();


builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApp();


var app = builder.Build();

app.UseForwardedHeaders();

app.MapControllers();
app.MapApiOpenApi();
app.Run();
