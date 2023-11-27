using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranslationRunner
{
    public class TranslationLanguage
    {
        public string visual_name { get; set; }
        public string code { get; set; }
        public string url { get; set; }
        public int version { get; set; }
        public string md5 { get; set; }

        public string path;
    }
}
