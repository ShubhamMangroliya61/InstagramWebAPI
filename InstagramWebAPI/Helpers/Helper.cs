using System.Net.Mail;
using System.Net;

namespace InstagramWebAPI.Helpers
{
    public class Helper
    {
        public async Task<bool> EmailSender(string email, string subject, string message)
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
                await client.SendMailAsync(new MailMessage(from: mail, to: email, subject, message));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
