﻿namespace GoogleARCore.Examples
{
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.SceneManagement;

    public class EntryScript : MonoBehaviour
    {
        public static string HostSet = "192.168.8.118";
        public Text Text_Status;
        private string content;
        private string xVal;
        private string yVal;
        private string zVal;
        private string cVal;

        // private StreamReader sRead; //= new StreamWriter(path, append: true);
        //private StreamWriter sWrite;
        public void QuickStart()
        {
            content = "Using Last Settings...:";
            string path = Application.persistentDataPath + @"/previousData.txt";
            StreamReader sRead = new StreamReader(path, System.Text.Encoding.ASCII);
            HostSet = sRead.ReadLine();
            SetXbox(sRead.ReadLine());
            SetYbox(sRead.ReadLine());
            SetZbox(sRead.ReadLine());
            SetConf(sRead.ReadLine());
            sRead.Close();
            content = "Set Server IP To: " + HostSet;

            HelloAR.Connection.Connect(HostSet);

            if (HelloAR.Connection.s == null)
            {
                content = "Unable To Connect... Try again";
                return;
            }
            else
            {
                content = "Connected To: " + HostSet;
                SceneManager.LoadScene("HelloAR");
            }
        }

        private void ReadFile()
        {
            string path = Application.persistentDataPath + @"/previousData.txt";
            StreamReader sRead = new StreamReader(path, System.Text.Encoding.ASCII);
            HostSet = sRead.ReadLine();
            SetXbox(sRead.ReadLine());
            sRead.Close();
            content = "Set Server IP To: " + HostSet;
            //DO READ FILE AND WRITE PREVIOUS IP TO HOSTSET
        }

        private void WriteFile()
        {
            string path = Application.persistentDataPath + @"/previousData.txt";
            StreamWriter sWrite = new StreamWriter(path, append: false, System.Text.Encoding.ASCII);
            sWrite.WriteLine(HostSet);
            sWrite.WriteLine(xVal);
            sWrite.WriteLine(yVal);
            sWrite.WriteLine(zVal);
            sWrite.WriteLine(cVal);
            sWrite.Close();

        }

        public void SetIP(int val)
        {
            switch (val)
            {
                case 0:
                    HostSet = "192.168.8.118";
                    content = "Set Server IP To: " + HostSet;
                    break;
                case 1:
                    HostSet = "192.168.8.118";
                    content = "Set Server IP To: " + HostSet;
                    break;
                case 2:
                    ReadFile();
                    content = "Set Server To Last Setting... IP = : " + HostSet;
                    break;
                case 3:
                    HostSet = "172.20.10.2";
                    content = "Set Server IP To: " + HostSet;
                    break;
                default:
                    HostSet = "192.168.8.118";
                    content = "Set Server IP To: " + HostSet;
                    break;

            }

            content = "Set Server IP To: " + HostSet;
        }

        public void SetIpMan(string Host)
        {
            HostSet = Host;
            content = "Set Server IP To: " + HostSet;
            content = "Set Server IP To: " + HostSet;
        }


        public void LoadScene()
        {
            WriteFile();
            HelloAR.Connection.Connect(HostSet);

            if (HelloAR.Connection.s == null)
            {
                content = "Unable To Connect... Try again";
                return;
            }
            else
            {
                content = "Connected To: " + HostSet;
                SceneManager.LoadScene("HelloAR");
            }
        }


        public void SetConf(string c_val)
        {
            cVal = c_val;
            float cfloat_val = float.Parse(c_val);
            //Common.PointcloudVisualizer.setConf = cfloat_val;
            HelloAR.HelloARController.setConf = cfloat_val;
            content = "Set Confidence To: " + c_val;

        }

        public void SetXbox(string xString)
        {
            xVal = xString;
            float xfloat = float.Parse(xString);
            float xMin = 0 - (xfloat / 2);
            float xMax = 0 + (xfloat / 2);
            //Common.PointcloudVisualizer.minVals.x = xMin;
            HelloAR.HelloARController.minVals.x = xMin;
            //Common.PointcloudVisualizer.maxVals.x = xMax;
            HelloAR.HelloARController.maxVals.x = xMax;
            content = "Set X Range To:  " + xMin + " --> " + xMax + " Metres";

        }

        public void SetYbox(string yString)
        {
            yVal = yString;
            float yfloat = float.Parse(yString);
            float yMin = 0 - (yfloat / 2);
            float yMax = 0 + (yfloat / 2);
            // Common.PointcloudVisualizer.minVals.y = yMin;
            //Common.PointcloudVisualizer.maxVals.y = yMax;
            HelloAR.HelloARController.minVals.y = yMin;
            HelloAR.HelloARController.maxVals.y = yMax;
            content = "Set Y Range To:  " + yMin + " --> " + yMax + " Metres";

        }

        public void SetZbox(string zString)
        {
            zVal = zString;
            float zfloat = float.Parse(zString);
            float zMin = 0.1f;
            float zMax = 0.1f + zfloat;
            HelloAR.HelloARController.minVals.z = zMin;
            HelloAR.HelloARController.maxVals.z = zMax;
            content = "Set Z Range To:  " + zMin + " --> " + zMax + " Metres";

        }


        /// <summary>
        /// True if the app is in the process of quitting due to an ARCore connection error,
        /// otherwise false.
        /// </summary>
        private bool m_IsQuitting = false;


        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            Text_Status.text = content;

            _UpdateApplicationLifecycle();

            // If the player has not touched the screen, we are done with this update.
            Touch touch;
            if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
            {
                return;
            }

        }
        private void _UpdateApplicationLifecycle()
        {
            // Exit the app when the 'back' button is pressed.
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }


            if (m_IsQuitting)
            {
                return;
            }
        }


        /// <summary>
        /// Actually quit the application.
        /// </summary>
        private void _DoQuit()
        {
            Application.Quit();
        }

        private void _ShowAndroidToastMessage(string message)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity =
                unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if (unityActivity != null)
            {
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    AndroidJavaObject toastObject =
                        toastClass.CallStatic<AndroidJavaObject>(
                            "makeText", unityActivity, message, 0);
                    toastObject.Call("show");
                }));
            }
        }

    }
}