namespace GoogleARCore.Mesh3D
{
    using UnityEngine;
    using UnityEngine.UI;
    using System;
    using OpenCvSharp;

    public class ExitScript : MonoBehaviour
    {
        public Text Text_Status;
        private string content;

        /// <summary>
        /// True if the app is in the process of quitting due to an ARCore connection error,
        /// otherwise false.
        /// </summary>
        public static Int32 track = 0;

        // Start is called before the first frame update
        void Start()
        {
            //Update screen to reflect points to be sent
            content = "Sending " + Mesh3DController.AllData.Count + " Images";
            Debug.Log(content);
            Text_Status.text = content;

            //Create connection to Server and test it
            ConExit.ConnectOut();
            ConExit.SendArrayCount(Mesh3DController.AllData.Count);

            content = "Sending " + Mesh3DController.AllData.Count + "Images";

        }

        // Update is called once per frame
        void Update()
        {                                  
            if (track < Mesh3DController.AllData.Count)
            {
                content = "Sending Image: " + (track + 1) + "/" + (Mesh3DController.AllData.Count);
                ExitSendImages();
                Debug.Log("TRACK = " + track);
                track++;
                Debug.Log("TRACK_NEW " + track);
            }
            else if (track == Mesh3DController.AllData.Count - 1)
            {
                ConExit.sOut.Close();
                content = "Succesfully Sent Images... Press Escape to Exit";
                Debug.Log(content);
                track++;
            }
            else
            {
                content = "Succesfully Sent Images... Press Escape to Exit";
            }

            Text_Status.text = content;
            // If the player has not touched the screen, we are done with this update.
            _UpdateApplicationLifecycle();
        }

        /// <summary>
        /// Sends images to the server
        /// Encodes image to jpg and then sends it
        /// Updates display to show progress
        /// </summary>
        private void ExitSendImages()
        {
              
                Mat imOut = Mesh3DController.AllData[track];
                Texture2D result = Unity.MatToTexture(imOut);
                result.Apply();
                byte[] im = result.EncodeToJPG(100);
                //AllBytes.Add(im);
                ConExit.SendIM(im);
                im = null;
                Debug.Log(content);
                Destroy(result);
        }
        
        public void ExitApp()
        {
            _DoQuit();
        }


        private void _UpdateApplicationLifecycle()
        {
            // Exit the app when the 'back' button is pressed.
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }

        }


        /// <summary>
        /// Actually quit the application.
        /// </summary>
        private void _DoQuit()
        {
            Application.Quit();
        }
    }
}
