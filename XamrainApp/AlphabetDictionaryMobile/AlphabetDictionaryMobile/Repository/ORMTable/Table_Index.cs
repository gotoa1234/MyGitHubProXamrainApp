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
    /// <summary>
    /// �h�q���޸�ƪ�
    /// </summary>
    [Table("Table_Index")]
    public class Table_Index
    {
        public string Guid { get; set; }

        public string TableName { get; set; }
    }
}