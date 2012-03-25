using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace Half_Caked
{
    static class Server
    {
        public const string URL = "http://www.mattgerstman.com/halfcaked";

        public static Guid RegisterProfile(string name)
        {
            try
            {
                return new Guid(PostAndReceive(name));
            }
            catch { return Guid.Empty; }
        }

        public static void SendHighScores(Guid g, Statistics stats)
        {
            try
            {
                PostAndReceive(g.ToString() + "," + stats.Level + "," + stats.Score);
            }
            catch { }
        }

        private static string PostAndReceive(string s)
        {
            WebRequest request = WebRequest.Create(URL);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            byte[] byteArray = Encoding.UTF8.GetBytes(s);
            request.ContentLength = byteArray.Length;

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            WebResponse response = request.GetResponse();
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            dataStream = response.GetResponseStream();

            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();

            reader.Close();
            dataStream.Close();
            response.Close();

            return responseFromServer;
        }
    }
}
