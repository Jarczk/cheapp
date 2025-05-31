using AspNetCore.Identity.Mongo;
using AspNetCore.Identity.Mongo.Model;
using Cheapp.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using dotenv.net;
using DotNetEnv;
using Cheapp.Options;
using Cheapp.Services;
using MongoDB.Driver;
using Microsoft.Extensions.Options;


Env.Load();
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();


/*var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
if (string.IsNullOrEmpty(jwtKey))
{
    jwtKey = "superSecretKeyOnlyForLocalDev";
}*/


// Add services to the container.
builder.Services.AddIdentityMongoDbProvider<ApplicationUser, ApplicationRole>(identity =>
{
    identity.Password.RequiredLength = 8;
    identity.Password.RequireDigit = false;
    // other options
},
   mongo =>
   {
       mongo.ConnectionString = builder.Configuration["MongoDB:ConnectionString"]; ;
       mongo.UsersCollection = "Users";
       mongo.RolesCollection = "Roles";
   });

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

var jwtOpts = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
             ?? throw new InvalidOperationException("Jwt section missing");

if (string.IsNullOrWhiteSpace(jwtOpts.Key))
    throw new InvalidOperationException("Jwt:Key cannot be empty");

if (Encoding.UTF8.GetByteCount(jwtOpts.Key) < 32)
    throw new InvalidOperationException("Jwt:Key must be ≥ 32 bytes (256 bits) for HS256");

var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOpts.Key));

builder.Services.AddAuthentication(opts =>
{
    opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opts =>
{
    opts.RequireHttpsMetadata = false;
    opts.SaveToken = true;
    opts.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtOpts.Issuer,
        ValidAudience = jwtOpts.Audience,
        IssuerSigningKey = key
    };
});


builder.Services.Configure<EbayOptions>(builder.Configuration.GetSection("Ebay"));
builder.Services.Configure<GroqOptions>(builder.Configuration.GetSection("Groq"));
builder.Services.Configure<MongoOptions>(builder.Configuration.GetSection("MongoDB"));

// eBay OAuth
builder.Services.AddHttpClient("EbayAuth", c =>
{
    c.BaseAddress = new Uri("https://api.sandbox.ebay.com/");
});

builder.Services.AddSingleton<IEbayOAuthService, EbayOAuthService>();
builder.Services.AddTransient<EbayAuthHandler>();

builder.Services.AddHttpClient<IEbayClient, EbayClient>((sp, c) =>
{
    var opt = sp.GetRequiredService<IOptions<EbayOptions>>().Value;
    c.BaseAddress = new Uri(opt.BaseUrl);
    c.DefaultRequestHeaders.Add("X-EBAY-C-MARKETPLACE-ID", opt.Marketplace);
})
.AddHttpMessageHandler<EbayAuthHandler>();

builder.Services.AddHostedService<EbayTokenWarmup>();

// MongoDB
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var opt = sp.GetRequiredService<IOptions<MongoOptions>>().Value;
    return new MongoClient(opt.ConnectionString);
});
builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var opt = sp.GetRequiredService<IOptions<MongoOptions>>().Value;
    return sp.GetRequiredService<IMongoClient>().GetDatabase(opt.Database);
});

// HTTP clients
builder.Services.AddHttpClient<IAssistantClient, GroqAssistantClient>((sp, c) =>
{
    var opt = sp.GetRequiredService<IOptions<GroqOptions>>().Value;
    c.BaseAddress = new Uri(opt.BaseUrl);
    c.DefaultRequestHeaders.Add("Authorization", $"Bearer {opt.ApiKey}");
});

// domain services
builder.Services.AddScoped<IOfferAggregator, OfferAggregator>();
builder.Services.AddScoped<IConversationService, ConversationService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    var roles = new[] { "User", "Admin" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new ApplicationRole { Name = role });
        }
    }
}

app.Run();

public record JwtOptions
{
    public string Key { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
}