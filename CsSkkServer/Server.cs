using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace CsSkkServer
{
    public class Server 
    {
        private static TcpListener listener { get; set; }  
        private static bool accept { get; set; } = false;  
   
        private static JisyoLibs Jisyos { set; get; }

        private static GoogleIme Google { set; get; }

        public static void StartServer(int port) 
        {  
            IPAddress address = IPAddress.Parse("127.0.0.1");  
            listener = new TcpListener(address, port);  
   
            listener.Start();  
            accept = true;  
   
            String[] dicPath = 
            { 
                @"/usr/share/skk/SKK-JISYO.L", 
                @"/usr/share/skk/SKK-JISYO.jinmei", 
            };
            // String dicPath = @"C:\Users\ymine\skkdic\SKK-JISYO.L";
            Jisyos = new JisyoLibs();
            Jisyos.SetupJisyos(dicPath);
            Google = new GoogleIme();
            Console.WriteLine($"Server started. Listening to TCP clients at 127.0.0.1:{port}");  
       }  
   
        public static void Listen()
       {  
           if (listener == null || !accept) 
           {
               return;
           }
            // Continue listening.  
            while (true)
            {  
                Console.WriteLine("Waiting for client...");  
                var connectResult = listener.AcceptTcpClientAsync();
                if (connectResult.Result == null) { continue; }
                var client = connectResult.Result;
                // var client = await listener.AcceptTcpClientAsync(); // Get the client  
                Console.WriteLine("Client connected. Waiting for data.");  
                string mes = "";
                while (mes != null && !mes.StartsWith("0"))
                {  
                    Messenger m = new Messenger() { Client = client };
                    if (ReadMessage(m))
                    {
                        byte[] b = m.Ms.ToArray();
                        byte[] buffer = new byte[m.ResSize];
                        for (int i = 0; i < m.ResSize; i++) { buffer[i] = b[i]; }
                        byte[] utf8Bytes = Encoding.Convert(Encoding.GetEncoding("EUC-JP"), Encoding.UTF8, buffer);
                        mes = Encoding.UTF8.GetString(utf8Bytes, 0, utf8Bytes.Length);
                        SendMessage(client, mes);
                    }
                }
                Console.WriteLine("Closing connection.");  
                client.GetStream().Dispose();
                client.Dispose();
            }  
        }

        private static Boolean ReadMessage(Messenger mes)
        {
            mes.Ms = new MemoryStream();
            byte[] resBytes = new byte[512];
            int resSize = 0;
            mes.ResSize = 0;
            while (true)
            {
                resSize = mes.Client.GetStream().Read(resBytes, 0, resBytes.Length);
                if (resSize == 0)
                {
                    return false;
                }
                mes.Ms.Write(resBytes, mes.ResSize, resSize);
                mes.ResSize += resSize;
                if (!mes.Client.GetStream().DataAvailable) break;
                if (resBytes[0] == '1' && resBytes[mes.ResSize - 1] == ' ') break;                            
                if (resBytes[0] != '1') break;
            }
            return true;
        }
        private static byte[] GetResult(String mes)
        {
            mes = mes.TrimStart('1');
            mes = mes.TrimStart('/');
            mes = mes.TrimEnd(' ');
            String result = Jisyos.Search(mes);
            if (!String.IsNullOrWhiteSpace(result))
            {
                return StringToEucBytes(result + '\n');
            }
            else
            {
                return StringToEucBytes(Google.Search(mes) + '\n');
            }

        }

        private static void SendMessage(TcpClient client, String mes)
        {
            byte[] data = null;
            switch (mes.Substring(0, 1))
            {
                case "0":
                    break;
                case "1":
                    data = GetResult(mes);
                    client.GetStream().Write(data, 0, data.Length);
                    break;
                case "2":
                    byte[] euc2 = StringToEucBytes("CsSkkServer.0.1\n");
                    client.GetStream().Write(euc2, 0, euc2.Length);
                    break;
                case "3":
                    byte[] euc3 = StringToEucBytes("localhost:127.0.0.1\n");
                    client.GetStream().Write(euc3, 0, euc3.Length);
                    break;
                case "4":
                    break;
                default:
                    break;
            }

        }

        private static byte[] StringToEucBytes(String s) 
        {
            byte[] u8 = Encoding.UTF8.GetBytes(s);
            return Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding("EUC-JP"), u8);
        }
    }

    public class Messenger
    {
        public TcpClient Client { set; get; }
        public MemoryStream Ms { set; get; }
        public int ResSize { set; get;}
    }   
    
}