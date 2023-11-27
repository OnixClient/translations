using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranslationRunner
{
    public class LangFile
    {
        public class Line
        {
            string comment;
            bool isEmpty;
            string key;
            string value;
            public string Comment { get { return comment; } set { comment = value; } }
            public bool IsEmpty { get { return isEmpty; } }
            public string Key { get { return key; } set { key = value; } }
            public string Value { get { return value; } set { this.value = value; } }
            public bool HasComment { get { return !string.IsNullOrEmpty(comment); } }
            public bool HasKey { get { return !string.IsNullOrEmpty(key); } }
            public bool HasValue { get { return !string.IsNullOrEmpty(value); } }
            public bool HasKeyAndValue { get { return HasKey && HasValue; } }

            public Line(string comment, string key, string value)
            {
                this.comment = comment;
                this.key = key;
                this.value = value;
                isEmpty = string.IsNullOrEmpty(comment) && string.IsNullOrEmpty(key) && string.IsNullOrEmpty(value);
            }
            public Line(string linee)
            {
                string line = linee.Trim('\t');
                if (string.IsNullOrWhiteSpace(line))
                {
                    isEmpty = true;
                    return;
                }
                if (line.Contains("##"))
                {
                    string[] parts = line.Split("##", 2);
                    comment = parts[1];
                    line = parts[0].Trim('\t');
                }
                if (line.Contains("="))
                {
                    string[] parts = line.Split("=", 2);
                    key = parts[0];
                    if (parts.Length > 1)
                        value = parts[1];
                    else
                        value = "";
                }
                else
                {
                    isEmpty = true;
                }
            }

            public override string ToString()
            {
                string result = "";
                if (HasKeyAndValue)
                    result += Key + "=" + Value;
                else if (HasKey)
                    result += Key + "=";
                if (HasComment)
                {
                    if (HasKey)
                        result += "\t\t";
                    result += "##";
                    if (!Comment.StartsWith("#") && !Comment.StartsWith(" "))
                        result += " ";
                    result += Comment;
                }
                return result;
            }
        }

        public List<Line> lines = new List<Line>();
        public SortedSet<string> keys = new SortedSet<string>();
        public string path = "";
        public LangFile() { }
        public LangFile(string[] lines)
        {
            foreach (string line in lines)
            {
                this.lines.Add(new Line(line));
            }
        }
        public LangFile(string path)
        {
            this.lines = new List<Line>();
            this.path = path;
            foreach (string line in File.ReadAllLines(path))
            {
                Line currentLine = new Line(line);
                this.lines.Add(currentLine);
                if (currentLine.HasKey)
                    this.keys.Add(currentLine.Key);
            }
        }
        public bool HasKey(string key)
        {
            return keys.Contains(key);
        }
        public Line? GetLine(string key)
        {
            return lines.Where(x => x.HasKey && x.Key == key).FirstOrDefault();
        }
        public List<string> GetMissingKeys(LangFile perfectFile)
        {
            List<string> result = new List<string>();
            foreach (string key in perfectFile.keys)
            {
                if (!HasKey(key))
                    result.Add(key);
            }
            return result;
        }
        public void Save(string? file = null)
        {
            if (string.IsNullOrEmpty(file))
                file = path;
            if (string.IsNullOrEmpty(file))
                throw new Exception("No file specified to save lang file");
            File.WriteAllLines(file, lines.Select(x => x.ToString()));
        }
        public void AddAutoGenratedHeader(string originalLanguage)
        {
            this.lines.Insert(0, new Line(""));
            this.lines.Insert(0, new Line(""));
            this.lines.Insert(0, new Line(""));
            this.lines.Insert(0, new Line("###############################################"));
            this.lines.Insert(0, new Line($"##     THIS FILE IS BUILT FROM \"{originalLanguage}\"       ##"));
            this.lines.Insert(0, new Line("## CHANGE USING THE AutomatedLanguage FOLDER ##"));
            this.lines.Insert(0, new Line("##    DO NOT CHANGE THE CONTENT MANUALLY     ##"));
            this.lines.Insert(0, new Line("##   THIS FILE IS GENERATED AUTOMATICALLY    ##"));
            this.lines.Insert(0, new Line("###############################################"));
        }
    }
}
