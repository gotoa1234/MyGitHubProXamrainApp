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

        #region �ܼ�

        //�ϥΪ̿�ܪ���ƪ�
        private List<Table_Row> _userSelectTable = new List<Table_Row>();
        //DB�W��
        private string _DbName = "Alphabet.db";
        //�ϥΪ̪������D��
        private int _Total = 20;//�`�����
        private int _NowCount = 0;//��e�����D�ؼ�
        private int _QuizRight = 0;//�����D��
        private int _RightQuestion = 0;//���ݳ�r�����T�ѵ�����
        
        //�y��
        private TextToSpeech _MySpeech;

        //�Ҹժ�20�ӳ�r
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
            //�}�l����
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
        /// Ū����Ʈw���
        /// </summary>
        public void Load_Data()
        {
            string Output = "";
            try
            {
                bool responsebool = false;
                //���oAssets�����귽
                try
                {
                    //NewToiec��Ʈw 
                    var Sourcepath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), _DbName);
                    using (var asset = Assets.Open(_DbName))
                    using (var dest = File.Create(Sourcepath))
                    {
                        asset.CopyTo(dest);
                    }
                    Output += string.Format("��Ʈw:{0} �إߤ�...", _DbName);
                }
                catch(Exception ex)
                {
                    Toast.MakeText(this, ex.Message, ToastLength.Short).Show();
                }
                
            }
            catch (Exception ex)
            {
                Output += "�L�kŪ����Ʈw Error : " + ex.Message;
            }
            Toast.MakeText(this, Output, ToastLength.Short).Show();
        }

        /// <summary>
        /// �]�wTable �i���Item����
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
        /// ��Table���U����rtrigger
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SpinTime_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            var Get_nowSelect = (Spinner)sender;
            DBNewToeic dbr = new DBNewToeic();

            //���o�Ӹ�ƪ����
            _userSelectTable = dbr.GetMatchData(dbr.GetIndexTableReflectName(Get_nowSelect.SelectedItem.ToString()));
            //spinner_mode
            var spinner_wordlist = FindViewById<Spinner>(Resource.Id.NewToeic_linearLayout_Bottom_tableLayout_tableRow1_spinner_Alphabetcountlist);
            List<string> spinnerItems = new List<string>();
            _userSelectTable.Count();
            //�N��r�ƶq����
            for (int i = 0; i < _userSelectTable.Count(); i++)
            {
                if (_userSelectTable.Count >= (i + 1) * 20)
                    spinnerItems.Add(string.Format("{0}.�^���{1}~{2}", i + 1, (i * 20) + 1, (i + 1) * 20));
                else
                {
                    spinnerItems.Add(string.Format("{0}.�^���r{1}~{2}", i + 1, (i * 20) + 1, _userSelectTable.Count));
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
        /// Newtoeic�����l��
        /// </summary>
        private void BtnQuizInitinal(object sender, EventArgs e)
        {
            try
            {
                #region ��l��
                  //�ܼ�
                _RightQuestion = 0;
                _QuizRight = 0;
                _NowCount = 0;
                FindViewById<TextView>(Resource.Id.NewToeic_linearLayout_Content2_tableLayout_tableRow1_textView_AnsRecord).Text = "";
                
                //�]�w20�D�D��
                _Total = buildingTable();

                FindViewById<TextView>(Resource.Id.NewToeic_linearLayout_Content2_tableLayout_tableRow1_textView_State).Text = string.Format("��e���A�G{0}/{1} �����:{2} ���T�v�G{3}", _NowCount, _Total, _QuizRight, 0);

                //�]�w�X�D
                SetQuestion();


                #endregion
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, "btn_quiz_initinal():���~�T��=> " + ex.Message, ToastLength.Long);
            }
        }

        /// <summary>
        /// �X�D
        /// </summary>
        private void SetQuestion()
        {
            try
            {
                //�X�D
                System.Random rnd = new System.Random(Guid.NewGuid().GetHashCode());

                //��@�ӥ���
                _RightQuestion = rnd.Next(0, 4);//-- min >=   max< NowDb.Count()
                
                //�]�w�e����ܤ�r
                FindViewById<TextView>(Resource.Id.NewToeic_linearLayout_top_textView_Question).Text = string.Format("{0}", this._UserQuizNewToeicTable[_NowCount].English);
                //�o�n��e�^��
                SpeakOut(this._UserQuizNewToeicTable[_NowCount].English);

                // MySpeech.Speak("abc", QueueMode.Add, null,null);
                //����H����m��
                List<int> Random_show = new List<int>();
                for (int i = 0; i < 4; i++)
                {
                    if (i == _RightQuestion)
                    {
                        Random_show.Add(_NowCount);
                        continue;
                    }

                    //�t�m������Right_Question���ƭ�
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
                Toast.MakeText(this, "SetQuestion():���~�T��=> " + ex.Message, ToastLength.Long);
            }
        }

        /// <summary>
        /// �سy�Ҹժ�20�ӳ�r�w
        /// </summary>
        /// <returns>�^�ǸӦ���r��</returns>
        private int buildingTable()
        {
            int count = 0;
            this._UserQuizNewToeicTable = new List<Table_Row>();
            //���o��e��ܪ���r��
            int wordindex = (int)FindViewById<Spinner>(Resource.Id.NewToeic_linearLayout_Bottom_tableLayout_tableRow1_spinner_Alphabetcountlist).SelectedItemId;
            for (int i = (wordindex * 20); i < (wordindex * 20) + 20; i++)
            {
                //���i�H�W�L
                if (i >= this._userSelectTable.Count())
                    break;

                _UserQuizNewToeicTable.Add(_userSelectTable[i]);
                count++;
            }
            //�üƸ̭������
            this._UserQuizNewToeicTable = _UserQuizNewToeicTable.OrderBy(o => System.Guid.NewGuid().ToString()).ToList();

            return count;
        }

        #endregion

        #region Event delegate

        /// <summary>
        /// �^��A 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btn_answerA(object sender, EventArgs e)
        {
            try
            {
                if (_NowCount < _Total)
                {
                    //�p�G����|+1 �����O0 ���|�W�[
                    _QuizRight += this.Btn_ansReflect(_RightQuestion,0);
                    
                    //���ܷ�e���A
                    _NowCount++;
                    FindViewById<TextView>(Resource.Id.NewToeic_linearLayout_Content2_tableLayout_tableRow1_textView_State).Text = string.Format("��e�i�סG{0}/{1} \r\n�����:{2} \r\n���T�v�G{3} \r\n�D�w�ơG{4}\r\n", _NowCount, _Total, _QuizRight, (Math.Round(((double)((float)_QuizRight / (float)_NowCount) * 100), 1).ToString() + "%"), _Total);

                }
                //�X�U�@�D��
                if (_NowCount < _Total)
                    SetQuestion();
                else
                    Toast.MakeText(this, "�w�g�����D�ؤF�A�Ы����s���շs���}�l", ToastLength.Long).Show();
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, "btn_answerA():���~�T��=> " + ex.Message, ToastLength.Long);
            }
        }

        /// <summary>
        /// �^��B 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btn_answerB(object sender, EventArgs e)
        {

            try
            {
                if (_NowCount < _Total)
                {

                    //�p�G����|+1 �����O0 ���|�W�[
                    _QuizRight += this.Btn_ansReflect(_RightQuestion, 1);

                    //���ܷ�e���A
                    _NowCount++;
                    FindViewById<TextView>(Resource.Id.NewToeic_linearLayout_Content2_tableLayout_tableRow1_textView_State).Text = string.Format("��e�i�סG{0}/{1} \r\n�����:{2} \r\n���T�v�G{3} \r\n�D�w�ơG{4}\r\n", _NowCount, _Total, _QuizRight, (Math.Round(((double)((float)_QuizRight / (float)_NowCount) * 100), 1).ToString() + "%"), _Total);

                }
                //�X�U�@�D��
                if (_NowCount < _Total)
                    SetQuestion();
                else
                    Toast.MakeText(this, "�w�g�����D�ؤF�A�Ы����s���շs���}�l", ToastLength.Long).Show();
            }
            catch (Exception ex)
            {

                Toast.MakeText(this, "btn_answerB():���~�T��=> " + ex.Message, ToastLength.Long);
            }
        }

        /// <summary>
        /// �^��C
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btn_answerC(object sender, EventArgs e)
        {
            try
            {
                if (_NowCount < _Total)
                {

                    //�p�G����|+1 �����O0 ���|�W�[
                    _QuizRight += this.Btn_ansReflect(_RightQuestion, 2);

                    //���ܷ�e���A
                    _NowCount++;
                    FindViewById<TextView>(Resource.Id.NewToeic_linearLayout_Content2_tableLayout_tableRow1_textView_State).Text = string.Format("��e�i�סG{0}/{1} \r\n�����:{2} \r\n���T�v�G{3} \r\n�D�w�ơG{4}\r\n", _NowCount, _Total, _QuizRight, (Math.Round(((double)((float)_QuizRight / (float)_NowCount) * 100), 1).ToString() + "%"), _Total);

                }
                //�X�U�@�D��
                if (_NowCount < _Total)
                    SetQuestion();
                else
                    Toast.MakeText(this, "�w�g�����D�ؤF�A�Ы����s���շs���}�l", ToastLength.Long).Show();
            }
            catch (Exception ex)
            {

                Toast.MakeText(this, "btn_answerC():���~�T��=> " + ex.Message, ToastLength.Long);
            }
        }

        /// <summary>
        /// �^��D
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btn_answerD(object sender, EventArgs e)
        {
            try
            {
                if (_NowCount < _Total)
                {

                    //�p�G����|+1 �����O0 ���|�W�[
                    _QuizRight += this.Btn_ansReflect(_RightQuestion, 3);

                    //���ܷ�e���A
                    _NowCount++;
                    FindViewById<TextView>(Resource.Id.NewToeic_linearLayout_Content2_tableLayout_tableRow1_textView_State).Text = string.Format("��e�i�סG{0}/{1} \r\n�����:{2} \r\n���T�v�G{3} \r\n�D�w�ơG{4}\r\n", _NowCount, _Total, _QuizRight, (Math.Round(((double)((float)_QuizRight / (float)_NowCount) * 100), 1).ToString() + "%"), _Total);

                }
                //�X�U�@�D��
                if (_NowCount < _Total)
                    SetQuestion();
                else
                    Toast.MakeText(this, "�w�g�����D�ؤF�A�Ы����s���շs���}�l", ToastLength.Long).Show();
            }
            catch (Exception ex)
            {

                Toast.MakeText(this, "btn_answerD():���~�T��=> " + ex.Message, ToastLength.Long);
            }
        }

        /// <summary>
        /// �^��ABCD ���Ӥ~�O����
        /// </summary>
        /// <param name="kind">���T����</param>
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
        /// �N�^�������G�i��O��
        /// </summary>
        /// <param name="right_num">���T���ת��Ʀr</param>
        /// <param name="self_num">�ۨ��Ҧb���Ʀr</param>
        /// <returns></returns>
        private int Btn_ansReflect(int right_num , int self_num)
        {
            //����
            if (right_num == self_num)
            {
                //Toast.MakeText(this, string.Format("����F {0}", FindViewById<Button>(Resource.Id.NewToeic_linearLayout_Content1_button_AnsA).Text), ToastLength.Short).Show();
                //�����諸����
                FindViewById<TextView>(Resource.Id.NewToeic_linearLayout_Content2_tableLayout_tableRow1_textView_AnsRecord).Text = (string.Format("�i�ݡj=> {0}\r\n", reflectRightAnswer() ));
                return 1;
            }
            else//����
            {
                //Toast.MakeText(this, "�^�����~�A����:" + reflectRightAnswer(), ToastLength.Short).Show();
                //�O����������
                FindViewById<TextView>(Resource.Id.NewToeic_linearLayout_Content2_tableLayout_tableRow1_textView_AnsRecord).Text = (string.Format("�i�A�j=> {0}\r\n", reflectRightAnswer()));
                return 0;
            }
        }

        #endregion

        /// <summary>
        /// ��@��r��y���\��
        /// </summary>
        /// <param name="status"></param>
        public void OnInit([GeneratedEnum] OperationResult status)
        {
            if (status == OperationResult.Success)
            {
                //�p�G�\��Q�ե� - �]�w�y����m-�Хέ^��
                _MySpeech.SetLanguage(Locale.Us);
            }
        }
        /// <summary>
        /// �o�X�y��
        /// </summary>
        /// <param name="text"></param>
        private void SpeakOut(string text)
        {
            var p = new Dictionary<string, string>();
            _MySpeech.Speak(text, QueueMode.Add, p);
        }
    }


}