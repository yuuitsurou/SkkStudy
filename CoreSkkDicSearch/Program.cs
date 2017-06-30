using System;

namespace CoreSkkDicSearch
{
    class Program
    {
        static void Main(string[] args)
        {
            Server.StartServer(5678);
            Server.Listen();
        }
    }
}
