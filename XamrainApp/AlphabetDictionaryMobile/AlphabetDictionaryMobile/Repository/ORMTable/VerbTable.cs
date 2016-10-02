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
    //�� �p�G�z�LSqliteORM�޳N �إߦ�Class(�N�OVerbTable)����ƪ�  �|�Q�R�W��VerbTables
    [Table("VerbTables")]
    public class VerbTable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

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

        /// <summary>
        /// �������s �ثe��:0,1,2 0:�i�[�J�ʦW����to + V 1:�u�i�H�[�J�ʦW�� 2:�i�H�[�Jto + V
        /// </summary>
        [MaxLength(2)]
        public int Kind { get; set; }
    }
}