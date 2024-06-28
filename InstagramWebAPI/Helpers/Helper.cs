using System.Net.Mail;
using System.Net;

namespace InstagramWebAPI.Helpers
{
    public class Helper
    {
        public async Task<bool> EmailSender(string email, string subject, string htmlMessage)
        {
            try
            {
                var mail = "tatva.dotnet.shubhammangroliya@outlook.com";
                var password = "snwwkdrbhcdxifyc";

                var client = new SmtpClient("smtp.office365.com", 587)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(mail, password)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(mail),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
