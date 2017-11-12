using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Net.Mail;
using System.Net;
using Whiskey.Utility.Data;

namespace Whiskey.Web.Helper
{

    public static class EmailHelper
    {

        static string SmtpServer = ConfigurationManager.AppSettings["SMTP-Server"];
        static string Sender = ConfigurationManager.AppSettings["SMTP-Email"];
        static string Password = ConfigurationManager.AppSettings["SMTP-Password"];

        public static OperationResult SendMail(string MailTo, string subject, string bodyinfo)
        {
            string formto = Sender;
            string to = MailTo;
            string content = subject;
            string body = bodyinfo;
            string name = Sender;
            string upass = Password;
            string smtp = SmtpServer;
            SmtpClient _smtpClient = new SmtpClient();
            _smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;//指定电子邮件发送方式
            _smtpClient.Host = smtp; //指定SMTP服务器
            _smtpClient.Credentials = new System.Net.NetworkCredential(name, upass);//用户名和密码
            MailMessage _mailMessage = new MailMessage();
            //发件人，发件人名 
            _mailMessage.From = new MailAddress(formto, "零时尚ERP管理平台");
            //收件人 
            _mailMessage.To.Add(to);
            _mailMessage.SubjectEncoding = System.Text.Encoding.GetEncoding("gb2312");
            _mailMessage.Subject = content;//主题

            _mailMessage.Body = body;//内容
            _mailMessage.BodyEncoding = System.Text.Encoding.GetEncoding("gb2312");//正文编码
            _mailMessage.IsBodyHtml = true;//设置为HTML格式
            _mailMessage.Priority = MailPriority.High;//优先级    
            _mailMessage.IsBodyHtml = true;
            try
            {
                _smtpClient.Send(_mailMessage);
                return new OperationResult(OperationResultType.Success, "邮件发送成功！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "邮件发送失败！错误如下：" + ex.Message,ex.ToString());
            }
        }


        public static OperationResult Activation(string MailTo, string url, string username)
        {
            string formto = Sender;
            string to = MailTo;
            string content = "零时尚ERP管理平台-激活帐号";
            string body = "尊敬的" + username + "用户:请点击此链接激活:";
            body += "<a href=" + url + ">" + url + "</a>";
            string name = Sender;
            string upass = Password;
            string smtp = SmtpServer;// "smtp.qq.com";
            SmtpClient _smtpClient = new SmtpClient();
            _smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;//指定电子邮件发送方式
            _smtpClient.Host = smtp; //指定SMTP服务器
            _smtpClient.Credentials = new System.Net.NetworkCredential(name, upass);//用户名和密码
            MailMessage _mailMessage = new MailMessage();
            //发件人，发件人名 
            _mailMessage.From = new MailAddress(formto, "零时尚ERP管理平台");
            //收件人 
            _mailMessage.To.Add(to);
            _mailMessage.SubjectEncoding = System.Text.Encoding.GetEncoding("gb2312");
            _mailMessage.Subject = content;//主题

            _mailMessage.Body = body;//内容
            _mailMessage.BodyEncoding = System.Text.Encoding.GetEncoding("gb2312");//正文编码
            _mailMessage.IsBodyHtml = true;//设置为HTML格式
            _mailMessage.Priority = MailPriority.High;//优先级    
            _mailMessage.IsBodyHtml = true;
            try
            {
                _smtpClient.Send(_mailMessage);
                return new OperationResult(OperationResultType.Success, "邮件发送成功！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "邮件发送失败！错误如下：" + ex.Message,ex.ToString());
            }

        }

    }
}