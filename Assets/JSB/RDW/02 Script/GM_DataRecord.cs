using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

using System.Text;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _OSP
{

    public class GM_DataRecord : MonoBehaviour
    {

        public static GM_DataRecord instance = null;

        private string rootpath = string.Empty;
        private string folder_Path = string.Empty;

        private string folderName = "CGnA_DataLog";
        private string fileName = string.Empty;

        private string str_DataCategory = string.Empty;

        [HideInInspector]
        public enum Experiment_Type { VS_Line };

        [SerializeField]
        private Experiment_Type curren_EX_Type;
        public Experiment_Type Curren_EX_Type { get { return curren_EX_Type; } }

        private float startTime = 0.0f;
        private float currentTime = 0.0f;

        [HideInInspector]
        public enum Warning_Type
        {
            Ex_Start,
            Ex_End,
            TotalResetMean,
            User00ResetMean,
            User01ResetMean,
            UserbetResetMean,
            UsershutterResetMean,
            MeanDistBetResets
        }

        [HideInInspector]
        public Queue<string> Queue_EX_DATA = new Queue<string>();
        [HideInInspector]

        public bool isCategoryPrinted;

        [SerializeField]
        private GameObject real_HMD;

        [SerializeField]
        private GameObject AppQuit_Pref;

        //-------------------------------

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }

            MakeFolder();
            //SetFileName();


        }

        private void Start()
        {
            startTime = currentTime = Time.time;

            // GM_DataRecord.instance.Write_Warning(GM_DataRecord.Warning_Type.Ex_Start, "");
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }


        private void Update()
        {
            //if (Input.GetKeyDown(KeyCode.Escape))
            //{
            //    Instantiate(AppQuit_Pref);
            //}


            if (Input.GetKeyDown(KeyCode.Slash))
            {
                Save_SteamingData_Batch();

            }
        }

        private void FixedUpdate()
        {
            currentTime = Time.time;

        }

        private void MakeFolder()
        {
            rootpath = Directory.GetCurrentDirectory();

            folder_Path = System.IO.Path.Combine(rootpath, folderName);

            Directory.CreateDirectory(folder_Path);
        }

        private void SetFileName()
        {
            string fileNameFormat = string.Empty;

            switch (curren_EX_Type)
            {
                case Experiment_Type.VS_Line:
                    fileName = "Experiment_01_DataLog_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                    break;

                
            }
        }

        public void Enequeue_Data(string _data)
        {
            currentTime = Time.time;
            string stringRelativeTime = string.Format("{0:F4}", currentTime - startTime);

            string refined_Data = DateTime.Now.ToString("yyyyMMddHHmmss.fff") + "," + stringRelativeTime + "," + _data;

            Queue_EX_DATA.Enqueue(refined_Data);
        }

        public void Save_SteamingData_Batch()
        {
            WriteSteamingData_Batch(ref Queue_EX_DATA);
        }

        public void Clear_Queue_EX_DATA()
        {
            Queue_EX_DATA.Clear();
        }

        public bool WriteSteamingData_Batch(ref Queue<string> _Queue_ex)
        {
            bool tempb = false;

            try
            {
                //string tempFileName = fileName + ".txt";
                SetFileName();
                string tempFileName = fileName + ".txt";
                string file_Location = System.IO.Path.Combine(folder_Path, tempFileName);

                string m_str_DataCategory = string.Empty;

                int totalCountoftheQueue = _Queue_ex.Count;

                List<string> copyDataQueue = new List<string>(_Queue_ex);

                //int markerCount = 1;

                string catestr = string.Empty;

                Debug.Log("Saving Data Starts. Queue Count : " + totalCountoftheQueue);

                using (StreamWriter streamWriter = File.AppendText(file_Location))
                {
                    //while (_Queue_ex.Count != 0)
                    {
                        for (int i = 0; i < totalCountoftheQueue; i++)
                        {
                            //string stringData = _Queue_ex.Dequeue();
                            string stringData = copyDataQueue[i];

                            if (stringData.Length > 0)
                            {
                                if (!isCategoryPrinted)
                                {
                                    switch (curren_EX_Type)
                                    {
                                        case Experiment_Type.VS_Line:
                                            str_DataCategory = "Date,Timestamp,"
                                                + "Event_Log,"
                                                + "Event_Log_Num,"
                                                + "resetWallCountperEpisode,"
                                                + "resetTotalCountperEpisode,"
                                                + "reset00CountperEpisode,"
                                                + "reset01CountperEpisode,"
                                                + "resetbetCountperEpisode";


                                            break;

                                    }
                                    streamWriter.WriteLine(str_DataCategory);
                                    isCategoryPrinted = true;
                                }

                                streamWriter.WriteLine(stringData);
                            }
                        }
                    }
                }
                tempb = true;
                isCategoryPrinted = false;
                //StartCoroutine(CheckSavingDataCompleted());
            }
            catch (Exception e)
            {
                Debug.Log("WriteSteamingData_BatchProcessing ERROR : " + e);
            }

            return tempb;
        }



        public void Write_Warning(Warning_Type _warning, string _additionalMessage)
        {
            string warning_Message = string.Empty;

            switch (_warning)
            {
                case Warning_Type.Ex_Start:
                    warning_Message = "[The Experiment Main Process Starts Now. " + _additionalMessage + "]";
                    break;

                case Warning_Type.Ex_End:
                    warning_Message = "[The Experiment Main Process Ends Now.]";
                    break;

                case Warning_Type.TotalResetMean:
                    warning_Message = "TotalRsetMean" + _additionalMessage + "]";
                    break;
                    
                case Warning_Type.User00ResetMean:
                    warning_Message = "User00ResetMean" + _additionalMessage + "]";
                    break;

                case Warning_Type.User01ResetMean:
                    warning_Message = "User01ResetMean" + _additionalMessage + "]";
                    break;

                case Warning_Type.UserbetResetMean:
                    warning_Message = "UserbetResetMean" + _additionalMessage + "]";
                    break;

                case Warning_Type.UsershutterResetMean:
                    warning_Message = "UsershutterResetMean" + _additionalMessage + "]";
                    break;

                case Warning_Type.MeanDistBetResets:
                    warning_Message = "MeanDistBetResets" + _additionalMessage + "]";
                    break;
            }

            Debug.Log(warning_Message);
            Enequeue_Data(warning_Message + "," + (int)_warning);
        }

      





















    }
}