using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content.PM;
using AlphabetDictionaryMobile.Controller.NewToeic;

namespace AlphabetDictionaryMobile
{
    [Activity(Label = "AlphabetDictionaryMobile", MainLauncher = true, Icon = "@drawable/icon" , ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : Activity
    {

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it

            //前往多益單字書頁面
            FindViewById<Button>(Resource.Id.Main_Button_GONewToeic).Click += btn_Newtoiec;
        }

        #region delegate function

        /// <summary>
        /// 按下後前往新多益單字
        /// </summary>
        /// <param name="stender"></param>
        /// <param name="e"></param>
        void btn_Newtoiec(object stender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(NewToeicActivity));
            this.StartActivity(intent);
        }

        #endregion


    }
}

