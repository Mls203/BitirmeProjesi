using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
namespace ETicaret
{
    public class MyEmail
    {
        public string ToEmail { get; private set; }
        public string Subject { get; private set; }
        public string Body { get; private set; }

        public MyEmail(string _ToEmail, string _Subject, string _Body)

        {
            this.ToEmail = _ToEmail;
            this.Subject = _Subject;
            this.Body = _Body;

        }

        public void SendMail()
        {
            MailMessage mail = new MailMessage()
            {
                From = new MailAddress("ergin.melis5@gmail.com", "Melisa ERGİN -öğrenci")//kimin göndereceği
            };

            mail.To.Add(this.ToEmail);//kime göndereceğiz
            mail.Subject = this.Subject;
            mail.Body = this.Body;

            SmtpClient client = new SmtpClient()
            {
                Port = 587,
                Host = "smtp.gmail.com",
                EnableSsl = true

            };

            client.Credentials = new System.Net.NetworkCredential("ergin.melis5@gmail.com", "Ergin.melisa.20321041520");//google üzerinden açacağım username ve şire
            client.Send(mail);//oturum açılmış clienta maili gönder
        }
    }
}