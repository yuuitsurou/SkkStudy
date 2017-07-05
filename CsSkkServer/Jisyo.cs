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

        public Jisyo(String jisyoPath) 
        {
            BuildKeyAndBody(jisyoPath);
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
                String [] ls = File.ReadAllLines(this._JisyoPath, Encoding.GetEncoding("EUC-JP"));
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

        public String Search(String word)
        {
            int index = this.Key.BinarySearch(word);
            if (index > -1)
            {
                String [] s = this.Body[index].Split('/');
                if (s == null) return String.Empty;
                List<String> items = new List<string>(s); 
                return "/" + String.Join("/", items.GetRange(1, items.Count - 1).ToArray());
            }
            else
            {
                return String.Empty;
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
            String result = String.Empty;
            foreach (var jisyo in this.Jisyos)
            {
                String w = jisyo.Search(word);
                if (String.IsNullOrWhiteSpace(w)) { continue; }
                if (!String.IsNullOrWhiteSpace(result) && w.StartsWith("/")) { w = w.TrimStart('/'); }
                result += w;
            }
            return result;
        }
    }
    
}