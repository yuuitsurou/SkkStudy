using System;

namespace CsSkkServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server.StartServer(1178);
            Server.Listen();
        }
    }
}
