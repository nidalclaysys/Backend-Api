using Microsoft.Extensions.FileProviders;
using MvcApp.Hubs;
using MvcApp.Services;
using MvcApp.Services.Interfaces;
using MyWebAppApi.Extensions;
using MyWebAppApi.Helper;
using MyWebAppApi.Middlewares;
using MyWebAppApi.Repository;
using MyWebAppApi.Repository.Interfaces;
using MyWebAppApi.Services;
using MyWebAppApi.Services.Interfaces;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("http://localhost:8080") 
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerWithJwt();


builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IUserServices, UserServices>();
builder.Services.AddScoped<IJwtHelper,JwtHelper>();
builder.Services.AddScoped<IUserFinder, UserFinder>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IChatServices, ChatServices>();

builder.Services.AddSignalR(options => {
    options.EnableDetailedErrors = true;
});
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft",LogEventLevel.Warning)
    .MinimumLevel.Override("System",LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Async(x => x.File(
        path: "Logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 31,
        shared:true,
        outputTemplate:
                "{Timestamp:yyyy-MM-dd HH:mm:ss} | {Level:u3} | {Message:1j}{NewLine}{Exception}"
        )).CreateLogger();

builder.Host.UseSerilog();
            


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseCors("AllowSpecificOrigins");

app.UseStaticFiles();


app.UseExceptionMiddleware();
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseStaticFiles();


app.UseAuthentication();
app.UseAuthorization();
app.UseUserIdMiddleware();


app.MapHub<ChatHub>("/chatHub");

app.MapControllers();

app.Run();
