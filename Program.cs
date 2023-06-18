using FilesAPI.Quartz;
using FilesAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Quartz;
using Serilog;
using Swashbuckle.AspNetCore.Filters;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    options.OperationFilter<SecurityRequirementsOperationFilter>();
});
builder.Services.AddSingleton<AppContext>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<FileService>();
builder.Services.AddQuartz(settings =>
{
    settings.UseMicrosoftDependencyInjectionJobFactory();

    var wasteBinCleanerJobKey = new JobKey("WasteBinCleanerJob");

    settings.AddJob<WasteBinCleanerJob>(opts => opts.WithIdentity(wasteBinCleanerJobKey));
    settings.AddTrigger(opts => opts
    .ForJob(wasteBinCleanerJobKey)
    .WithIdentity($"{wasteBinCleanerJobKey}-trigger")
    .WithCronSchedule("0 0/30 * * * ?"));

    var BackupDbJobKey = new JobKey("BackupDbJob");

    settings.AddJob<BackupDbJob>(opts => opts.WithIdentity(BackupDbJobKey));
    settings.AddTrigger(opts => opts
    .ForJob(BackupDbJobKey)
    .WithIdentity($"{BackupDbJobKey}-trigger")
    .WithCronSchedule("0 0 0 1,14 * ?"));
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = false);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateAudience = false,
        ValidateIssuer = false,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("JQHWDy1u2h3b87!*&$&!$hdff786")), 
    };
});

builder.Logging.AddSerilog(new LoggerConfiguration().WriteTo.PostgreSQL("Host=localhost;Port=5432;Database=filesdb;Username=postgres;Password=1", "Logss", needAutoCreateTable: true).CreateLogger());

var app = builder.Build();

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

