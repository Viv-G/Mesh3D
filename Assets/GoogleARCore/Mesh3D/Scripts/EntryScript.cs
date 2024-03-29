﻿namespace GoogleARCore.Mesh3D
{
    using System.IO;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.SceneManagement;

    public class EntryScript : MonoBehaviour
    {
        /// <summary>
        /// Default IP Address
        /// </summary>
        public static string HostSet = "192.168.8.118";
        public Text Text_Status;
        private string content;
        private string xVal;
        private string yVal;
        private string zVal;
        private string cVal;
        private string imVal;

        /// <summary>
        /// Loads last used settings
        /// Triggered via GUI button
        /// </summary>
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
            SetImageRate(sRead.ReadLine());
            sRead.Close();
            content = "Set Server IP To:  " + HostSet
                + "\nSet Width To:  " + xVal
                + "\nSet Height To:  " + yVal
                + "\nSet Depth To:  " + zVal
                + "\nSet Conf To:  " + cVal
                + "\nSet Image Capture Rate To:  " + imVal;
        }

        /// <summary>
        /// Reads last used IP Address from file
        /// Triggered via GUI button
        /// </summary>
        private void ReadFile()
        {
            string path = Application.persistentDataPath + @"/previousData.txt";
            StreamReader sRead = new StreamReader(path, System.Text.Encoding.ASCII);
            HostSet = sRead.ReadLine();
            sRead.Close();
            content = "Set Server IP To: " + HostSet;
            
        }

        /// <summary>
        /// Saves the current settings to file, called everytime app is run
        /// </summary>
        private void WriteFile()
        {
            string path = Application.persistentDataPath + @"/previousData.txt";
            StreamWriter sWrite = new StreamWriter(path, append: false, System.Text.Encoding.ASCII);
            sWrite.WriteLine(HostSet);
            sWrite.WriteLine(xVal);
            sWrite.WriteLine(yVal);
            sWrite.WriteLine(zVal);
            sWrite.WriteLine(Mesh3DController.setConf.ToString());
            sWrite.WriteLine(imVal);
            sWrite.Close();

        }

        /// <summary>
        /// Sets IP address if dropdown is used
        /// </summary>
        /// <param name="val">int val is set from dropdown, 0 starts as default, 1 = option 1... etc</param>
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
        /// <summary>
        /// Manual IP address entry field
        /// </summary>
        /// <param name="Host">String Host takes manaul ip address and overwrites default</param>
        public void SetIpMan(string Host)
        {
            HostSet = Host;
            content = "Set Server IP To: " + HostSet;

        }

        /// <summary>
        /// Load the main scene, connect to Server and enter the scene
        /// </summary>
        public void LoadScene()
        {
            WriteFile();
            Connection.Connect(HostSet);

            if (Connection.s == null)
            {
                content = "Unable To Connect... Try again";
                return;
            }
            else
            {
                content = "Connected To: " + HostSet;
                SceneManager.LoadScene("Mesh3D");
            }
        }

        /// <summary>
        /// Set minimum confidence value for points
        /// </summary>
        /// <param name="c_val">Lower bound of confidence value</param>
        public void SetConf(string c_val)
        {
            cVal = c_val;
            float cfloat_val = float.Parse(c_val);
            Mesh3DController.setConf = cfloat_val;
            content = "Set Confidence To: " + c_val;

        }

        /// <summary>
        /// Sets desired image capture interval
        /// </summary>
        /// <param name="im_val">string im_val, desired amount of seconds to wait, entered via keyboard</param>
        public void SetImageRate(string im_val)
        {
            imVal = im_val;
            float imfloat_val = float.Parse(im_val);
            float im_interval = 1f / imfloat_val;
            Mesh3DController.s_FrameRateUpdateInterval = imfloat_val;
            content = "Set Image Capture Interval To: " + im_val + "Seconds";

        }

        /// <summary>
        /// Sets x bounding box values
        /// </summary>
        /// <param name="xString">width of box</param>
        public void SetXbox(string xString)
        {
            xVal = xString;
            float xfloat = float.Parse(xString);
            float xMin = 0 - (xfloat / 2);
            float xMax = 0 + (xfloat / 2);
            Mesh3DController.minVals.x = xMin;
            Mesh3DController.maxVals.x = xMax;
            content = "Set X Range To:  " + xMin + " --> " + xMax + " Metres";

        }

        /// <summary>
        /// Sets y bounding box values
        /// </summary>
        /// <param name="yString">height of box, adjustments made to true height to better suit user input</param>
        public void SetYbox(string yString)
        {
            yVal = yString;
            float yfloat = float.Parse(yString);
            float yMin = 0.1f - yfloat;
            float yMax = 0 + (yfloat / 2);
            Mesh3DController.minVals.y = yMin;
            Mesh3DController.maxVals.y = yMax;
            Mesh3DController.yConst = yMax;
            content = "Set Y Range To:  " + yMin + " --> " + yMax + " Metres";

        }
        /// <summary>
        /// Sets depth of bounding box
        /// </summary>
        /// <param name="zString">depth of the box as a string</param>
        public void SetZbox(string zString)
        {
            zVal = zString;
            float zfloat = float.Parse(zString);
            float zMin = 0.1f;
            float zMax = 0.1f + zfloat;
            Mesh3DController.minVals.z = zMin;
            Mesh3DController.maxVals.z = zMax;
            content = "Set Z Range To:  " + zMin + " --> " + zMax + " Metres";

        }


        /// <summary>
        /// True if the app is in the process of quitting due to an ARCore connection error,
        /// otherwise false.
        /// </summary>
        private bool m_IsQuitting = false;


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