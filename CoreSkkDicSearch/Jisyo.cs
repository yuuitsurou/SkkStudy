using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CoreSkkDicSearch 
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
                        String keyX = x.Split(" /".ToCharArray())[0];
                        String keyY = y.Split(" /".ToCharArray())[0];
                        return (keyX == keyY ? (0) : (keyX.CompareTo(keyY) < 0 ? (-1) : (1)));
                    });
                    this.Key = new List<String>();
                    foreach (String i in this.Body)
                    {
                        this.Key.Add(i.Split(" /".ToCharArray())[0]);
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
                return "/" + this.Body[index].Split(" /".ToCharArray())[1];
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
                if (!String.IsNullOrWhiteSpace(result) && w.StartsWith("/")) { w = w.Remove(0, 1); }
                result += w;
            }
            return result;
        }
    }
}