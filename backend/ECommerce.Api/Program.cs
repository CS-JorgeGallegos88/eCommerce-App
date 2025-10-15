using ECommerce.Api.Middlewares;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddWebServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Ensure the database directory exists
    var dbPath = Path.GetDirectoryName(Path.GetFullPath("ecommerce.db"));
    if (!string.IsNullOrEmpty(dbPath) && !Directory.Exists(dbPath))
    {
        Directory.CreateDirectory(dbPath);
    }

    app.UseSwagger();
    app.UseSwaggerUI(opt =>
    {
        opt.SwaggerEndpoint("/swagger/v1/swagger.json", "Api v1");
        opt.RoutePrefix = "swagger";
    });

    using (var scope = app.Services.CreateScope())
    {
        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();
        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<SaveChangesMiddleware>();

app.UseXContentTypeOptions();
app.UseReferrerPolicy(opt => opt.NoReferrer());
app.UseXXssProtection(opt => opt.EnabledWithBlockMode());
app.UseXfo(opt => opt.Deny());
app.UseCsp(opt => opt
    .BlockAllMixedContent()
);


app.UseRouting();

app.UseCors();

app.UseHealthChecks("/health");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
