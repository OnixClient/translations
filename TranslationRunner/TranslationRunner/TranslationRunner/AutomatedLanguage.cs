using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static TranslationRunner.LangFile;

namespace TranslationRunner
{
    public class AutomatedLanguage
    {
        public class TextReplacementValue
        {
            public string toFind { get; set; }
            public string replaceWith { get; set; }
        }
        
        public string origin { get; set; }
        public string target { get; set; }
        public List<TextReplacementValue> textReplacements { get; set; }
        public Dictionary<string, string> rawKeys { get; set; }
        public AutomatedLanguage(string origin, string target, List<TextReplacementValue> textReplacements, Dictionary<string,string> rawKeys)
        {
            this.origin = origin;
            this.target = target;
            this.textReplacements = textReplacements;
            this.rawKeys = rawKeys;
        }
        private string capitalizeFirstLetter(string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;
            return s.Substring(0, 1).ToUpper() + s.Substring(1);
        }
        public string Correct(string key, string value)
        {
            if (rawKeys.ContainsKey(key))
                return rawKeys[key];
            string result = value;
            foreach (var replacement in textReplacements)
            {
                result = result.Replace(replacement.toFind, replacement.replaceWith);
            }
            return result;
        }
        public AutomatedLanguage(string path)
        {
            this.target = Path.GetFileNameWithoutExtension(path);
            JsonObject json = JsonSerializer.Deserialize<JsonObject>(File.ReadAllText(path));
            origin = json["origin"].ToString();
            textReplacements = new List<TextReplacementValue>();
            if (json.ContainsKey("text_replacement")) { 
                foreach (var value in json["text_replacement"].AsObject())
                {
                    TextReplacementValue v = new TextReplacementValue()
                    {
                        toFind = value.Key,
                        replaceWith = value.Value.ToString()
                    };
                    textReplacements.Add(v);
                    TextReplacementValue cv = new TextReplacementValue
                    {
                        replaceWith = capitalizeFirstLetter(v.replaceWith),
                        toFind = capitalizeFirstLetter(v.toFind)
                    };
                    textReplacements.Add(cv);
                    TextReplacementValue fcv = new TextReplacementValue
                    {
                        replaceWith = v.replaceWith.ToUpper(),
                        toFind = v.toFind.ToUpper()
                    };
                    textReplacements.Add(fcv);
                    TextReplacementValue flv = new TextReplacementValue
                    {
                        replaceWith = capitalizeFirstLetter(v.replaceWith.ToLower()),
                        toFind = capitalizeFirstLetter(v.toFind.ToLower())
                    };
                    textReplacements.Add(flv);
                }
            }
            rawKeys = new Dictionary<string, string>();
            if (json.ContainsKey("full_keys"))
            {
                foreach (var value in json["full_keys"].AsObject())
                {
                    rawKeys.Add(value.Key, value.Value.ToString());
                }
            }
        }
    }
}
