using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using WC.Logger;

namespace WC.API.SMTP
{
    public class SMTPClient
    {
        private const string SubjectSuccessPreffix = "SUCCESS: ";
        private const string SubjectWithErrorPreffix = "FAILURE: ";
        private const string SubjectWithFailedPreffix = "FAILURE: ";
        private const string SubjectWithHigherThresholdExcedeedPreffix = "FAILURE: ";
        private const string SubjectWithLowerThresholdExcedeedPreffix = "WARNING: ";

        private readonly string _host;
        private readonly int _port;
        private readonly string _user;
        private readonly string _password;
        private readonly string _fromAddress;
        private readonly List<string> _toAddresses;

        public SMTPClient(string host, int port, string user, string password, string fromAddress, List<string> toAddresses)
        {
            this._host = host;
            this._port = port;
            this._user = user;
            this._password = password;
            this._fromAddress = fromAddress;
            this._toAddresses = toAddresses;
        }

        public bool SendEmail(string subject, string body)
        {
            bool output = false;

            try
            {
                using (SmtpClient smtp = new SmtpClient())
                {
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.UseDefaultCredentials = false;
                    smtp.EnableSsl = true;
                    smtp.Host = this._host;
                    smtp.Port = this._port;
                    smtp.Credentials = new NetworkCredential(this._user, this._password);

                    var message = new MailMessage();
                    message.From = new MailAddress(this._fromAddress);
                    foreach (string toAddress in this._toAddresses)
                    {
                        message.To.Add(toAddress);
                    }

                    message.Subject = subject;
                    message.Body = body;

                    smtp.Send(message);
                }

                output = true;
                return output;
            }
            catch (Exception ex)
            {
                LogEngine.CrawlerLogger.Error($"Exception: {ex}");
                return false;
            }
        }
    }
}

