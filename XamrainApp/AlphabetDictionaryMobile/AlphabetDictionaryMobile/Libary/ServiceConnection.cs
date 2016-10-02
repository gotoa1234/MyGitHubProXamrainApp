using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Speech.Tts;
using System.IO;
using System.Net;
using System.Xml;

/// <summary>
/// Service 連結 目的：撈取最新版本的資料
/// </summary>
namespace AlphabetDictionaryMobile.ServiceConnection
{
    public class ServiceConnection
    { 
        //DB路徑
        private string ServiceConnection_dbpath = "http://210.59.250.198/AppService/Home/Download?FileName=Alphabet.db&FileNamePathfull=C%3A%5CAppService%5CFileUploads%5CMobile%5CAlphabetDictionaryMobile%5CAlphabet.db";
        //版本號路徑
        private string ServiceConnection_version = "http://210.59.250.198/AppService/Home/Download?FileName=AlphabetDictionaryMobileVersion.xml&FileNamePathfull=C%3A%5CAppService%5CFileUploads%5CMobile%5CAlphabetDictionaryMobile%5CAlphabetDictionaryMobileVersion.xml";

        /// <summary>
        /// 建構式
        /// </summary>
        public ServiceConnection()
        {
            
        }

        /// <summary>
        /// 取得DB
        /// </summary>
        public bool Get_DB()
        {
            try
            {
                //NewToiec資料庫 
                var Sourcepath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Alphabet.db");
                //撈取Service SqliteDB
                Stream sqlitedb = Get_FromServer(ServiceConnection_dbpath);

                if (sqlitedb == null)
                    return false;

                using (var dest = File.Create(Sourcepath))
                {
                    //寫到本機資料庫
                    sqlitedb.CopyTo(dest);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 取得版本號資訊
        /// </summary>
        public string Get_Version()
        {
            WebClient client = new WebClient();//------Client
            MemoryStream ms = new MemoryStream(client.DownloadData(ServiceConnection_version));
            string msg= Encoding.ASCII.GetString(ms.ToArray());

            return msg;
        }


        #region private function

        /// <summary>
        /// 從Server取得Sqlite的資料
        /// </summary>
        /// <returns></returns>
        private Stream Get_FromServer(string url)
        {
            //Create a stream for the file
            Stream SQdb = null;

            //This controls how many bytes to read at a time and send to the client
            int bytesToRead = 10000;

            // Buffer to read bytes in chunk size specified above
            byte[] buffer = new Byte[bytesToRead];

            // The number of bytes read
            try
            {
                //Create a WebRequest to get the file
                HttpWebRequest fileReq = (HttpWebRequest)HttpWebRequest.Create(url);

                //Create a response for this request
                HttpWebResponse fileResp = (HttpWebResponse)fileReq.GetResponse();

                if (fileReq.ContentLength > 0)
                    fileResp.ContentLength = fileReq.ContentLength;

                //Get the Stream returned from the response
                SQdb = fileResp.GetResponseStream();
            }
            catch (Exception ex)
            {
            }

            return SQdb;

        }



        #endregion
    }
}