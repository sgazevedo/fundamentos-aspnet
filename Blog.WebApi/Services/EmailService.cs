using System.Net.Mail;
using System.Net;

namespace Blog.WebApi.Services
{
  public class EmailService : IEmailService
  {
    public bool Send(string toName, string toEmail, string subject, string body, string fromName, string fromEmail)
    {
      var smtpClient = new SmtpClient(Configuration.Smtp.Host, Configuration.Smtp.Port)
      {
        Credentials = new NetworkCredential(Configuration.Smtp.UserName, Configuration.Smtp.Password),
        DeliveryMethod = SmtpDeliveryMethod.Network,
        EnableSsl = true
      };
      var mail = new MailMessage
      {
        From = new MailAddress(fromEmail, fromName)
      };
      mail.To.Add(new MailAddress(toEmail, toName));
      mail.Subject = subject;
      mail.Body = body;
      mail.IsBodyHtml = true;

      try
      {
        smtpClient.Send(mail);
        return true;
      }
      catch
      {
        return false;
      }
    }
  }
}
