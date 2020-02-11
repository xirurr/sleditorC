using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using NLog;
using NLog.Fluent;

namespace ConsoleApplication1.Services
{
        public class SmtpEmailService : IOutgoingEmailService
    {
        private readonly string username;
        private readonly string password;
        private readonly string host;
        private readonly int port;
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public SmtpEmailService(string host, int port, string username, string password)
        {
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentException("Host can not be empty");

            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username can not be empty");

            if (port == 0)
                throw new ArgumentException("Port can not be 0");

            this.host = host;
            this.port = port;
            this.username = username;
            this.password = password;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="message"></param>
        /// <param name="from"></param>
        /// <param name="recipients"></param>
        /// <param name="cc"></param>
        /// <param name="bcc"></param>
        /// <param name="attachments">
        /// Tuple<Stream, string, string>
        /// Stream: attachment content
        /// string: attachment filename
        /// string: attachment mime type name
        /// string: attachment content id (CID)
        /// </param>
        public void Send(string subject, string message, string from, string[] recipients, string[] cc, string[] bcc, Tuple<Stream, string, string, string>[] attachments)
        {
            var atts = new List<Attachment>();

            foreach (var attachment in attachments)
            {
                attachment.Item1.Position = 0;
                var at = new Attachment(attachment.Item1, attachment.Item3);
                at.ContentDisposition.FileName = attachment.Item2;
                if (attachment.Item4 != null)
                    at.ContentId = attachment.Item4;
                atts.Add(at);
            }

            Send(subject, message, from, recipients, cc, bcc, atts);
        }

        public void Send(string subject, string message, string from, string[] recipients, string[] cc, string[] bcc, Tuple<string, string>[] attachments)
        {
            var atts = new List<Attachment>();

            foreach (var attachment in attachments)
            {
                var at = new Attachment(attachment.Item1);
                at.ContentId = attachment.Item2;
                atts.Add(at);
            }

            Send(subject, message, from, recipients, cc, bcc, atts);
        }

        public void Send(string subject, string message, string from, string[] recipients, string[] cc, string[] bcc, ICollection<Attachment> attachments)
        {
            logger.Trace().Message("Try to send Email")
                .Property("subject", subject ?? "null")
                .Property("from", from ?? "null")
                .Property("recipients", string.Join(";", recipients) == null ? "null" : string.Join(";", recipients))
                .Property("сс_recipients", string.Join(";", cc) == null ? "null" : string.Join(";", cc))
                .Property("bcc_recipients", string.Join(";", bcc) == null ? "null" : string.Join(";", bcc))
                .Write();
            using (var client = new SmtpClient())
            {
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.EnableSsl = true;
                client.Host = host;
                client.Port = port;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(username, password);

                var email = new MailMessage();
                email.From = new MailAddress(!string.IsNullOrWhiteSpace(from) ? from : username);
                email.Subject = subject;
                email.IsBodyHtml = true;
                email.Body = message;
                foreach (var attachment in attachments)
                {
                    email.Attachments.Add(attachment);
                }

                foreach (var recipient in recipients.Where(r => !string.IsNullOrWhiteSpace(r)))
                    email.To.Add(new MailAddress(recipient.Trim()));

                foreach (var recipient in cc.Where(r => !string.IsNullOrWhiteSpace(r)))
                    email.CC.Add(new MailAddress(recipient.Trim()));

                foreach (var recipient in bcc.Where(r => !string.IsNullOrWhiteSpace(r)))
                    email.Bcc.Add(new MailAddress(recipient.Trim()));

                if (email.To.Count == 0)
                    throw new ArgumentException("You should specify at least one recepient");

                try
                {
                    client.Send(email);
                    logger.Info().Message($"Email sent. Subject:{subject}. Recepients:{string.Join(";", recipients)}. Attachments: {attachments.Count}").Write();
                }
                catch (Exception ex)
                {
                    logger.Error().Exception(ex).Message($"Error sending email. Subject:{subject}. Recepients:{string.Join("; ", recipients)}. Attachments: {attachments.Count}").Write();
                }
            }
        }
    }
}