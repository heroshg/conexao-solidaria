using HealthChecks.NpgSql;
using Identity.Api.ErrorHandling;
using Identity.Application.UseCases;
using Identity.Infrastructure;
using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Seed;
using Microsoft.OpenApi;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Identity.Api", Version = "v1" });
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<IExceptionResponseHandler, EmailAlreadyInUseExceptionHandler>();
builder.Services.AddSingleton<IExceptionResponseHandler, CpfAlreadyInUseExceptionHandler>();
builder.Services.AddSingleton<IExceptionResponseHandler, InvalidCredentialsExceptionHandler>();
builder.Services.AddSingleton<IExceptionResponseHandler, DomainExceptionHandler>();

builder.Services.AddIdentityInfrastructure(builder.Configuration);

builder.Services.AddScoped<RegisterDonorUseCase>();
builder.Services.AddScoped<LoginUseCase>();

var identityDbConnectionString = builder.Configuration.GetConnectionString("IdentityDb")!;
builder.Services.AddHealthChecks()
    .AddNpgSql(identityDbConnectionString, name: "identity-db");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpMetrics();

app.UseExceptionHandler();

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapMetrics("/metrics");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<IdentityDbContext>();
    var passwordHasher = services.GetRequiredService<Identity.Application.Abstractions.IPasswordHasher>();
    var logger = services.GetRequiredService<ILogger<Program>>();
    await NgoManagerSeeder.SeedAsync(context, passwordHasher, logger);
}

app.Run();
