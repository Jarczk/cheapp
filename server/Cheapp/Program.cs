using AspNetCore.Identity.Mongo;
using AspNetCore.Identity.Mongo.Model;
using Cheapp.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

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
