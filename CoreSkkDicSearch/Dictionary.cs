using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CoreSkkDicSearch 
{
    public class Dictionary
    {
        private String _DicPath { set; get; }
        public Boolean SetPath(String path)
        {
            if (File.Exists(path))
            {
                this._DicPath = path;
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

        public Dictionary(String dicPath) 
        {
            BuildKeyAndBody(dicPath);
        }

        public void BuildKeyAndBody(String dicPath)
        {
            if (SetPath(dicPath))
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
                String [] ls = File.ReadAllLines(this._DicPath, Encoding.GetEncoding("EUC-JP"));
                if (ls != null)
                {
                    this.Body = new List<string>();
                    foreach(String l in ls) 
                    {
                        if (!l.StartsWith(";;")) { this.Body.Add(l); }
                    }
                    this.Body.Sort();
                    this.Key = new List<String>();
                    foreach (String i in this.Body)
                    {
                        this.Key.Add(i.Split(' ')[0]);
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
                return this.Body[index].Split(' ')[1];
            }
            else
            {
                return String.Empty;
            }
        }
    }

    public class DicLibs
    {
        private List<Dictionary> _Dics { set; get; }
        private List<Dictionary> Dics
        {
            set { this._Dics = value; }
            get { return this._Dics ?? (this._Dics = new List<Dictionary>()); }
        }

        public void SetupDics(String[] dicPaths) 
        {
            foreach (var dicpath in dicPaths)
            {
                this.Dics.Add(new Dictionary(dicpath));
            }
        }

        public String Search(String word)
        {
            String result = String.Empty;
            foreach (var dic in this.Dics)
            {
                String w = dic.Search(word);
                if (String.IsNullOrWhiteSpace(w)) { continue; }
                if (!String.IsNullOrWhiteSpace(result) && w.StartsWith("/")) { w = w.Remove(0, 1); }
                result += w;
            }
            return result;
        }
    }
}