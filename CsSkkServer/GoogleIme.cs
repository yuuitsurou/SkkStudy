using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace CsSkkServer
{
    public class GoogleIme
    {
        private static HttpClient hc = new HttpClient();

        public async Task<String> Search(String word)
        {
            try {
                var text = System.Net.WebUtility.UrlEncode(word);
                var result = await hc.GetStringAsync("http://www.google.com/transliterate?langpair=ja-Hira|ja&text=" + text);
                var jarray = JArray.Parse(result);
                var first = new List<String>(jarray[0][1].Children().Values<String>());
                var kekka = new List<String>();
                for (int i = 0; i < first.Count; i++)
                {
                    var list = new List<String>();
                    for (int ii = 1; ii < jarray.Count; ii++)
                    {
                        var words = new List<String>(jarray[ii][1].Children().Values<String>());
                        for (int iii = 0; iii < words.Count; iii++)
                        {
                            if (iii < list.Count)
                            {
                                list[iii] += words[iii];
                            }
                            else
                            {
                                list.Add(first[i] + words[iii]);
                            }
                        }
                    }
                    var s = first[i];
                    if (list.Count > 0) { s = String.Join("/", list.ToArray()); }
                    kekka.Add(s);
                }
                return "1/" + String.Join("/", kekka.ToArray()) + "/\n";            
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }        
    }
    
}