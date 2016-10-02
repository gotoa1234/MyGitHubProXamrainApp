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
    public class Table_Row
    {
        /// <summary>
        /// �^��
        /// </summary>
        [MaxLength(300)]
        public string English { get; set; }

        /// <summary>
        /// ����N��
        /// </summary>
        [MaxLength(300)]
        public string Chinese { get; set; }

    }
}