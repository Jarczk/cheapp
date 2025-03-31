using AspNetCore.Identity.Mongo;
using AspNetCore.Identity.Mongo.Model;
using Cheapp.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using dotenv.net;


var builder = WebApplication.CreateBuilder(args);

DotEnv.Load();

var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
if (string.IsNullOrEmpty(jwtKey))
{
    jwtKey = "superSecretKeyOnlyForLocalDev";
}

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
       // other options
   });

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
