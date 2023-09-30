namespace Blog.WebApi
{
  public static class Configuration
  {
    public static string JwtKey { get; set; }
    public static string ApiKeyName { get; set; }
    public static string ApiKey { get; set; }

    public static SmtpConfiguration Smtp = new();

    public static SendEmailConfiguration Email = new();

    public static ImagesConfiguration Images = new();

    public class SmtpConfiguration
    {
      public string Host { get; set; }
      public int Port { get; set; } = 25;
      public string UserName { get; set; }
      public string Password { get; set; }
    }

    public class SendEmailConfiguration
    {
      public string FromName { get; set; }
      public string FromEmail { get; set; }
      public string Subject { get; set; }
      public string Body { get; set; }
    }

    public class ImagesConfiguration
    {
      public string UrlHost { get; set; }
    }
  }
}
