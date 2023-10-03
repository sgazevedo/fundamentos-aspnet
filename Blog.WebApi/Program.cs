using Blog.Data;
using Blog.WebApi;
using Blog.WebApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IO.Compression;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
LoadConfiguration(builder);
ConfigureAuthentication(builder);
ConfigureMvc(builder);
ConfigureServices(builder);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure app to redirect http requests to https requests.
app.UseHttpsRedirection();

// Configure app to requires Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseStaticFiles();

app.UseResponseCompression();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.Run();

static void LoadConfiguration(WebApplicationBuilder builder)
{
  Configuration.JwtKey = builder.Configuration.GetValue<string>("JwtKey");
  Configuration.ApiKeyName = builder.Configuration.GetValue<string>("ApiKeyName");
  Configuration.ApiKey = builder.Configuration.GetValue<string>("ApiKey");

  var smtp = new Configuration.SmtpConfiguration();
  builder.Configuration.GetSection("Smtp").Bind(smtp);
  Configuration.Smtp = smtp;

  var email = new Configuration.SendEmailConfiguration();
  builder.Configuration.GetSection("Email").Bind(email);
  Configuration.Email = email;

  var images = new Configuration.ImagesConfiguration();
  builder.Configuration.GetSection("Images").Bind(images);
  Configuration.Images = images;
}

static void ConfigureAuthentication(WebApplicationBuilder builder)
{
  var key = Encoding.ASCII.GetBytes(Configuration.JwtKey);
  builder.Services.AddAuthentication(x =>
  {
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
  }).AddJwtBearer(x =>
  {
    x.TokenValidationParameters = new TokenValidationParameters
    {
      ValidateIssuerSigningKey = true,
      IssuerSigningKey = new SymmetricSecurityKey(key),
      ValidateIssuer = false,
      ValidateAudience = false
    };
  });
}

static void ConfigureMvc(WebApplicationBuilder builder)
{
  builder.Services.AddMemoryCache();
  builder.Services.AddResponseCompression(options =>
  {
    // options.Providers.Add<BrotliCompressionProvider>();
    // options.Providers.Add<CustomCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
  });
  builder.Services.Configure<GzipCompressionProviderOptions>(options =>
  {
    options.Level = CompressionLevel.Optimal;
  });
  builder
      .Services
      .AddControllers()
      .ConfigureApiBehaviorOptions(options => { options.SuppressModelStateInvalidFilter = true; })
      .AddJsonOptions(x =>
      {
        x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
      });
}

static void ConfigureServices(WebApplicationBuilder builder)
{
  var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
  builder.Services.AddDbContext<BlogDataContext>(options =>
  {
    options.UseSqlServer(connectionString);
  });
  builder.Services.AddTransient<ITokenService, TokenService>();
  builder.Services.AddTransient<IEmailService, EmailService>();
}
