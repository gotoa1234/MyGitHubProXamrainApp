using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content.PM;
using AlphabetDictionaryMobile.Controller.NewToeic;
using Android;

namespace AlphabetDictionaryMobile
{
    [Activity(Label = "AlphabetDictionaryMobile", MainLauncher = true, Icon = "@drawable/icon" , ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : Activity
    {

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            //檢查權限
            CheckAppPermissions();
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

        private void CheckAppPermissions()
        {
            if ((int)Build.VERSION.SdkInt < 23)
            {
                return;
            }
            else
            {
                if (PackageManager.CheckPermission(Manifest.Permission.ReadExternalStorage, PackageName) != Permission.Granted
                    && PackageManager.CheckPermission(Manifest.Permission.WriteExternalStorage, PackageName) != Permission.Granted)
                {
                    var permissions = new string[] { Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage };
                    RequestPermissions(permissions, 1);
                }
            }
        }

        #endregion


    }
}

