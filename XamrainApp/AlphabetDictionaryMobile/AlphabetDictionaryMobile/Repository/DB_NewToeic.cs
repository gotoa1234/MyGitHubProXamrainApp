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
using System.IO;
using SQLite;
using AlphabetDictionaryMobile.Repository.ORMTable;

namespace AlphabetDictionaryMobile.Repository
{
    public class DB_NewToeic
    {
        public string Get_Allrecord()
        {
            string Output = "";
            try
            {
                string dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Alphabet.db");
                var db = new SQLiteConnection(dbPath);

                //-- ORM 取得資料內容
                var tablerows = db.Table<Table_Row>();

                foreach (var row in tablerows)
                {
                    Output += string.Format("英文: {0} 中文:{1} \r\n", row.English, row.Chinese);
                }
            }
            catch (Exception ex)
            {
                Output += "Error : " + ex.Message;
            }
            return Output;
        }

        /// <summary>
        /// 取得資料庫底下的資料表索引
        /// </summary>
        /// <returns></returns>
        public List<Table_Index> Get_DbIndexTable()
        {
            List<Table_Index> dbList = new List<Table_Index>();


            try
            {
                string dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Alphabet.db");
                var db = new SQLiteConnection(dbPath);

                //-- ORM 取得資料內容
                var tablerows = db.Table<Table_Index>();

                foreach (var row in tablerows)
                {
                    Table_Index single = new Table_Index();
                    single.Guid = row.Guid;
                    single.TableName = row.TableName;
                    dbList.Add(single);
                }
            }
            catch (Exception ex)
            {
            }
            return dbList;
        }

        /// <summary>
        /// 取得資料表的資料內容
        /// </summary>
        /// <returns></returns>
        public List<Table_Row> Get_MatchData(string Guid_refect)
        {
            List<Table_Row> dbList = new List<Table_Row>();
            try
            {
                string dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Alphabet.db");
                var db = new SQLiteConnection(dbPath);
                List<Object> Getrows = db.Query(new TableMapping(typeof(Table_Row)), string.Format("Select * from {0}", Guid_refect));

                foreach (var row in Getrows)
                {
                    Table_Row single = new Table_Row();
                    //強制轉型 
                    single.Chinese = ((Table_Row)row).Chinese;
                    single.English = ((Table_Row)row).English;
                    dbList.Add(single);
                }
            }
            catch (Exception ex)
            {
            }
            return dbList;
        }

        public string Get_IndexTableReflectName(string Tablename)
        {
            try
            {
                string dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Alphabet.db");
                var db = new SQLiteConnection(dbPath);
                var tablerows = db.Table<Table_Index>();
                var get = tablerows.Where(o => o.TableName == Tablename).FirstOrDefault().Guid;
                return get;

            }
            catch (Exception ex)
            {
            }

            return "";
        }
    }
}