using Campaigns.Api.ErrorHandling;
using Campaigns.Api.Security;
using Campaigns.Application.UseCases;
using Campaigns.Infrastructure;
using Campaigns.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Campaigns.Api", Version = "v1" });

    // Adds the "Authorize" button in Swagger UI so a JWT can be pasted in manually for testing.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<IExceptionResponseHandler, CampaignNotFoundExceptionHandler>();
builder.Services.AddSingleton<IExceptionResponseHandler, CampaignNotReceivingDonationsExceptionHandler>();
builder.Services.AddSingleton<IExceptionResponseHandler, CampaignConcurrencyConflictExceptionHandler>();
builder.Services.AddSingleton<IExceptionResponseHandler, DomainExceptionHandler>();

builder.Services.AddCampaignsInfrastructure(builder.Configuration);

builder.Services.AddScoped<CreateCampaignUseCase>();
builder.Services.AddScoped<UpdateCampaignUseCase>();
builder.Services.AddScoped<GetCampaignByIdUseCase>();
builder.Services.AddScoped<GetPublicCampaignsUseCase>();
builder.Services.AddScoped<CreateDonationUseCase>();

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtPublicKey = RsaPublicKeyProvider.Load(jwtSection["PublicKeyPath"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new RsaSecurityKey(jwtPublicKey),
            // Default ClockSkew is 5 minutes when left unset — an expired token would still be
            // accepted for up to 5 extra minutes past its exp claim. 30s covers realistic clock
            // drift between pods without silently extending the token's real lifetime.
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

var campaignsDbConnectionString = builder.Configuration.GetConnectionString("CampaignsDb")!;
builder.Services.AddHealthChecks()
    .AddNpgSql(campaignsDbConnectionString, name: "campaigns-db");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpMetrics();
app.UseExceptionHandler();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapMetrics("/metrics");

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CampaignsDbContext>();
    await context.Database.MigrateAsync();
}

app.Run();
