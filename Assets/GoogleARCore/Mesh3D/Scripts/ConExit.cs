﻿namespace GoogleARCore.Mesh3D
{

    using System.Net.Sockets;
    using System.Text;
    using UnityEngine;

    /// <summary>
    /// Creates Connection to Server and sends data as string
    /// Tests connection, and then ends each image
    /// </summary>
    ///
    public class ConExit : MonoBehaviour
    {
        private static int port = 6666;
        private static string HostIP = EntryScript.HostSet;
        public static int pCount;
        public static Socket sOut = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


        public static void ConnectOut()
        {
            Debug.Log("Establishing Connection to " + HostIP);

            try
            {
                sOut.Connect(HostIP, port);

         //       sOut.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                if (sOut.Connected)
                {
                    Debug.Log("Connected!");
                }
            }
            catch (SocketException e)
            {
                // 10035 == WSAEWOULDBLOCK
                if (e.NativeErrorCode.Equals(10056))
                {
                    Debug.LogError("Connection Already Established \n");
                    return;
                }
                else
                {
                    string errMessage = "Disconnected: error code: " + e.NativeErrorCode;
                    Debug.LogError(errMessage);
                    return;
                }
            }
            if (sOut == null)
                Debug.LogError("Connection failed");
        }

        public static bool SendArrayCount(int size)
        {
            string sizeString = size.ToString();
            byte[] ArraySizeBytes = Encoding.ASCII.GetBytes(sizeString);
            try
            {
                sOut.Send(ArraySizeBytes);
                Debug.Log("Sent Array Size");
            }
            catch (SocketException e)
            {
                string errMessage = "ERROR_sending: " + e.Message + " " + e.ErrorCode;
                Debug.LogError(errMessage);
                return false;
            }

            try
            {
                byte[] confBuffer = new byte[4096];
                sOut.Receive(confBuffer);
                string confString = Encoding.ASCII.GetString(confBuffer);
                if ((string.Compare(confString, "CONF")) == 0){
                    Debug.Log("SUCCESFULLY SENT  AND CONFIRMED ARRAY SIZE");
                    return true;
                }
            }
            catch (SocketException e)
            {
                string errMessage = "ERROR_recieving: " + e.Message + " " + e.ErrorCode;
                Debug.LogError(errMessage);
                return false;
            }
            return false;
        }

        /// <summary>
        /// Sends the image to be sent
        /// Takes the image in bytes, encoded to JPG
        /// Breaks it into chunks of 4096 bytes
        /// Sends each chunk until complete
        /// </summary>
        /// <param name="im">Image in bytes to be sent</param>
        public static void SendIM(byte[] im)
        {
            pCount++;
            int size = im.Length;
            string SizeSend = "SIZE " + size;
            byte[] sizeBytes = Encoding.ASCII.GetBytes(SizeSend);
            sOut.Send(sizeBytes);
            Debug.Log("Sucessfully Sent Size: " + SizeSend);

            Debug.Log("Sending IMAGE");
            try
            {
                for (int j = 0; j < size; j += 4096)
                {
                    int rem = size - j;
                    if (rem < 4096)
                    {
                        int rI = sOut.Send(im, j, rem, 0);
                        string byteStringLogRem = "Sent REM " + rI + " Bytes...";
                        Debug.Log(byteStringLogRem);

                    }
                    else
                    {
                        int i = sOut.Send(im, j, 4096, 0);
                        string byteStringLog = "Sent Normal " + i + " Bytes...";
                        Debug.Log(byteStringLog);
                    }
                }
                Debug.Log("Sucessfully Sent IMAGE ");
                im = null;
            }
            catch (SocketException e)
            {
                string errMessage = "ERROR_sending: " + e.Message + " " + e.ErrorCode;
                Debug.LogError(errMessage);
            }
            byte[] recBuf = new byte[4096];
            sOut.Receive(recBuf, 4096, 0);
            string imRec = Encoding.ASCII.GetString(recBuf);
            Debug.Log("Answer = " + imRec);
        }
    }
}