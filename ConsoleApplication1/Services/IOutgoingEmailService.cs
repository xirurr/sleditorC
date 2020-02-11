using System;
using System.IO;

namespace ConsoleApplication1.Services
{
    public interface IOutgoingEmailService
    {
        /// <summary>
        /// Sends email with attachments from memory stream.
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
        void Send(string subject, string message, string from, string[] recipients, string[] cc, string[] bcc, Tuple<Stream, string, string, string>[] attachments);

        /// <summary>
        /// Sends email with attachments from disk.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="message"></param>
        /// <param name="from"></param>
        /// <param name="recipients"></param>
        /// <param name="cc"></param>
        /// <param name="bcc"></param>
        /// <param name="attachments">
        /// Tuple<string, string>
        /// string: attachment filename
        /// string: attachment mime type name
        /// </param>
        void Send(string subject, string message, string from, string[] recipients, string[] cc, string[] bcc, params Tuple<string, string>[] attachments);
    }
}