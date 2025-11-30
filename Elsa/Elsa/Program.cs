using Elsa.EntityFrameworkCore.Extensions;
using Elsa.EntityFrameworkCore.Modules.Management;
using Elsa.EntityFrameworkCore.Modules.Runtime;
using Elsa.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Connection string
var connectionString = builder.Configuration.GetConnectionString("Elsa") ?? "Data Source=elsa.db";

// Add services to the container
builder.Services.AddControllers();

// Configure Swagger with CustomSchemaIds to avoid conflicts
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.CustomSchemaIds(type => type.FullName); // Prevent duplicate schemaId issues
});

// Add Elsa services
builder.Services.AddElsa(elsa =>
{
    // Configure Management feature
    elsa.UseWorkflowManagement(management =>
    {
        management.UseEntityFrameworkCore(ef => ef.UseSqlite(connectionString));
    });

    // Configure Runtime feature
    elsa.UseWorkflowRuntime(runtime =>
    {
        runtime.UseEntityFrameworkCore(ef => ef.UseSqlite(connectionString));
    });

    // Add API endpoints
    elsa.UseWorkflowsApi();
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Elsa API V1");
        c.RoutePrefix = "swagger"; // Swagger UI available at /swagger
    });
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors(cors => cors.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

// Elsa API Middleware MUST come before MapControllers
app.UseWorkflowsApi();

app.MapControllers();

app.Run();
