namespace GoogleARCore.Mesh3D
{
    using System;
    using GoogleARCore;
    using UnityEngine;
    using OpenCvSharp;
    using System.Runtime.InteropServices;

    public class CamImage : MonoBehaviour
    {

        /// <summary>
        /// Captures a YUV420888 image from the devices CPU, using ARCores CameraImageBytes class
        /// Image is then converted to RGB using OpenCV+ A free plugin based on OpenCV Sharp
        /// Image is then rotated and flipped to align with the viewport.
        /// Adds images to a List of Mat Arrays, which can be converted to JPG using Unity Textures at a later date
        /// NOTE: No Unity classes are used in this function to ensure that it is thread safe (Unity itself is not threadsafe).
        /// </summary>
        public static void GetCameraImage()
        {

            // Use using to make sure that C# disposes of the CameraImageBytes afterwards
            using (CameraImageBytes camBytes = Frame.CameraImage.AcquireCameraImageBytes())
            {

                // If acquiring failed, return null
                if (!camBytes.IsAvailable)
                {
                    return;
                }

                // To save a YUV_420_888 image, you need 1.5*pixelCount bytes.
                byte[] YUVimage = new byte[(int)(camBytes.Width * camBytes.Height * 1.5f)];

                // As CameraImageBytes keep the Y, U and V data in three separate
                // arrays, we need to put them in a single array. This is done using
                // native pointers, which are considered unsafe in C#.
                unsafe
                {
                    for (int i = 0; i < camBytes.Width * camBytes.Height; i++)
                    {
                        YUVimage[i] = *((byte*)camBytes.Y.ToPointer() + (i * sizeof(byte)));
                    }

                    for (int i = 0; i < camBytes.Width * camBytes.Height / 4; i++)
                    {
                        YUVimage[(camBytes.Width * camBytes.Height) + 2 * i] = *((byte*)camBytes.U.ToPointer() + (i * camBytes.UVPixelStride * sizeof(byte)));
                        YUVimage[(camBytes.Width * camBytes.Height) + 2 * i + 1] = *((byte*)camBytes.V.ToPointer() + (i * camBytes.UVPixelStride * sizeof(byte)));
                    }
                }

                // GCHandles help us "pin" the arrays in the memory, so that we can
                // pass them to the C++ code.
                GCHandle pinnedArray = GCHandle.Alloc(YUVimage, GCHandleType.Pinned);

                IntPtr pointerYUV = pinnedArray.AddrOfPinnedObject();

                Mat input = new Mat(camBytes.Height + camBytes.Height / 2, camBytes.Width, MatType.CV_8UC1, pointerYUV);
                Mat output = new Mat(camBytes.Height, camBytes.Width, MatType.CV_8UC3);

                Cv2.CvtColor(input, output, ColorConversionCodes.YUV2BGR_NV12);

                // FLIP AND TRANPOSE TO VERTICAL
                Cv2.Transpose(output, output);
                Cv2.Flip(output, output, FlipMode.Y);

               Mesh3DController.AllData.Add(output);
               pinnedArray.Free();
               Mesh3DController.ErrorString = "Thread Sucess!";
            }
            
        }
    }
}
