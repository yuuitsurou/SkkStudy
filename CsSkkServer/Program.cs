using System;
using System.Collections.Generic;
using System.IO;

namespace CsSkkServer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    Console.Error.WriteLine("Please set path to dictionaries...");
                }
                List<String> paths = new List<string>();
                /**
                foreach (var s in args)
                {
                    if (File.Exists(s)) { paths.Add(s); }
                }
                **/
                paths.Add(@"/usr/share/skk/SKK-JISYO.L");
                paths.Add(@"/usr/share/skk/SKK-JISYO.JIS2");
                paths.Add(@"/usr/share/skk/SKK-JISYO.JIS2004");
                paths.Add(@"/usr/share/skk/SKK-JISYO.JIS3_4");
                paths.Add(@"/usr/share/skk/SKK-JISYO.itaiji");
                paths.Add(@"/usr/share/skk/SKK-JISYO.itaiji.JIS3_4");
                Server.StartServer(1178, paths.ToArray());
                Server.Listen();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("{0} [{1}]", ex.Message, ex.StackTrace);
            }
            finally
            {
                Server.StopServer();
            }
        }
    }
}
