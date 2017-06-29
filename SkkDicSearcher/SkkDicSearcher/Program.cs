﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkkDicSearcher
{
    class Program
    {
        static void Main(string[] args)
        {
            String[] src = File.ReadAllLines(@"C:\Users\ymine\skkdic\SKK-JISYO.L", Encoding.GetEncoding("EUC-JP"));
            if (src == null) { System.Environment.Exit(0); }
            List<String> dic = new List<string>(src);
            dic.Sort();
            List<String> midasi = new List<string>();
            foreach (String item in dic)
            {
                midasi.Add(item.Split(' ')[0]);
            }

            //ListenするIPアドレス
            string ipString = "127.0.0.1";
            System.Net.IPAddress ipAdd = System.Net.IPAddress.Parse(ipString);

            //ホスト名からIPアドレスを取得する時は、次のようにする
            //string host = "localhost";
            //System.Net.IPAddress ipAdd =
            //    System.Net.Dns.GetHostEntry(host).AddressList[0];
            //.NET Framework 1.1以前では、以下のようにする
            //System.Net.IPAddress ipAdd =
            //    System.Net.Dns.Resolve(host).AddressList[0];

            //Listenするポート番号
            int port = 2001;

            //TcpListenerオブジェクトを作成する
            System.Net.Sockets.TcpListener listener =
                new System.Net.Sockets.TcpListener(ipAdd, port);

            //Listenを開始する
            listener.Start();
            Console.WriteLine("Listenを開始しました({0}:{1})。",
                ((System.Net.IPEndPoint)listener.LocalEndpoint).Address,
                ((System.Net.IPEndPoint)listener.LocalEndpoint).Port);

            //接続要求があったら受け入れる
            System.Net.Sockets.TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine("クライアント({0}:{1})と接続しました。",
                ((System.Net.IPEndPoint)client.Client.RemoteEndPoint).Address,
                ((System.Net.IPEndPoint)client.Client.RemoteEndPoint).Port);

            //NetworkStreamを取得
            System.Net.Sockets.NetworkStream ns = client.GetStream();

            //読み取り、書き込みのタイムアウトを10秒にする
            //デフォルトはInfiniteで、タイムアウトしない
            //(.NET Framework 2.0以上が必要)
            // ns.ReadTimeout = 10000;
            // ns.WriteTimeout = 10000;

            //クライアントから送られたデータを受信する
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            bool disconnected = false;
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            byte[] resBytes = new byte[256];
            int resSize = 0;
            /**
            while ((resSize = ns.Read(resBytes, 0, resBytes.Length)) != 0)
            {
                ms.Write(resBytes, 0, resSize);
            }
             * */
            do
            {
                //データの一部を受信する
                resSize = ns.Read(resBytes, 0, resBytes.Length);
                //Readが0を返した時はクライアントが切断したと判断
                if (resSize == 0)
                {
                    disconnected = true;
                    Console.WriteLine("クライアントが切断しました。");
                    break;
                }
                //受信したデータを蓄積する
                ms.Write(resBytes, 0, resSize);
                //まだ読み取れるデータがあるか、データの最後が\nでない時は、
                // 受信を続ける
            } while (ns.DataAvailable || resBytes[resSize - 1] != '\n');
            //受信したデータを文字列に変換
            string resMsg = enc.GetString(ms.GetBuffer(), 0, (int)ms.Length);
            ms.Close();
            //末尾の\nを削除
            resMsg = resMsg.TrimEnd('\n');
            // resMsg = resMsg.TrimEnd('\r');
            Console.WriteLine(resMsg);
            int index = midasi.BinarySearch(resMsg);
            String sendMsg = String.Empty;
            if (index > -1)
            {
                sendMsg = dic[index].Split(' ')[1];
            }
            else
            {
                sendMsg = "Not found...";
            }
            //クライアントにデータを送信する
            //クライアントに送信する文字列を作成
            //文字列をByte型配列に変換
            byte[] sendBytes = enc.GetBytes(sendMsg + '\n');
            //データを送信する
            ns.Write(sendBytes, 0, sendBytes.Length);
            Console.WriteLine(sendMsg);

            //閉じる
            ns.Close();
            client.Close();
            Console.WriteLine("クライアントとの接続を閉じました。");

            //リスナを閉じる
            listener.Stop();
            Console.WriteLine("Listenerを閉じました。");

            Console.ReadLine();
        }
    }
}
