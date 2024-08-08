namespace WebApiCore.MailingSetup
{
    public class MailViewModel
    {
        public string SenderEmail { get; set; }
        public string SenderName { get; set; }
        public string ReceiverEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
