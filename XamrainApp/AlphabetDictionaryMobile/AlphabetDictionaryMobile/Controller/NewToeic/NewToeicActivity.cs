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
using Android.Content.PM;
using AlphabetDictionaryMobile.Repository.ORMTable;
using AlphabetDictionaryMobile.Repository;
using System.IO;
using SQLite;
using Android.Speech.Tts;
using Java.Util;
using System.Net;
using Org.Apache.Http.Protocol;

namespace AlphabetDictionaryMobile.Controller.NewToeic
{
    [Activity(Label = "NewToeicActivity", ScreenOrientation = ScreenOrientation.Portrait) ]
    public class NewToeicActivity : Activity, TextToSpeech.IOnInitListener
    {

        #region 變數

        //使用者選擇的資料表
        private List<Table_Row> _userSelectTable = new List<Table_Row>();
        //DB名稱
        private string _DbName = "Alphabet.db";
        //使用者的測驗題數
        private int _Total = 20;//總測驗數
        private int _NowCount = 0;//當前測驗題目數
        private int _QuizRight = 0;//答對題數
        private int _RightQuestion = 0;//提問單字的正確解答項目
        
        //語音
        private TextToSpeech _MySpeech;

        //考試的20個單字
        private List<Table_Row> _UserQuizNewToeicTable = new List<Table_Row>();
        #endregion
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.NewToeic);

            // initinal
            _MySpeech = new TextToSpeech(this, this);

            // SpeakOut();
            Load_Data();
            SetSpinItem();

            // Create your application here 
            //開始測驗
            Button btn_quizinitinal = FindViewById<Button>(Resource.Id.NewToeic_linearLayout_Bottom_tableLayout_tableRow2_button_StartQuiz);
            btn_quizinitinal.Click += this.BtnQuizInitinal;

            //delegate
            FindViewById<Button>(Resource.Id.NewToeic_linearLayout_Content1_button_AnsA).Click += btn_answerA;
            FindViewById<Button>(Resource.Id.NewToeic_linearLayout_Content1_button_AnsB).Click += btn_answerB;
            FindViewById<Button>(Resource.Id.NewToeic_linearLayout_Content1_button_AnsC).Click += btn_answerC;
            FindViewById<Button>(Resource.Id.NewToeic_linearLayout_Content1_button_AnsD).Click += btn_answerD;
        }

        #region function

        /// <summary>
        /// 讀取資料庫資料
        /// </summary>
        public void Load_Data()
        {
            string Output = "";
            try
            {
                bool responsebool = false;
                //取得Assets內的資源
                try
                {
                    //NewToiec資料庫 
                    var Sourcepath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), _DbName);
                    using (var asset = Assets.Open(_DbName))
                    using (var dest = File.Create(Sourcepath))
                    {
                        asset.CopyTo(dest);
                    }
                    Output += string.Format("資料庫:{0} 建立中...", _DbName);
                }
                catch(Exception ex)
                {
                    Toast.MakeText(this, ex.Message, ToastLength.Short).Show();
                }
                
            }
            catch (Exception ex)
            {
                Output += "無法讀取資料庫 Error : " + ex.Message;
            }
            Toast.MakeText(this, Output, ToastLength.Short).Show();
        }

        /// <summary>
        /// 設定Table 可選擇Item項目
        /// </summary>
        public void SetSpinItem()
        {
            //table data
            DBNewToeic dbr = new DBNewToeic();
            var get_table = dbr.Get_DbIndexTable();
            //spinner_mode
            var spinTime = FindViewById<Spinner>(Resource.Id.NewToeic_linearLayout_Bottom_tableLayout_tableRow1_spinner_Tableindex);
            List<string> spinnerItems = new List<string>();
            foreach (var objection in get_table)
            {
                spinnerItems.Add(objection.TableName);
            }
            ArrayAdapter adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, spinnerItems);

            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinTime.Adapter = adapter;

            spinTime.ItemSelected += SpinTime_ItemSelected;
        }

        /// <summary>
        /// 該Table底下的單字trigger
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SpinTime_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            var Get_nowSelect = (Spinner)sender;
            DBNewToeic dbr = new DBNewToeic();

            //取得該資料表的資料
            _userSelectTable = dbr.GetMatchData(dbr.GetIndexTableReflectName(Get_nowSelect.SelectedItem.ToString()));
            //spinner_mode
            var spinner_wordlist = FindViewById<Spinner>(Resource.Id.NewToeic_linearLayout_Bottom_tableLayout_tableRow1_spinner_Alphabetcountlist);
            List<string> spinnerItems = new List<string>();
            _userSelectTable.Count();
            //將單字數量分化
            for (int i = 0; i < _userSelectTable.Count(); i++)
            {
                if (_userSelectTable.Count >= (i + 1) * 20)
                    spinnerItems.Add(string.Format("{0}.英文單{1}~{2}", i + 1, (i * 20) + 1, (i + 1) * 20));
                else
                {
                    spinnerItems.Add(string.Format("{0}.英文單字{1}~{2}", i + 1, (i * 20) + 1, _userSelectTable.Count));
                    break;
                }
            }
            ArrayAdapter adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, spinnerItems);

            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinner_wordlist.Adapter = adapter;

        }


        #endregion

        #region Quiz initinal


        /// <summary>
        /// Newtoeic測驗初始化
        /// </summary>
        private void BtnQuizInitinal(object sender, EventArgs e)
        {
            try
            {
                #region 初始化
                  //變數
                _RightQuestion = 0;
                _QuizRight = 0;
                _NowCount = 0;
                FindViewById<TextView>(Resource.Id.NewToeic_linearLayout_Content2_tableLayout_tableRow1_textView_AnsRecord).Text = "";
                
                //設定20道題目
                _Total = buildingTable();

                FindViewById<TextView>(Resource.Id.NewToeic_linearLayout_Content2_tableLayout_tableRow1_textView_State).Text = string.Format("當前狀態：{0}/{1} 答對數:{2} 正確率：{3}", _NowCount, _Total, _QuizRight, 0);

                //設定出題
                SetQuestion();


                #endregion
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, "btn_quiz_initinal():錯誤訊息=> " + ex.Message, ToastLength.Long);
            }
        }

        /// <summary>
        /// 出題
        /// </summary>
        private void SetQuestion()
        {
            try
            {
                //出題
                System.Random rnd = new System.Random(Guid.NewGuid().GetHashCode());

                //選一個正解
                _RightQuestion = rnd.Next(0, 4);//-- min >=   max< NowDb.Count()
                
                //設定畫面顯示文字
                FindViewById<TextView>(Resource.Id.NewToeic_linearLayout_top_textView_Question).Text = string.Format("{0}", this._UserQuizNewToeicTable[_NowCount].English);
                //發聲當前英文
                SpeakOut(this._UserQuizNewToeicTable[_NowCount].English);

                // MySpeech.Speak("abc", QueueMode.Add, null,null);
                //顯示隨機放置數
                List<int> Random_show = new List<int>();
                for (int i = 0; i < 4; i++)
                {
                    if (i == _RightQuestion)
                    {
                        Random_show.Add(_NowCount);
                        continue;
                    }

                    //配置不等於Right_Question的數值
                    for (int obj = 0; ;)
                    {
                        obj = rnd.Next(0, _Total);
                        if (obj != _NowCount && !(Random_show.Exists(o => o == obj)))
                        {
                            Random_show.Add(obj);
                            break;
                        }
                    }
                }
                FindViewById<Button>(Resource.Id.NewToeic_linearLayout_Content1_button_AnsA).Text = this._UserQuizNewToeicTable[Random_show[0]].Chinese;
                FindViewById<Button>(Resource.Id.NewToeic_linearLayout_Content1_button_AnsB).Text = this._UserQuizNewToeicTable[Random_show[1]].Chinese;
                FindViewById<Button>(Resource.Id.NewToeic_linearLayout_Content1_button_AnsC).Text = this._UserQuizNewToeicTable[Random_show[2]].Chinese;
                FindViewById<Button>(Resource.Id.NewToeic_linearLayout_Content1_button_AnsD).Text = this._UserQuizNewToeicTable[Random_show[3]].Chinese;

            }
            catch (Exception ex)
            {
                Toast.MakeText(this, "SetQuestion():錯誤訊息=> " + ex.Message, ToastLength.Long);
            }
        }

        /// <summary>
        /// 建造考試的20個單字庫
        /// </summary>
        /// <returns>回傳該次單字數</returns>
        private int buildingTable()
        {
            int count = 0;
            this._UserQuizNewToeicTable = new List<Table_Row>();
            //取得當前選擇的單字組
            int wordindex = (int)FindViewById<Spinner>(Resource.Id.NewToeic_linearLayout_Bottom_tableLayout_tableRow1_spinner_Alphabetcountlist).SelectedItemId;
            for (int i = (wordindex * 20); i < (wordindex * 20) + 20; i++)
            {
                //不可以超過
                if (i >= this._userSelectTable.Count())
                    break;

                _UserQuizNewToeicTable.Add(_userSelectTable[i]);
                count++;
            }
            //亂數裡面的資料
            this._UserQuizNewToeicTable = _UserQuizNewToeicTable.OrderBy(o => System.Guid.NewGuid().ToString()).ToList();

            return count;
        }

        #endregion

        #region Event delegate

        /// <summary>
        /// 回答A 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btn_answerA(object sender, EventArgs e)
        {
            try
            {
                if (_NowCount < _Total)
                {
                    //如果答對會+1 答錯是0 不會增加
                    _QuizRight += this.Btn_ansReflect(_RightQuestion,0);
                    
                    //改變當前狀態
                    _NowCount++;
                    FindViewById<TextView>(Resource.Id.NewToeic_linearLayout_Content2_tableLayout_tableRow1_textView_State).Text = string.Format("當前進度：{0}/{1} \r\n答對數:{2} \r\n正確率：{3} \r\n題庫數：{4}\r\n", _NowCount, _Total, _QuizRight, (Math.Round(((double)((float)_QuizRight / (float)_NowCount) * 100), 1).ToString() + "%"), _Total);

                }
                //出下一題目
                if (_NowCount < _Total)
                    SetQuestion();
                else
                    Toast.MakeText(this, "已經完成題目了，請按重新測試新的開始", ToastLength.Long).Show();
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, "btn_answerA():錯誤訊息=> " + ex.Message, ToastLength.Long);
            }
        }

        /// <summary>
        /// 回答B 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btn_answerB(object sender, EventArgs e)
        {

            try
            {
                if (_NowCount < _Total)
                {

                    //如果答對會+1 答錯是0 不會增加
                    _QuizRight += this.Btn_ansReflect(_RightQuestion, 1);

                    //改變當前狀態
                    _NowCount++;
                    FindViewById<TextView>(Resource.Id.NewToeic_linearLayout_Content2_tableLayout_tableRow1_textView_State).Text = string.Format("當前進度：{0}/{1} \r\n答對數:{2} \r\n正確率：{3} \r\n題庫數：{4}\r\n", _NowCount, _Total, _QuizRight, (Math.Round(((double)((float)_QuizRight / (float)_NowCount) * 100), 1).ToString() + "%"), _Total);

                }
                //出下一題目
                if (_NowCount < _Total)
                    SetQuestion();
                else
                    Toast.MakeText(this, "已經完成題目了，請按重新測試新的開始", ToastLength.Long).Show();
            }
            catch (Exception ex)
            {

                Toast.MakeText(this, "btn_answerB():錯誤訊息=> " + ex.Message, ToastLength.Long);
            }
        }

        /// <summary>
        /// 回答C
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btn_answerC(object sender, EventArgs e)
        {
            try
            {
                if (_NowCount < _Total)
                {

                    //如果答對會+1 答錯是0 不會增加
                    _QuizRight += this.Btn_ansReflect(_RightQuestion, 2);

                    //改變當前狀態
                    _NowCount++;
                    FindViewById<TextView>(Resource.Id.NewToeic_linearLayout_Content2_tableLayout_tableRow1_textView_State).Text = string.Format("當前進度：{0}/{1} \r\n答對數:{2} \r\n正確率：{3} \r\n題庫數：{4}\r\n", _NowCount, _Total, _QuizRight, (Math.Round(((double)((float)_QuizRight / (float)_NowCount) * 100), 1).ToString() + "%"), _Total);

                }
                //出下一題目
                if (_NowCount < _Total)
                    SetQuestion();
                else
                    Toast.MakeText(this, "已經完成題目了，請按重新測試新的開始", ToastLength.Long).Show();
            }
            catch (Exception ex)
            {

                Toast.MakeText(this, "btn_answerC():錯誤訊息=> " + ex.Message, ToastLength.Long);
            }
        }

        /// <summary>
        /// 回答D
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btn_answerD(object sender, EventArgs e)
        {
            try
            {
                if (_NowCount < _Total)
                {

                    //如果答對會+1 答錯是0 不會增加
                    _QuizRight += this.Btn_ansReflect(_RightQuestion, 3);

                    //改變當前狀態
                    _NowCount++;
                    FindViewById<TextView>(Resource.Id.NewToeic_linearLayout_Content2_tableLayout_tableRow1_textView_State).Text = string.Format("當前進度：{0}/{1} \r\n答對數:{2} \r\n正確率：{3} \r\n題庫數：{4}\r\n", _NowCount, _Total, _QuizRight, (Math.Round(((double)((float)_QuizRight / (float)_NowCount) * 100), 1).ToString() + "%"), _Total);

                }
                //出下一題目
                if (_NowCount < _Total)
                    SetQuestion();
                else
                    Toast.MakeText(this, "已經完成題目了，請按重新測試新的開始", ToastLength.Long).Show();
            }
            catch (Exception ex)
            {

                Toast.MakeText(this, "btn_answerD():錯誤訊息=> " + ex.Message, ToastLength.Long);
            }
        }

        /// <summary>
        /// 回應ABCD 哪個才是正解
        /// </summary>
        /// <param name="kind">正確答案</param>
        /// <returns></returns>
        private string reflectRightAnswer()
        {
            if (_RightQuestion == 0)
                return FindViewById<TextView>(Resource.Id.NewToeic_linearLayout_Content1_button_AnsA).Text;
            else if (_RightQuestion == 1)
                return FindViewById<TextView>(Resource.Id.NewToeic_linearLayout_Content1_button_AnsB).Text;
            else if (_RightQuestion == 2)
                return FindViewById<TextView>(Resource.Id.NewToeic_linearLayout_Content1_button_AnsC).Text;
            else
                return FindViewById<TextView>(Resource.Id.NewToeic_linearLayout_Content1_button_AnsD).Text;
        }

        /// <summary>
        /// 將回答的結果進行記錄
        /// </summary>
        /// <param name="right_num">正確答案的數字</param>
        /// <param name="self_num">自身所在的數字</param>
        /// <returns></returns>
        private int Btn_ansReflect(int right_num , int self_num)
        {
            //答對
            if (right_num == self_num)
            {
                //Toast.MakeText(this, string.Format("答對了 {0}", FindViewById<Button>(Resource.Id.NewToeic_linearLayout_Content1_button_AnsA).Text), ToastLength.Short).Show();
                //紀錄對的答案
                FindViewById<TextView>(Resource.Id.NewToeic_linearLayout_Content2_tableLayout_tableRow1_textView_AnsRecord).Text = (string.Format("【Ｏ】=> {0}\r\n", reflectRightAnswer() ));
                return 1;
            }
            else//答錯
            {
                //Toast.MakeText(this, "回答錯誤，正解:" + reflectRightAnswer(), ToastLength.Short).Show();
                //記錄錯的答案
                FindViewById<TextView>(Resource.Id.NewToeic_linearLayout_Content2_tableLayout_tableRow1_textView_AnsRecord).Text = (string.Format("【ｘ】=> {0}\r\n", reflectRightAnswer()));
                return 0;
            }
        }

        #endregion

        /// <summary>
        /// 實作文字轉語音功能
        /// </summary>
        /// <param name="status"></param>
        public void OnInit([GeneratedEnum] OperationResult status)
        {
            if (status == OperationResult.Success)
            {
                //如果功能被調用 - 設定語音位置-請用英文
                _MySpeech.SetLanguage(Locale.Us);
            }
        }
        /// <summary>
        /// 發出語音
        /// </summary>
        /// <param name="text"></param>
        private void SpeakOut(string text)
        {
            var p = new Dictionary<string, string>();
            _MySpeech.Speak(text, QueueMode.Add, p);
        }
    }


}