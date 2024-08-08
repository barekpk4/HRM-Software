using Nancy.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace WebApiCore.SmsGatewaySetup
{
    public static class SmsConfigSetup
    {

        public static bool SendSms(string number, string text)
        {
            try
            {

                var obj = new RootObject();
                var authentication = new Authentication();
                authentication.username = "ENAGROUP";
                authentication.password = "14JgWjmo";
                obj.authentication = authentication;

                var rc = new Recipient();
                rc.gsm = number;
                var lstRc = new List<Recipient>();
                lstRc.Add(rc);

                var message = new Message();
                message.recipients = lstRc;
                message.text = text;
                message.sender = "8809617644588";

                var msgList = new List<Message>();
                msgList.Add(message);

                obj.messages = msgList;


                var js = new JavaScriptSerializer();
                var msg = js.Serialize(obj);


                var request = (HttpWebRequest)WebRequest.Create("http://api.rankstelecom.com/api/v3/sendsms/json");

                var postData = msg;
                var data = Encoding.ASCII.GetBytes(postData);

                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = data.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                var response = (HttpWebResponse)request.GetResponse();

                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
