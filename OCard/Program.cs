using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;


namespace OCard
{
    class Program
    {
        static void Main(string[] args)
        {
            String postData = "";
            String url = "";
            string path = @Environment.CurrentDirectory;
            String LogPath = @path + "/OCardLog";

            DeleteLog(LogPath);

            if (!Directory.Exists(LogPath))
            {
                Directory.CreateDirectory(LogPath);
            }

            try
            {
                using (StreamReader str = new StreamReader(path + "/in.txt"))
                {
                    url = str.ReadLine();
                    postData = str.ReadToEnd();
                    postData = postData.Replace("\r\n", "");
                }
            }
            catch (Exception error)
            {
                OcardLog(error.ToString(), LogPath);
            }

            // postData = "key=ocard_pos&secret=d8db49fe7770c8d84f50e6e07cc457a3&for_web=0";
            //OcardLog("--------------------------------------------------------------------------");
            OcardLog("程式開啟", LogPath);
            File.Delete("out.txt");
            OcardLog("APIUrl:" + url, LogPath);
            OcardLog("Input:" + postData, LogPath);
            //url = "https://api.ocard.co/api2/auth";

            int ApiTry = 0;
            while (ApiTry <= 2)
            {
                string result = "";
                try
                {
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                    req.Credentials = CredentialCache.DefaultCredentials;

                    req.Method = "POST";

                    req.ContentType = "application/x-www-form-urlencoded";

                    req.Timeout = 15000;//请求超时时间 Timeout 時間為毫秒，30 * 1000 表示 30秒 為超時上限時間。
                                        //req.Timeout = 100;

                    byte[] data = Encoding.UTF8.GetBytes(postData);

                    req.ContentLength = data.Length;

                    using (Stream reqStream = req.GetRequestStream())
                    {
                        reqStream.Write(data, 0, data.Length);
                        reqStream.Close();
                    }
                   

                    HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

                    Stream stream = resp.GetResponseStream();

                    //获取响应内容
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        result = reader.ReadToEnd();
                        HttpsOutTxt(result);
                        OcardLog("Oupt:" + result, LogPath);
                        reader.Close();
                    }

                    resp.Close();
                    data = null;
                    break;
                }
                catch (WebException error)
                {
                    /*if (error.Status == WebExceptionStatus.Timeout)
                    {
                        HttpsOutTxt("伺服器沒有回應");
                    }*/
                    OcardLog(error.ToString(), LogPath);
                }
                ApiTry++;
            }

            OcardLog("程式關閉", LogPath);
            OcardLog("--------------------------------------------------------------------------", LogPath);


        }

        private static string StringToUnicode(string postData)
        {
            string dst = "";
            char[] src = postData.ToCharArray();
            for (int i = 0; i < src.Length; i++)
            {
                byte[] bytes = Encoding.Unicode.GetBytes(src[i].ToString());
                string str = @"\u" + bytes[1].ToString("X2") + bytes[0].ToString("X2");
                dst += str;
            }
            return dst;
        }

        private static void DeleteLog(string logPath)
        {
            //去除資料夾和子檔案的只讀屬性
            //去除資料夾的只讀屬性
            System.IO.DirectoryInfo fileInfo = new DirectoryInfo(logPath);
            fileInfo.Attributes = FileAttributes.Normal & FileAttributes.Directory;
            //去除檔案的只讀屬性
            System.IO.File.SetAttributes(logPath, System.IO.FileAttributes.Normal);
            //判斷資料夾是否還存在
            if (Directory.Exists(logPath))
            {
                foreach (string f in Directory.GetFileSystemEntries(logPath))
                {
                    if (File.Exists(f))
                    {
                        if (Directory.GetCreationTime(f) < DateTime.Now.AddDays(-60))
                        {
                            //如果有子檔案刪除檔案
                            File.Delete(f);
                        }
                    }
 
                }
            }
        }

        private static void HttpsOutTxt(String ResultData)
        {
            using (StreamWriter sw = new StreamWriter("out.txt", true))
            {
                sw.Write(ResultData);
            }
        }

        private static void OcardLog(String Data,String LogPath)
        {
            using (StreamWriter file = new StreamWriter(Path.Combine(LogPath, DateTime.Now.ToString("yyyyMMdd") + ".txt"), true))
            {
                file.WriteLine(DateTime.Now + " " + Data);// 直接追加檔案末尾，換行             
            }

        }

    }
}
