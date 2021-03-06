﻿using System;
using System.Net.Sockets;
using System.IO;
using System.Text;

namespace CoreDicClient
{
	class Program
    {
        static void Main(string[] args)
        {
            RunClient();
        }

        private static void RunClient()
        {

            //サーバーのIPアドレス（または、ホスト名）とポート番号
            string ipOrHost = "127.0.0.1";
            //string ipOrHost = "localhost";
            int port = 5678;

            //TcpClientを作成し、サーバーと接続する
            using(var c = new TcpClient())
            {
                // await c.ConnectAsync(ipOrHost, port);
                var t = c.ConnectAsync(ipOrHost, port);
                t.Wait();
                Console.WriteLine("サーバー({0}:{1})と接続しました({2}:{3})。",
                                    ((System.Net.IPEndPoint)c.Client.RemoteEndPoint).Address,
                                    ((System.Net.IPEndPoint)c.Client.RemoteEndPoint).Port,
                                    ((System.Net.IPEndPoint)c.Client.LocalEndPoint).Address,
                                    ((System.Net.IPEndPoint)c.Client.LocalEndPoint).Port);

                NetworkStream ns = c.GetStream();
                //読み取り、書き込みのタイムアウトを10秒にする
                //デフォルトはInfiniteで、タイムアウトしない
                //(.NET Framework 2.0以上が必要)
                //ns.ReadTimeout = 10000;
                //ns.WriteTimeout = 10000;
                Encoding enc = Encoding.UTF8;
                while (true)
                {
                    //サーバーに送信するデータを入力してもらう
                    Console.WriteLine("文字列を入力し、Enterキーを押してください。");
                    string sendMsg = Console.ReadLine();
                    // String sendMsg = "は";
                    //何も入力されなかった時は終了
                    if (sendMsg == null || sendMsg.Length == 0) sendMsg = "quit";
                    //サーバーにデータを送信する
                    //文字列をByte型配列に変換
                    byte[] sendBytes = enc.GetBytes(sendMsg + '\n');
                    //データを送信する
                    // await ns.WriteAsync(sendBytes, 0, sendBytes.Length);
                    ns.Write(sendBytes, 0, sendBytes.Length);
                    Console.WriteLine(sendMsg);
                    if (sendMsg == "quit") break;
                    //サーバーから送られたデータを受信する
                    var ms = new MemoryStream();
                    byte[] resBytes = new byte[256];
                    int resSize = 0;
                    do
                    {
                        //データの一部を受信する
                        // resSize = await ns.ReadAsync(resBytes, 0, resBytes.Length);
                        resSize = ns.Read(resBytes, 0, resBytes.Length);
                        //Readが0を返した時はサーバーが切断したと判断
                        if (resSize == 0)
                        {
                            Console.WriteLine("サーバーが切断しました。");
                            break;
                        }
                        //受信したデータを蓄積する
                        ms.Write(resBytes, 0, resSize);
                        //まだ読み取れるデータがあるか、データの最後が\nでない時は、
                        // 受信を続ける
                    } while (ns.DataAvailable || resBytes[resSize - 1] != '\n');
                    //受信したデータを文字列に変換
                    ArraySegment<byte> b = new ArraySegment<byte>();
                    ms.TryGetBuffer(out b);
                    string resMsg = enc.GetString(b.Array, 0, resSize);
                    //末尾の\nを削除
                    resMsg = resMsg.TrimEnd('\n');
                    Console.WriteLine(resMsg);
                    ms.Dispose();
                }
                ns.Dispose();
            }            
        }
    }
}
