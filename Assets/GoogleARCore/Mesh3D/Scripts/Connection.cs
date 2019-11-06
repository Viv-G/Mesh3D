namespace GoogleARCore.Mesh3D
{
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
            try
            {
                s.Connect(Host, port);
                if (s.Connected)
                {
                }
            }
            catch (SocketException e)
            {
                // 10035 == WSAEWOULDBLOCK
                if (e.NativeErrorCode.Equals(10056))
                {
                    return;
                }
                else
                {
                    string errMessage = "Disconnected: error code: " + e.NativeErrorCode;
                    return;
                }
            }

        }

        /// <summary>
        /// Sends Number of Points and Point Cloud Data to the server
        /// Attempts reconnection if errors are caught
        /// Exits application if reconnection is unsuccesful
        /// </summary>
        /// <param name="ptsStringObj">A String Array of size 2
        /// </param>
        /// <returns>Void</returns>
        public static void WriteString(System.Object ptsStringObj)
        {
            string[] ptsStringArray = (string[]) ptsStringObj;
            // Convert to Strings
            string numPoints = ptsStringArray[0] + " ENDN\n";
            string buffSend = ptsStringArray[1] + " ENDP\n";
            // Convert to Bytes
            byte[] nSend = Encoding.ASCII.GetBytes(numPoints);
            byte[] sBytes = Encoding.ASCII.GetBytes(buffSend);
            int size = sBytes.Length;
            string sizeSend = size.ToString();
            //SEND
            try
            {
                s.Send(nSend);
                s.Send(sBytes);
            }
            //Catch error and attempt reconnect
            catch (SocketException e)
            {
                string errMessage = "Disconnected: error code: " + e.NativeErrorCode;
                s.Close();
                s.Connect(HostIP, port);
                return;
            }
        }
    }
}