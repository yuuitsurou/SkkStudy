using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CsSkkServer
{
    public class Jisyo
    {
        private String _JisyoPath { set; get; }

        private Boolean ValidDictionary { set; get; }
        public Boolean SetPath(String path)
        {
            this.ValidDictionary = true;
            if (File.Exists(path))
            {
                this._JisyoPath = path;
            }
            else
            {
                this.ValidDictionary = false;
            }
            return this.ValidDictionary;
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

        public byte[] Search(String word)
        {
            if (!this.ValidDictionary) { return null; }
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

        public byte[] Search(String word)
        {
            List<byte> kekkas = new List<byte>();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            foreach (var jisyo in this.Jisyos)
            {
                byte[] kekka = jisyo.Search(word);
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