using System;

namespace CoreSkkDicSearch
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
