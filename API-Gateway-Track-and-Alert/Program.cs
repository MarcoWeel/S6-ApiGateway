using API_Gateway.Helpers;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Kubernetes;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("secrets/appsettings.secrets.json", true);


var securityKey = builder.Configuration.GetSection("Keys")["SecurityKey"];

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOcelot().AddKubernetes();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.MapControllers();

// Add Authorization
var config = new OcelotPipelineConfiguration
{
    AuthorizationMiddleware
        = async (downStreamContext, next) =>
        await JwtMiddleware.CreateAuthorizationFilter(downStreamContext, securityKey, next)
};

app.UseOcelot(config).Wait();

app.Run();
