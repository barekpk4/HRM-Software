using System.Collections.Generic;

namespace WebApiCore.SmsGatewaySetup
{
    public class Authentication
    {
        public string username { get; set; }
        public string password { get; set; }
    }

    public class Recipient
    {
        public string gsm { get; set; }
    }

    public class Message
    {
        public string sender { get; set; }
        public string text { get; set; }
        public List<Recipient> recipients { get; set; }
    }

    public class RootObject
    {
        public Authentication authentication { get; set; }
        public List<Message> messages { get; set; }
    }

}
