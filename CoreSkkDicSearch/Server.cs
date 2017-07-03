using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace CoreSkkDicSearch
{
    public class Server 
    {
        private static TcpListener listener { get; set; }  
        private static bool accept { get; set; } = false;  
   
        private static DicLibs Dics { set; get; }

        public static void StartServer(int port) 
        {  
            IPAddress address = IPAddress.Parse("127.0.0.1");  
            listener = new TcpListener(address, port);  
   
            listener.Start();  
            accept = true;  
   
            String[] dicPath = 
            { 
                @"/usr/share/skk/SKK-JISYO.L", 
                @"/usr/share/skk/SKK-JISYO.JIS2", 
                @"/usr/share/skk/SKK-JISYO.JIS2004", 
                @"/usr/share/skk/SKK-JISYO.JIS3_4", 
                @"/usr/share/skk/SKK-JISYO.itaiji", 
                @"/usr/share/skk/SKK-JISYO.itaiji.JIS3_4"
            };
            // String dicPath = @"C:\Users\ymine\skkdic\SKK-JISYO.L";
            Dics = new DicLibs();
            Dics.SetupDics(dicPath);
            Console.WriteLine($"Server started. Listening to TCP clients at 127.0.0.1:{port}");  
       }  
   
        public static void Listen()
       {  
            if(listener != null && accept) 
            {  
                // Continue listening.  
                while (true)
                {  
                    Console.WriteLine("Waiting for client...");  
                    var connectResult = listener.AcceptTcpClientAsync();
                    if (connectResult.Result == null) { continue; }
                    var client = connectResult.Result;
                    // var client = await listener.AcceptTcpClientAsync(); // Get the client  
   
                    Console.WriteLine("Client connected. Waiting for data.");  

                    string message = "";
                    while (message != null && !message.StartsWith("quit"))
                    {  
                        // byte[] data = Encoding.ASCII.GetBytes("Send next data: [enter 'quit' to terminate] ");  
                        // client.GetStream().Write(data, 0, data.Length);  
                        byte[] data = null;
                        MemoryStream ms = new MemoryStream();
                        byte[] resBytes = new byte[256];
                        int resSize = 0;
						Boolean connected = true;
                        do
                        {
                            // resSize = await client.GetStream().ReadAsync(resBytes, 0, resBytes.Length);
                            resSize = client.GetStream().Read(resBytes, 0, resBytes.Length);
							if (resSize == 0)
							{
								connected = false;
								break;
							}
                            ms.Write(resBytes, 0 ,resSize);

                        } while (client.GetStream().DataAvailable || resBytes[resSize - 1] != '\n');
						if (connected)
						{
							// message = Encoding.ASCII.GetString(buffer);
							ArraySegment<byte> bf = new ArraySegment<byte>();
							ms.TryGetBuffer(out bf);
							message = Encoding.UTF8.GetString(bf.Array, 0, resSize);
							if (!message.StartsWith("quit"))
							{
								message = message.TrimEnd('\n');
                                String result = Dics.Search(message);
								if (!String.IsNullOrWhiteSpace(result))
								{
									data = Encoding.UTF8.GetBytes(result + '\n');
								}
								else
								{
									data = Encoding.UTF8.GetBytes("Not found..." + '\n');
								}
								// await client.GetStream().WriteAsync(data, 0, data.Length);
								client.GetStream().Write(data, 0, data.Length);
							}
						}
					}
                    Console.WriteLine("Closing connection.");  
                    client.GetStream().Dispose();
					client.Dispose();
                }  

            }  
        }  
    }   
}