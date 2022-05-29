using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;
using Simple.File.Api;
using Simple.File.Api.Interfaces;
using Simple.File.Api.Security;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>
        ("BasicAuthentication", null);

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("basic", new OpenApiSecurityScheme  
    {  
        Name = "Authorization",  
        Type = SecuritySchemeType.Http,  
        Scheme = "basic",  
        In = ParameterLocation.Header,  
        Description = "Basic Authorization"  
    });  
    options.AddSecurityRequirement(new OpenApiSecurityRequirement  
    {  
        {  
            new OpenApiSecurityScheme  
            {  
                Reference = new OpenApiReference  
                {  
                    Type = ReferenceType.SecurityScheme,  
                    Id = "basic"  
                }  
            },  
            new string[] {}  
        }  
    });
});

builder.Services.AddAuthorization();

// add services/providers/repositories
builder.Services.AddHttpContextAccessor();
// TODO: do not use hard coded strings
builder.Services.AddSingleton<SimpleFileStorageOptions>(serviceProvider => new SimpleFileStorageOptions(builder.Configuration[nameof(SimpleFileApiSettings.FilesLocation)], Int32.Parse(builder.Configuration[nameof(SimpleFileApiSettings.FileSizeLimitInBytes)])));
builder.Services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
builder.Services.AddScoped<IUserFileStorage, UserFileStorage>();
builder.Services.AddScoped<ISimpleFileStorage, SimpleFileStorage>();
builder.Services.Configure<UserServiceOptions>(
    builder.Configuration.GetSection(nameof(SimpleFileApiSettings.UserServiceOptions)));
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
    });
    app.UseSwaggerUI(options =>
    {

    });
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();