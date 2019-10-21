//-----------------------------------------------------------------------
// <copyright file="HelloARController.cs" company="Google">
//
// Copyright 2017 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace GoogleARCore.Mesh3D
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using GoogleARCore;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using OpenCvSharp;

#if UNITY_EDITOR
    // Set up touch input propagation while using Instant Preview in the editor.
    using Input = InstantPreviewInput;
#endif

    /// <summary>
    /// Controls the HelloAR example.
    /// </summary>
    public class Mesh3DController : MonoBehaviour
    {
        /// <summary>
        /// The ARCoreSession monobehavior that manages the ARCore session.
        /// </summary>
        public ARCoreSession ARSessionManager;

        /// <summary>
        /// The first-person camera being used to render the passthrough camera image (i.e. AR
        /// background).
        /// </summary>
        public Camera FirstPersonCamera;

        /// <summary>
        /// A prefab to place when a raycast from a user touch hits a horizontal plane.
        /// </summary>
        public GameObject GameObjectHorizontalPlanePrefab;

        /// <summary>
        /// A prefab to place when a raycast from a user touch hits a feature point.
        /// </summary>
//        public GameObject GameObjectPointPrefab;

        /// <summary>
        /// The rotation in degrees need to apply to prefab when it is placed.
        /// </summary>
        private const float k_PrefabRotation = 180.0f;

        /// <summary>
        /// True if the app is in the process of quitting due to an ARCore connection error,
        /// otherwise false.
        /// </summary>
        private bool m_IsQuitting = false;

        /// <summary>
        /// A Text box that is used to output the camera intrinsics values.
        /// </summary>
        public Text CameraIntrinsicsOutput;

        //    private bool m_UseHighResCPUTexture = false;
        private bool m_UseHighResCPUTexture = true;
        private ARCoreSession.OnChooseCameraConfigurationDelegate m_OnChoseCameraConfiguration =
            null;
        private int m_HighestResolutionConfigIndex = 0;
        private int m_LowestResolutionConfigIndex = 0;
        private bool m_Resolutioninitialized = false;
        private Text m_ImageTextureToggleText;
        private float m_RenderingFrameRate = 0f;
        private float m_RenderingFrameTime = 0f;
        private int m_FrameCounter = 0;
        private float m_FramePassedTime = 0.0f;
        /// <summary>
        /// The frame rate update interval.
        /// </summary>
        private static float s_FrameRateUpdateInterval = 2.0f;

        /// <summary>
        /// Current Camera Pose
        /// </summary>
        public Pose curPose = Pose.identity;

        /// <summary>
        /// Tracking Variable for points to send
        /// </summary>
        private int m_Track;

        /// <summary>
        /// Tracking Variable for total frames
        /// </summary>
        private int m_Frames;

        /// <summary>
        /// Min values for bounding box
        /// </summary>
        public static Vector3 minVals = new Vector3(-0.25f, -1f, 0.1f);


        /// <summary>
        /// Max values for bounding box
        /// </summary>
        public static Vector3 maxVals = new Vector3(0.25f, 0.1f, .75f);

        /// <summary>
        /// Default Confidence Value
        /// </summary>
        public static float setConf = 0.5f;

        public static float Dplane;
        public static bool planeFlag = false;


        /// <summary>
        /// The Unity Awake() method.
        /// </summary>
        public void Awake()
        {
            // Enable ARCore to target 60fps camera capture frame rate on supported devices.
            // Note, Application.targetFrameRate is ignored when QualitySettings.vSyncCount != 0.
            Application.targetFrameRate = 60;

            // Lock screen to portrait.
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.orientation = ScreenOrientation.Portrait;

            // Register the callback to set camera config before arcore session is enabled.
            m_OnChoseCameraConfiguration = _ChooseCameraConfiguration;
            ARSessionManager.RegisterChooseCameraConfigurationCallback(
                m_OnChoseCameraConfiguration);

            var config = ARSessionManager.SessionConfig;
            if (config != null)
            {
                config.CameraFocusMode = CameraFocusMode.Auto;
            }
        }

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
            _UpdateApplicationLifecycle();
            _UpdateFrameRate();
            ExportMeshPoints();

            /// Update Camera Intrinsics///
            var cameraIntrinsics = Frame.CameraImage.ImageIntrinsics;
            string intrinsicsType = "CPU Image";
            //HelloARController.
            CameraIntrinsicsOutput.text = _CameraIntrinsicsToString(cameraIntrinsics, intrinsicsType);

            // If the player has not touched the screen, we are done with this update.
            Touch touch;
            if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
            {
                return;
            }

            // Should not handle input if the player is pointing on UI.
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                return;
            }

            // Raycast against the location the player touched to search for planes.
            TrackableHit hit;
            TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
                TrackableHitFlags.FeaturePointWithSurfaceNormal;

            if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
            {
                // Use hit pose and camera pose to check if hittest is from the
                // back of the plane, if it is, no need to create the anchor.
                if ((hit.Trackable is DetectedPlane) &&
                    Vector3.Dot(FirstPersonCamera.transform.position - hit.Pose.position,
                        hit.Pose.rotation * Vector3.up) < 0)
                {
                    Debug.Log("Hit at back of the current DetectedPlane");
                }
                else
                {
                    if (hit.Trackable is DetectedPlane)
                    {
                        GameObject prefab;
                        DetectedPlane detectedPlane = hit.Trackable as DetectedPlane;

                        Vector3 pCent = detectedPlane.CenterPose.position;
                        minVals.y = (pCent.y + 0.005f);
                        maxVals.y = (pCent.y + 2 * (maxVals.y));

                        Vector3 pNor = detectedPlane.CenterPose.rotation * Vector3.up;

                        float Aeq = pNor.x;
                        float Beq = pNor.y;
                        float Ceq = pNor.z;
                        float Deq = (pNor.x * -pCent.x) + (pNor.y * -pCent.y) + (pNor.z * -pCent.z);
                        Dplane = Deq;

                        string message = "PLANE EQUATION IS: " + Aeq + " , " + Beq + " , " + Ceq + " , " + Deq + "\n";
                        Debug.Log(message);

                        prefab = GameObjectHorizontalPlanePrefab;


                        // Instantiate prefab at the hit pose.
                        var gameObject = Instantiate(prefab, hit.Pose.position, hit.Pose.rotation);

                        // Compensate for the hitPose rotation facing away from the raycast (i.e.
                        // camera).
                        gameObject.transform.Rotate(0, k_PrefabRotation, 0, Space.Self);

                        // Create an anchor to allow ARCore to track the hitpoint as understanding of
                        // the physical world evolves.
                        var anchor = hit.Trackable.CreateAnchor(hit.Pose);

                        // Make game object a child of the anchor.
                        gameObject.transform.parent = anchor.transform;

                        planeFlag = true;
                    }
                }
            }
        }

        /// <summary>
        /// Update Frame Rate
        /// </summary>
        private void _UpdateFrameRate()
        {
            m_FrameCounter++;
            m_FramePassedTime += Time.deltaTime;
            if (m_FramePassedTime > s_FrameRateUpdateInterval)
            {
                m_RenderingFrameTime = 1000 * m_FramePassedTime / m_FrameCounter;
                m_RenderingFrameRate = 1000 / m_RenderingFrameTime;
                m_FramePassedTime = 0f;
                m_FrameCounter = 0;
                CamImage.GetCameraImage();
            }
        }

        /// <summary>
        /// Generate string to print the value in CameraIntrinsics.
        /// </summary>
        /// <param name="intrinsics">The CameraIntrinsics to generate the string from.</param>
        /// <param name="intrinsicsType">The string that describe the type of the
        /// intrinsics.</param>
        /// <returns>The generated string.</returns>
        private string _CameraIntrinsicsToString(CameraIntrinsics intrinsics, string intrinsicsType)
        {
            float fovX = 2.0f * Mathf.Rad2Deg * Mathf.Atan2(
                intrinsics.ImageDimensions.x, 2 * intrinsics.FocalLength.x);
            float fovY = 2.0f * Mathf.Rad2Deg * Mathf.Atan2(
                intrinsics.ImageDimensions.y, 2 * intrinsics.FocalLength.y);

            string frameRateTime = m_RenderingFrameRate < 1 ? "Calculating..." :
                string.Format("{0}ms ({1}fps)", m_RenderingFrameTime.ToString("0.0"),
                    m_RenderingFrameRate.ToString("0.0"));

            string message = string.Format(
                "Unrotated Camera {4} Intrinsics:{0}  Focal Length: {1}{0}  " +
                "Principal Point: {2}{0}  Image Dimensions: {3}{0}  " +
                "Unrotated Field of View: ({5}°, {6}°){0}" +
                "Render Frame Time: {7}",
                Environment.NewLine, intrinsics.FocalLength.ToString(),
                intrinsics.PrincipalPoint.ToString(), intrinsics.ImageDimensions.ToString(),
                intrinsicsType, fovX, fovY, frameRateTime);
            return message;
        }

        /// <summary>
        /// Select the desired camera configuration.
        /// If high resolution toggle is checked, select the camera configuration
        /// with highest cpu image and highest FPS.
        /// If low resolution toggle is checked, select the camera configuration
        /// with lowest CPU image and highest FPS.
        /// </summary>
        /// <param name="supportedConfigurations">A list of all supported camera
        /// configuration.</param>
        /// <returns>The desired configuration index.</returns>
        private int _ChooseCameraConfiguration(List<CameraConfig> supportedConfigurations)
        {

            if (!m_Resolutioninitialized)
            {
                m_HighestResolutionConfigIndex = 0;
                m_LowestResolutionConfigIndex = 0;
                CameraConfig maximalConfig = supportedConfigurations[0];
                CameraConfig minimalConfig = supportedConfigurations[0];
                for (int index = 1; index < supportedConfigurations.Count; index++)
                {
                    CameraConfig config = supportedConfigurations[index];
                    if ((config.ImageSize.x > maximalConfig.ImageSize.x &&
                         config.ImageSize.y > maximalConfig.ImageSize.y) ||
                        (config.ImageSize.x == maximalConfig.ImageSize.x &&
                         config.ImageSize.y == maximalConfig.ImageSize.y &&
                         config.MaxFPS > maximalConfig.MaxFPS))
                    {
                        m_HighestResolutionConfigIndex = index;
                        maximalConfig = config;
                    }

                    if ((config.ImageSize.x < minimalConfig.ImageSize.x &&
                         config.ImageSize.y < minimalConfig.ImageSize.y) ||
                        (config.ImageSize.x == minimalConfig.ImageSize.x &&
                         config.ImageSize.y == minimalConfig.ImageSize.y &&
                         config.MaxFPS > minimalConfig.MaxFPS))
                    {
                        m_LowestResolutionConfigIndex = index;
                        minimalConfig = config;
                    }
                }


                m_Resolutioninitialized = true;
            }

            if (m_UseHighResCPUTexture)
            {
                return m_HighestResolutionConfigIndex;
            }

            return m_LowestResolutionConfigIndex;
        }

        /// <summary>
        /// Check and update the application lifecycle.
        /// </summary>
        private void _UpdateApplicationLifecycle()
        {
            // Exit the app when the 'back' button is pressed.
            if (Input.GetKey(KeyCode.Escape))
            {
                ExportImages();
                Application.Quit();
            }

            // Only allow the screen to sleep when not tracking.
            if (Session.Status != SessionStatus.Tracking)
            {
                Screen.sleepTimeout = SleepTimeout.SystemSetting;
            }
            else
            {
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }

            if (m_IsQuitting)
            {
                return;
            }

            // Quit if ARCore was unable to connect and give Unity some time for the toast to
            // appear.
            if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
            {
                _ShowAndroidToastMessage("Camera permission is needed to run this application.");
                m_IsQuitting = true;
                Invoke("_DoQuit", 0.5f);
            }
            else if (Session.Status.IsError())
            {
                _ShowAndroidToastMessage(
                    "ARCore encountered a problem connecting.  Please start the app again.");
                m_IsQuitting = true;
                Invoke("_DoQuit", 0.5f);
            }
        }

        /// <summary>
        /// Export Images (SAVE TO DISK)
        /// </summary>
/*        private void ExportImages()
        {
            var path = Application.persistentDataPath;
            Texture2D result;
            result = Unity.MatToTexture(output);
            result.Apply();

            byte[] im = result.EncodeToJPG(100);
            for (var i = 0; i < CamImage.AllData.Count; i++)
            {
                byte[] imOut = CamImage.AllData[i];
                string fileName = "/IMG" + i + ".jpg";
                File.WriteAllBytes(path + fileName, imOut);
                string messge = "Succesfully Saved Image To " + path + "\n";
                Debug.Log(messge);
            }
        } */

        public void ExportImages()
        {
            /// Write Camera intrinsics to text file
            var path = Application.persistentDataPath;
            StreamWriter sr = new StreamWriter(path + @"/intrinsics.txt");
            sr.WriteLine(CameraIntrinsicsOutput.text);
            Debug.Log(CameraIntrinsicsOutput.text);
            sr.Close();

            if (Connection.s.Connected)
            {
                Connection.s.Close();
            }
            ConExit.ConnectOut();
            ConExit.SendArraySize(CamImage.AllData.Count);
            CameraIntrinsicsOutput.text = "Sending " + CamImage.AllData.Count + "Images";
            // Loop through Mat List, Add to Texture and Save.
            for (var i = 0; i < CamImage.AllData.Count; i++)
            {
                Mat imOut = CamImage.AllData[i];
                Texture2D result = Unity.MatToTexture(imOut);
                result.Apply();

                byte[] im = result.EncodeToJPG(100);
                ConExit.SendIM(im);

                CameraIntrinsicsOutput.text = "Succesfully Sent Image " + i + "\n Remaining: " + (CamImage.AllData.Count - i);
                //Debug.Log(messge);
                Destroy(result);
            }
            CameraIntrinsicsOutput.text = "Success! Exciting...";
            //Connection.s.Close();
            ConExit.sOut.Close();
            Application.Quit();
        }


        public void ExportMeshPoints()
        {
            string buff = "";
            m_Track = 0;

            if (Frame.PointCloud.PointCount > 0 && Frame.PointCloud.IsUpdatedThisFrame)
            {
                string dPlaneString = Dplane + " ";
                buff = dPlaneString;
                curPose = Frame.Pose;
                string curPosePos = curPose.position.x + " " + curPose.position.y + " " + curPose.position.z + " ";
                buff += curPosePos;


                for (int i = 0; i < Frame.PointCloud.PointCount; i++)
                {
                    Vector3 point = Frame.PointCloud.GetPointAsStruct(i); // -UNTRANSFORMED                
                    int idPoint = Frame.PointCloud.GetPointAsStruct(i).Id;
                    float conf = Frame.PointCloud.GetPointAsStruct(i).Confidence;

                    if (point.x < minVals.x || point.y < minVals.y || point.z < minVals.z ||
                        point.x > maxVals.x || point.y > maxVals.y || point.z > maxVals.z || conf < setConf)
                    {
                        continue;
                    }
                   
                    PointcloudVisualizer.AddPointToCache(point);
                    string content = idPoint + " " + point.x + " " + point.y + " " + point.z + " ";
                    buff += content;

                    m_Track += 1;

                }
                if (m_Track != 0)
                {
                    Connection.WriteString(m_Track, buff);
                }

            }
        }

        /// <summary>
        /// Actually quit the application.
        /// </summary>
        private void _DoQuit()
        {
            if (Connection.s.Connected)
            {
                Connection.s.Close();
            }
            if (ConExit.sOut.Connected)
            {
                ConExit.sOut.Close();
            }
            Application.Quit();
        }

        /// <summary>
        /// Show an Android toast message.
        /// </summary>
        /// <param name="message">Message string to show in the toast.</param>
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
