using System;

namespace CoreSkkDicSearch
{
    class Program
    {
        static void Main(string[] args)
        {
            Server.StartServer(1177);
            Server.Listen();
        }
    }
}
