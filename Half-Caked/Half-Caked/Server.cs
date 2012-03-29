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
        public const string URL = "http://www.mattgerstman.com/halfcaked/scores";

        public static int RegisterProfile(string name)
        {
            try
            {
                return Int32.Parse(PostAndReceive("newUser.php", name));
            }
            catch { return -1; }
        }

        public static void SendHighScores(int guid, Statistics stats)
        {
            try
            {
                PostAndReceive("updateScore.php", guid + "," + stats.Level + "," + stats.Score);
            }
            catch { }
        }

        private static string PostAndReceive(string dest, string s)
        {
            WebRequest request = WebRequest.Create(URL +"/" + dest);

            request.Method = "POST";
            request.ContentType = "text/html";

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
