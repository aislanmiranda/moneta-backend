using System.Security.Claims;
using System.Text.Json.Serialization;
using AgentOffice.Api.Auth;
using AgentOffice.Api.Middleware;
using AgentOffice.Api.Realtime;
using AgentOffice.Application;
using AgentOffice.Application.Auth;
using AgentOffice.Domain.Auth;
using AgentOffice.Infra;
using AgentOffice.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options => options.Filters.Add<ModuleOwnershipFilter>())
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var jwtOptions = builder.Configuration.GetSection(JwtOptions.Section).Get<JwtOptions>() ?? new JwtOptions();
if (jwtOptions.JwtKey.Length < 32)
    throw new InvalidOperationException("Configure Auth:JwtKey (Auth__JwtKey) com pelo menos 32 caracteres.");

builder.Services.AddSingleton(jwtOptions);
builder.Services.AddSingleton<ITokenService, JwtTokenService>();
builder.Services.AddHttpContextAccessor();
// Registered before AddInfra: AppDbContext needs an ICurrentUser for its per-user query filters.
builder.Services.AddScoped<HttpCurrentUser>();
builder.Services.AddScoped<ICurrentUser>(sp => sp.GetRequiredService<HttpCurrentUser>());

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = JwtOptions.Issuer,
            ValidAudience = JwtOptions.Issuer,
            IssuerSigningKey = jwtOptions.SigningKey(),
            NameClaimType = ClaimTypes.Name,
            ClockSkew = TimeSpan.FromMinutes(1),
        };
        // Browsers can't set headers on the SignalR WebSocket — the client sends the token as
        // ?access_token=... instead, accepted only on the hub path.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(token) && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                    context.Token = token;
                return Task.CompletedTask;
            },
        };
    });

// Everything requires a signed-in user unless marked [AllowAnonymous] (only register/login).
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

builder.Services.AddInfra(builder.Configuration);
builder.Services.AddRepositories();
builder.Services.AddApplication();

builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
        options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddHostedService<PostgresNotificationListener>();

// The PWA normally calls the API same-origin (Vite dev proxy / the frontend container's nginx),
// so CORS only matters when it's pointed at the API directly via VITE_API_ORIGIN.
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://localhost:1420", "http://localhost:8080")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors("Frontend");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationsHub>("/hubs/notifications");

app.Run();
