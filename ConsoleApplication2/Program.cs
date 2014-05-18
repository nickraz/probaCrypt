using InTheHand.Net.Sockets;
using System;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace ConsoleApplication2
{
    class Program
    {

        private static AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
        readonly Guid OurServiceClassId = new Guid("{29913A2D-EB93-40cf-BBB8-DEEE26452197}");
        readonly string OurServiceName = "32feet.NET Chat2";

        public BluetoothClient Bluetoothclient = new BluetoothClient();

        public BluetoothListener Bluetoothlistener = null;

        public delegate void InfoDelegate(object obj, string pstr);
        public event InfoDelegate DataAvailable;

        private NetworkStream Ns = null;

        public static void md5(string source)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(source);
            MD5 md = MD5.Create();
            byte [] cryptoData = md.ComputeHash(data);
            Console.WriteLine(System.Text.Encoding.UTF8.GetString(cryptoData));
            md.Clear();
        }

        public void Listen()
        {
            try { new BluetoothClient(); }
            catch (Exception ex)
            {
                var msg = "Bluetooth init failed: " + ex;
                MessageBox.Show(msg);
                throw new InvalidOperationException(msg, ex);
            }
            Bluetoothlistener = new BluetoothListener(OurServiceClassId);
            Bluetoothlistener.ServiceName = OurServiceName;
            Bluetoothlistener.Start();
            Bluetoothclient = Bluetoothlistener.AcceptBluetoothClient();
            byte[] data = new byte[1024];
            Ns = Bluetoothclient.GetStream();
            Ns.BeginRead(data, 0, data.Length, new AsyncCallback(ReadCallBack), data);
            DataAvailable(this, "Begin to read");
        }

        private void ReadCallBack(IAsyncResult ar)
        {
            try
            {
                if (ar != null && ar.IsCompleted)
                {
                    byte[] data = (byte[])ar.AsyncState;
                    DataAvailable(this, System.Text.UTF8Encoding.ASCII.GetString(data));
                    if (Bluetoothclient != null)
                    {
                        if (Ns != null)
                        {
                            Ns.Flush();
                            Ns.BeginRead(data, 0, data.Length, new AsyncCallback(ReadCallBack), data);
                        }
                    }
                }
            }
            catch (Exception ex) { throw ex; }
        }


        private static byte[] generateKeyAndIV(int size)
        {
            byte[] key = new byte[size];
            for (int i = 0; i < key.Length; i++)
            {
                if (i % 2 == 0)
                    key[i] = (byte)((i + (i + 6)) % 147);
                else if (i % 3 == 0)
                    key[i] = (byte)((i + (i * 6)) % 19);
                else if (i % 5 == 0)
                    key[i] = (byte)((i + (i + 6)) % 96);
                else if (i % 7 == 0)
                    key[i] = (byte)((i + (i * 6)) % 47);
                else key[i] = (byte)((i + (i * 32)) % 256);
            }
            return key;
        }

        private static void saveProtectedProcesses(string protectedProcesses)
        {
            aes.Key = generateKeyAndIV(32);
            aes.IV = generateKeyAndIV(16);
            FileStream fs = new FileStream("protect.dat", FileMode.OpenOrCreate, FileAccess.Write);
            CryptoStream cs = new CryptoStream(fs, aes.CreateEncryptor(), CryptoStreamMode.Write);
            StreamWriter sw = new StreamWriter(cs);
            sw.Write(protectedProcesses);
            sw.Close();
        }

        private static string readProtectedProcesses()
        {
          aes.Key = generateKeyAndIV(32);
          aes.IV = generateKeyAndIV(16);
            FileStream fs = new FileStream("protect.dat", FileMode.Open, FileAccess.Read);
            CryptoStream cs = new CryptoStream(fs, aes.CreateDecryptor(), CryptoStreamMode.Read);
            StreamReader sr = new StreamReader(cs);
            string result = sr.ReadToEnd();
            sr.Close();
            return result;
        }

        public static void Main(string[] args)
        {
            //Program p = new Program();
            string procs = "chrome|filezilla|firefox";
          // saveProtectedProcesses(procs);
            Console.WriteLine(readProtectedProcesses());
            md5("Пароль");
            Console.ReadKey();
        }
    }
}