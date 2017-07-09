using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CsSkkServer
{
    public class Server 
    {
        
        private static TcpListener listener { get; set; }  

        private static bool accept { get; set; } = false;  
   
        private static JisyoLibs Jisyos { set; get; }

        private static GoogleIme Google { set; get; }

        public static void StartServer(int port, String[] dicpaths) 
        {  
            try
            {
                IPAddress address = IPAddress.Parse("127.0.0.1");  
                listener = new TcpListener(address, port);  
    
                listener.Start();
                accept = true;  
    
                // String dicPath = @"C:\Users\ymine\skkdic\SKK-JISYO.L";
                Jisyos = new JisyoLibs();
                Jisyos.SetupJisyos(dicpaths);
                Google = new GoogleIme();
                // Console.WriteLine($"Server started. Listening to TCP clients at 127.0.0.1:{port}");  
            }
            catch (Exception ex)
            {
                throw ex;
            }
       }

       public static void StopServer()
       {
           if (listener != null) { listener.Stop(); }
       }
   
        public static async Task Listen()
       {
           try
           {  
                if (listener == null || !accept) { return; }
                // Continue listening.  
                while (true)
                {  
                    // Console.WriteLine("Waiting for client...");  
                    var client = await listener.AcceptTcpClientAsync();
                    if (client == null) { continue; }
                    // Console.WriteLine("Client connected. Waiting for data.");  
                    var t = Task.Run(() => SkkSession(client)).ConfigureAwait(false);
                }
           }
           catch (Exception ex)
           {
               throw ex;
           }
        }

        private static void SkkSession(TcpClient client)
        {
            try 
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                using(client)
                using (var stream = client.GetStream())
                {
                    string mes = "";
                    while (mes != null && !mes.StartsWith("0"))
                    {  
                        MessageInfo info = new MessageInfo();
                        if (ReadMessage(stream, info))
                        {
                            byte[] b = info.MessageBytes();
                            if (b != null)
                            {
                                byte[] utf8Bytes = Encoding.Convert(Encoding.GetEncoding("EUC-JP"), Encoding.UTF8, b);
                                mes = Encoding.UTF8.GetString(utf8Bytes, 0, utf8Bytes.Length);
                                SendMessage(stream, mes);
                            }
                            else
                            {
                                mes = null;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            // Console.WriteLine("Closing connection.");  
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static Boolean ReadMessage(NetworkStream stream, MessageInfo info)
        {
            try
            {
                byte[] resBytes = new byte[512];
                int resSize = 0;
                info.ResSize = 0;
                while (true)
                {
                    resSize = stream.Read(resBytes, 0, resBytes.Length);
                    if (resSize == 0) return false;
                    info.Buffer.Write(resBytes, info.ResSize, resSize);
                    info.ResSize += resSize;
                    if (!stream.DataAvailable) break;
                    if (resBytes[0] == '1' && resBytes[info.ResSize - 1] == ' ') break;                            
                    if (resBytes[0] != '1') break;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        private static byte[] GetResult(String mes)
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                mes = mes.TrimStart('1');
                mes = mes.TrimStart('/');
                mes = mes.TrimEnd(' ');
                byte[] result = Jisyos.Search(mes);
                if (result != null)
                {
                    return result;
                }
                else
                {
                    var t = Task.Run(() => Google.Search(mes));
                    t.Wait();
                    byte[] u8 = Encoding.UTF8.GetBytes(t.Result);
                    return Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding("EUC-JP"), u8);                
                }
            } 
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void SendMessage(NetworkStream stream, String mes)
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                byte[] data = null;
                switch (mes.Substring(0, 1))
                {
                    case "0":
                        break;
                    case "1":
                        data = GetResult(mes);
                        stream.Write(data, 0, data.Length);
                        break;
                    case "2":
                        byte[] euc2 = Encoding.GetEncoding("EUC-JP").GetBytes("CsSkkServer.0.1");
                        stream.Write(euc2, 0, euc2.Length);
                        break;
                    case "3":
                        byte[] euc3 = Encoding.GetEncoding("EUC-JP").GetBytes("localhost:127.0.0.1");
                        stream.Write(euc3, 0, euc3.Length);
                        break;
                    case "4":
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    public class MessageInfo
    {
        private MemoryStream _Buffer { set;get;}
        public MemoryStream Buffer
        {
            set { this._Buffer = value; }
            get { return this._Buffer ?? (this._Buffer = new MemoryStream()); }
        }
        public int ResSize { set; get;}

        public byte[] MessageBytes()
        {
            if (ResSize <= 0) return null; 
            if (this.Buffer.Length == 0) return null; 
            byte[] b = this.Buffer.ToArray();
            byte[] buf = new byte[this.ResSize];
            for (int i = 0; i < this.ResSize; i++) { buf[i] = b[i]; }
            return buf;
        }
    }   
    
}