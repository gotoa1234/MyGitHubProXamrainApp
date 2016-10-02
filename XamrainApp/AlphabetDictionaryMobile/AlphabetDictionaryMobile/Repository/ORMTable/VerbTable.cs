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
using SQLite;

namespace AlphabetDictionaryMobile.Repository.ORMTable
{
    //※ 如果透過SqliteORM技術 建立此Class(就是VerbTable)的資料表  會被命名為VerbTables
    [Table("VerbTables")]
    public class VerbTable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// 英文
        /// </summary>
        [MaxLength(300)]
        public string English { get; set; }

        /// <summary>
        /// 中文意思
        /// </summary>
        [MaxLength(300)]
        public string Chinese { get; set; }

        /// <summary>
        /// 種類分群 目前分:0,1,2 0:可加入動名詞或to + V 1:只可以加入動名詞 2:可以加入to + V
        /// </summary>
        [MaxLength(2)]
        public int Kind { get; set; }
    }
}