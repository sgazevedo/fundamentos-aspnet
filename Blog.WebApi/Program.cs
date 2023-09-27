using Blog.Data;
using Blog.WebApi;
using Blog.WebApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

ConfigureAuthentication(builder);
ConfigureMvc(builder);
ConfigureServices(builder);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

LoadConfiguration(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Configure app to requires Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static void LoadConfiguration(WebApplication app)
{
  Configuration.JwtKey = app.Configuration.GetValue<string>("JwtKey");
  Configuration.ApiKeyName = app.Configuration.GetValue<string>("ApiKeyName");
  Configuration.ApiKey = app.Configuration.GetValue<string>("ApiKey");

  var smtp = new Configuration.SmtpConfiguration();
  app.Configuration.GetSection("SmtpConfiguration").Bind(smtp);
  Configuration.Smtp = smtp;
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
  builder
      .Services
      .AddControllers()
      .ConfigureApiBehaviorOptions(options => { options.SuppressModelStateInvalidFilter = true; });
}

static void ConfigureServices(WebApplicationBuilder builder)
{
  builder.Services.AddDbContext<BlogDataContext>();
  builder.Services.AddTransient<ITokenService, TokenService>();
}
