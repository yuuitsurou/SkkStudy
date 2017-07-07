using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CsSkkServer
{
    public class Jisyo
    {
        private String _JisyoPath { set; get; }
        public Boolean SetPath(String path)
        {
            if (File.Exists(path))
            {
                this._JisyoPath = path;
                return true;
            }
            else
            {
                return false;
            }
        }

        private List<String> _Body { set; get; }
        private List<String> Body 
        {
            set { this._Body = value; }
            get { return this._Body ?? ( this._Body = new List<string>() ); }
        }

        private List<String> _Key { set; get; }
        private List<String> Key 
        {
            set { this._Key = value; }
            get { return this._Key ?? ( this._Key = new List<string>() ); }
        }

        private List<Komoku> _KomokuList { set; get; }
        private List<Komoku> KomokuList
        {
            set { this._KomokuList = value; }
            get { return this._KomokuList ?? ( this._KomokuList = new List<Komoku>() ); }
        }

        public Jisyo(String jisyoPath) 
        {
            // BuildKeyAndBody(jisyoPath);
            if (SetPath(jisyoPath))
            {
                BuildKomokuList();
            }
            else
            {
                throw new FileNotFoundException();
            }
        }

        public void BuildKeyAndBody(String jisyoPath)
        {
            if (SetPath(jisyoPath))
            {
                BuildKeyAndBody();
            }
            else
            {
                throw new FileNotFoundException();
            }
        }
        public void BuildKeyAndBody()
        {
            try {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                String [] ls = File.ReadAllLines(this._JisyoPath, Encoding.GetEncoding(20932));
                if (ls != null)
                {
                    this.Body = new List<string>();
                    foreach(String l in ls) 
                    {
                        if (!l.StartsWith(";;")) { this.Body.Add(l); }
                    }
                    this.Body.Sort(delegate(String x, String y) 
                    {
                        String keyX = x.Split('/')[0].TrimEnd(' ');
                        String keyY = y.Split('/')[0].TrimEnd(' ');
                        return (keyX == keyY ? (0) : (keyX.CompareTo(keyY) < 0 ? (-1) : (1)));
                    });
                    this.Key = new List<String>();
                    foreach (String i in this.Body)
                    {
                        this.Key.Add(i.Split('/')[0].TrimEnd(' '));
                    }
                }
            } 
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void BuildKomokuList()
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                byte[] bs = File.ReadAllBytes(this._JisyoPath);
                if (bs == null) { return; }
                List<byte[]> lines = new List<byte[]>();
                int start = 0;
                for (int i = 0; i < bs.Length; i++)
                {
                    if (bs[i] == '\n')
                    {
                        byte[] b = new byte[i - start];
                        for (int ii = 0; ii < (i - start); ii++)
                        {
                            b[ii] = bs[start + ii];
                        }
                        lines.Add(b);
                        start = i + 1;
                    }
                }
                foreach(byte[] line in lines)
                {
                    String s = Encoding.GetEncoding("EUC-JP").GetString(line, 0, line.Length);
                    if (s.StartsWith(";;")) { continue; }
                    Komoku k = new Komoku() { Midasi = s.Split('/')[0].TrimEnd(' ') };
                    List<byte> kanji = new List<byte>();
                    Boolean begin = false;
                    foreach (var item in line)
                    {
                        if (begin) { kanji.Add(item); }
                        if (!begin && item == ' ') { begin = true; }
                    }
                    k.Kanji = kanji.ToArray();
                    this.KomokuList.Add(k);
                }
                this.KomokuList.Sort(delegate(Komoku x, Komoku y)
                {
                    return (x.Midasi == y.Midasi ? (0) : (x.Midasi.CompareTo(y.Midasi)) < 0 ? (-1) : (1));
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public String Search(String word)
        {
            int index = this.Key.BinarySearch(word);
            if (index > -1)
            {
                String [] s = this.Body[index].Split('/');
                if (s == null) return String.Empty;
                List<String> items = new List<string>(s); 
                return String.Join("/", items.GetRange(1, items.Count - 1).ToArray());
            }
            else
            {
                return String.Empty;
            }
        }

        public byte[] SearchWithResultByte(String word)
        {
            int index = this.KomokuList.FindIndex(x => x.Midasi == word);
            if (index > -1)
            {
                return this.KomokuList[index].Kanji;
            }
            else
            {
                return null;
            }
        }
    }

    public class JisyoLibs
    {
        private List<Jisyo> _Jisyos { set; get; }
        private List<Jisyo> Jisyos
        {
            set { this._Jisyos = value; }
            get { return this._Jisyos ?? (this._Jisyos = new List<Jisyo>()); }
        }

        public void SetupJisyos(String[] jisyoPaths) 
        {
            foreach (var jisyopath in jisyoPaths)
            {
                this.Jisyos.Add(new Jisyo(jisyopath));
            }
        }

        public String Search(String word)
        {
            String resultWords = String.Empty;
            foreach (var jisyo in this.Jisyos)
            {
                String w = jisyo.Search(word);
                if (String.IsNullOrWhiteSpace(w)) { continue; }
                if (!String.IsNullOrWhiteSpace(resultWords) && w.StartsWith("/")) { w = w.TrimStart('/'); }
                resultWords += w;
            }
            String result = String.Empty;
            if (!String.IsNullOrWhiteSpace(resultWords)) { result = "1/" + resultWords; }
            Console.WriteLine(result);
            return result;
        }

        public byte[] SearchWithResultByte(String word)
        {
            List<byte> kekkas = new List<byte>();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            foreach (var jisyo in this.Jisyos)
            {
                byte[] kekka = jisyo.SearchWithResultByte(word);
                if (kekka == null) { continue; }
                int start = 0;
                if (kekkas.Count > 0 && kekka[0] == '/') { start++; }
                for (int i = start; i < kekka.Length; i++)
                {
                    kekkas.Add(kekka[i]);
                }
            }
            if (kekkas.Count > 0)
            {
                List<byte> result = new List<byte>();
                foreach (var item in Encoding.GetEncoding("EUC-JP").GetBytes("1"))
                {
                    result.Add(item);
                }
                foreach (var item in kekkas)
                {
                    result.Add(item);
                }
                foreach (var item in Encoding.GetEncoding("EUC-JP").GetBytes("\n"))
                {
                    result.Add(item);
                }
                return result.ToArray();
            }
            else
            {
                return null;
            }
        }
    }

    public class Komoku
    {
        public String Midasi { set; get;}
        public byte[] Kanji { set; get; }
    }
    
}