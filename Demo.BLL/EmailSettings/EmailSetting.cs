using Demo.DAL.Models.IdentityModule;
using System.Net;
using System.Net.Mail;

namespace Demo.BLL.EmailSettings
{
    public class EmailSetting : IEmailSetting
    {
        public void SendEmail(Email email)
        {
            var Client = new SmtpClient("smtp.gmail.com", 587);
            Client.EnableSsl = true;
            Client.Credentials = new NetworkCredential("eslamrabai5445@gmail.com", "odrodpdurkdtnclr");
            Client.Send("eslamrabai5445@gmail.com", email.To, email.Subject, email.Body);
        }
    }
}
