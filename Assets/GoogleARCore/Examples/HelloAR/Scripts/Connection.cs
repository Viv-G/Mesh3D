namespace GoogleARCore.Examples.HelloAR
{
    /*    using System.Collections;
        using System.Collections.Generic;
        using System.Net; */
    using System.Net.Sockets;
    using System.Text;
    using UnityEngine;

    /// <summary>
    /// Creates Connection to Server and sends data as string
    /// </summary>
    ///
    public class Connection : MonoBehaviour
    {
        private static int port = 11111;
        private static string HostIP = EntryScript.HostSet;
        public static int pCount;
        public static Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


        public static void Connect(string Host)
        {
            Debug.Log("Establishing Connection to " + Host);
            try
            {
                s.Connect(Host, port);
                if (s.Connected)
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
                    Application.Quit();
                    return;
                }
                else
                {
                    string errMessage = "Disconnected: error code: " + e.NativeErrorCode;
                    Debug.LogError(errMessage);
                    return;
                }
            }

        }

        public static void WriteString(int NPoints, string pointBuffer)
        {

            // Convert to Strings
            string numPoints = NPoints.ToString() + " ENDN\n";
            string buffSend = pointBuffer + " ENDP\n";
            // Convert to Bytes
            byte[] nSend = Encoding.ASCII.GetBytes(numPoints);
            byte[] sBytes = Encoding.ASCII.GetBytes(buffSend);
            int size = sBytes.Length;
            string sizeSend = size.ToString();
            //            Debug.LogError(sizeSend);
            //SEND
            try
            {
                s.Send(nSend);
                s.Send(sBytes);
            }
            catch (SocketException e)
            {
                string errMessage = "Disconnected: error code: " + e.NativeErrorCode;
                Debug.LogError(errMessage);
                return;
            }
        }
    }
}