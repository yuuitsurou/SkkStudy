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
            if (SetPath(dicPath)) { BuildKeyAndBody(); }
        }

        public void BuildKeyAndBody()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            String [] ls = File.ReadAllLines(this._DicPath, Encoding.GetEncoding("EUC-JP"));
            if (ls != null)
            {
                this._Body = new List<string>();
                foreach(String l in ls) 
                {
                    if (!l.StartsWith(";;")) { this._Body.Add(l); }
                }
                this._Body.Sort();
                this._Key = new List<String>();
                foreach (String i in this._Body)
                {
                    this._Key.Add(i.Split(' ')[0]);
                }
            }
        }

        public String Search(String word)
        {
            int index = this._Key.BinarySearch(word);
            if (index > -1)
            {
                return this._Body[index].Split(' ')[1];
            }
            else
            {
                return String.Empty;
            }
        }
    }
}