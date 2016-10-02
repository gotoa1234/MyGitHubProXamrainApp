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
        private List<Table_Row> user_SelectTable = new List<Table_Row>();
        //DB�W��
        private string DBname = "Alphabet.db";
        //�ϥΪ̪������D��
        private int Total = 20;//�`�����
        private int nowcount = 0;//��e�����D�ؼ�
        private int quizright = 0;//�����D��
        private int Right_Question = 0;//���ݳ�r�����T�ѵ�����
        private int SetMode = 0;//��e�]�w�Ҧ�
        //�y��
        private TextToSpeech Myspeech;

        //�Ҹժ�20�ӳ�r
        private List<Table_Row> user_QuizNewtoeicTable = new List<Table_Row>();
        #endregion
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.NewToeic);

            // initinal
            Myspeech = new TextToSpeech(this, this);


           // SpeakOut();
            Load_Data();
            Set_spinitem();

            // Create your application here 
            //�}�l����
            Button btn_quizinitinal = FindViewById<Button>(Resource.Id.NewToeic_linearLayout_Bottom_tableLayout_tableRow2_button_StartQuiz);
            btn_quizinitinal.Click += this.btn_quiz_initinal;

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
                //build new db
                string dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), DBname);
                var db = new SQLiteConnection(dbPath);
                bool responsebool = false;
                //���o�u�W�������
                try
                {
                    ServiceConnection.ServiceConnection ServiceGet = new ServiceConnection.ServiceConnection();
                    if (responsebool = ServiceGet.Get_DB())
                    {
                        string msg = ServiceGet.Get_Version();
                        Output += string.Format("��Ʈw:{0} ���o���\", DBname);
                    }
                }
                catch
                {

                }

                if (responsebool == false)//���Service������ƥ��ѡA�ϥ�app�W���Ѫ���Ʈw
                {
                    //NewToiec��Ʈw 
                    var Sourcepath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), DBname);
                    using (var asset = Assets.Open(DBname))
                    using (var dest = File.Create(Sourcepath))
                    {
                        asset.CopyTo(dest);
                    }
                    Output += string.Format("��Ʈw:{0} �إߤ�...", DBname);
                }
                
            }
            catch (Exception ex)
            {
                Output += "Error : " + ex.Message;
            }
            Toast.MakeText(this, Output, ToastLength.Short).Show();
        }

        /// <summary>
        /// �]�wTable �i���Item����
        /// </summary>
        public void Set_spinitem()
        {
            //table data
            DB_NewToeic dbr = new DB_NewToeic();
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
            DB_NewToeic dbr = new DB_NewToeic();

            //���o�Ӹ�ƪ����
            user_SelectTable = dbr.Get_MatchData(dbr.Get_IndexTableReflectName(Get_nowSelect.SelectedItem.ToString()));
            //spinner_mode
            var spinner_wordlist = FindViewById<Spinner>(Resource.Id.NewToeic_linearLayout_Bottom_tableLayout_tableRow1_spinner_Alphabetcountlist);
            List<string> spinnerItems = new List<string>();
            user_SelectTable.Count();
            //�N��r�ƶq����
            for (int i = 0; i < user_SelectTable.Count(); i++)
            {
                if (user_SelectTable.Count >= (i + 1) * 20)
                    spinnerItems.Add(string.Format("{0}.�^���{1}~{2}", i + 1, (i * 20) + 1, (i + 1) * 20));
                else
                {
                    spinnerItems.Add(string.Format("{0}.�^���r{1}~{2}", i + 1, (i * 20) + 1, user_SelectTable.Count));
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
        void btn_quiz_initinal(object sender, EventArgs e)
        {
            try
            {
               
                #region ��l��
                  //�ܼ�
                Right_Question = 0;
                quizright = 0;
                nowcount = 0;
                FindViewById<TextView>(Resource.Id.NewToeic_linearLayout_Content2_tableLayout_tableRow1_textView_AnsRecord).Text = "";
                
                //�]�w20�D�D��
                Total = buildingTable();

                FindViewById<TextView>(Resource.Id.NewToeic_linearLayout_Content2_tableLayout_tableRow1_textView_State).Text = string.Format("��e���A�G{0}/{1} �����:{2} ���T�v�G{3}", nowcount, Total, quizright, 0);

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
                Right_Question = rnd.Next(0, 4);//-- min >=   max< NowDb.Count()
                
                //�]�w�e����ܤ�r
                FindViewById<TextView>(Resource.Id.NewToeic_linearLayout_top_textView_Question).Text = string.Format("{0}", this.user_QuizNewtoeicTable[nowcount].English);
                //�o�n��e�^��
                SpeakOut(this.user_QuizNewtoeicTable[nowcount].English);

                // MySpeech.Speak("abc", QueueMode.Add, null,null);
                //����H����m��
                List<int> Random_show = new List<int>();
                for (int i = 0; i < 4; i++)
                {
                    if (i == Right_Question)
                    {
                        Random_show.Add(nowcount);
                        continue;
                    }

                    //�t�m������Right_Question���ƭ�
                    for (int obj = 0; ;)
                    {
                        obj = rnd.Next(0, Total);
                        if (obj != nowcount && !(Random_show.Exists(o => o == obj)))
                        {
                            Random_show.Add(obj);
                            break;
                        }
                    }
                }
                FindViewById<Button>(Resource.Id.NewToeic_linearLayout_Content1_button_AnsA).Text = this.user_QuizNewtoeicTable[Random_show[0]].Chinese;
                FindViewById<Button>(Resource.Id.NewToeic_linearLayout_Content1_button_AnsB).Text = this.user_QuizNewtoeicTable[Random_show[1]].Chinese;
                FindViewById<Button>(Resource.Id.NewToeic_linearLayout_Content1_button_AnsC).Text = this.user_QuizNewtoeicTable[Random_show[2]].Chinese;
                FindViewById<Button>(Resource.Id.NewToeic_linearLayout_Content1_button_AnsD).Text = this.user_QuizNewtoeicTable[Random_show[3]].Chinese;

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
            this.user_QuizNewtoeicTable = new List<Table_Row>();
            //���o��e��ܪ���r��
            int wordindex = (int)FindViewById<Spinner>(Resource.Id.NewToeic_linearLayout_Bottom_tableLayout_tableRow1_spinner_Alphabetcountlist).SelectedItemId;
            for (int i = (wordindex * 20); i < (wordindex * 20) + 20; i++)
            {
                //���i�H�W�L
                if (i >= this.user_SelectTable.Count())
                    break;

                user_QuizNewtoeicTable.Add(user_SelectTable[i]);
                count++;
            }
            //�üƸ̭������
            this.user_QuizNewtoeicTable = user_QuizNewtoeicTable.OrderBy(o => System.Guid.NewGuid().ToString()).ToList();

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
                if (nowcount < Total)
                {
                    //�p�G����|+1 �����O0 ���|�W�[
                    quizright += this.Btn_ansReflect(Right_Question,0);
                    
                    //���ܷ�e���A
                    nowcount++;
                    FindViewById<TextView>(Resource.Id.NewToeic_linearLayout_Content2_tableLayout_tableRow1_textView_State).Text = string.Format("��e�i�סG{0}/{1} \r\n�����:{2} \r\n���T�v�G{3} \r\n�D�w�ơG{4}\r\n", nowcount, Total, quizright, (Math.Round(((double)((float)quizright / (float)nowcount) * 100), 1).ToString() + "%"), Total);

                }
                //�X�U�@�D��
                if (nowcount < Total)
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
                if (nowcount < Total)
                {

                    //�p�G����|+1 �����O0 ���|�W�[
                    quizright += this.Btn_ansReflect(Right_Question, 1);

                    //���ܷ�e���A
                    nowcount++;
                    FindViewById<TextView>(Resource.Id.NewToeic_linearLayout_Content2_tableLayout_tableRow1_textView_State).Text = string.Format("��e�i�סG{0}/{1} \r\n�����:{2} \r\n���T�v�G{3} \r\n�D�w�ơG{4}\r\n", nowcount, Total, quizright, (Math.Round(((double)((float)quizright / (float)nowcount) * 100), 1).ToString() + "%"), Total);

                }
                //�X�U�@�D��
                if (nowcount < Total)
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
                if (nowcount < Total)
                {

                    //�p�G����|+1 �����O0 ���|�W�[
                    quizright += this.Btn_ansReflect(Right_Question, 2);

                    //���ܷ�e���A
                    nowcount++;
                    FindViewById<TextView>(Resource.Id.NewToeic_linearLayout_Content2_tableLayout_tableRow1_textView_State).Text = string.Format("��e�i�סG{0}/{1} \r\n�����:{2} \r\n���T�v�G{3} \r\n�D�w�ơG{4}\r\n", nowcount, Total, quizright, (Math.Round(((double)((float)quizright / (float)nowcount) * 100), 1).ToString() + "%"), Total);

                }
                //�X�U�@�D��
                if (nowcount < Total)
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
                if (nowcount < Total)
                {

                    //�p�G����|+1 �����O0 ���|�W�[
                    quizright += this.Btn_ansReflect(Right_Question, 3);

                    //���ܷ�e���A
                    nowcount++;
                    FindViewById<TextView>(Resource.Id.NewToeic_linearLayout_Content2_tableLayout_tableRow1_textView_State).Text = string.Format("��e�i�סG{0}/{1} \r\n�����:{2} \r\n���T�v�G{3} \r\n�D�w�ơG{4}\r\n", nowcount, Total, quizright, (Math.Round(((double)((float)quizright / (float)nowcount) * 100), 1).ToString() + "%"), Total);

                }
                //�X�U�@�D��
                if (nowcount < Total)
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
            if (Right_Question == 0)
                return FindViewById<TextView>(Resource.Id.NewToeic_linearLayout_Content1_button_AnsA).Text;
            else if (Right_Question == 1)
                return FindViewById<TextView>(Resource.Id.NewToeic_linearLayout_Content1_button_AnsB).Text;
            else if (Right_Question == 2)
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
                Myspeech.SetLanguage(Locale.Us);
            }
        }
        /// <summary>
        /// �o�X�y��
        /// </summary>
        /// <param name="text"></param>
        private void SpeakOut(string text)
        {
            var p = new Dictionary<string, string>();
            Myspeech.Speak(text, QueueMode.Add, p);
        }
    }


}