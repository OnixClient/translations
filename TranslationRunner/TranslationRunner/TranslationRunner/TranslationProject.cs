using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;

namespace TranslationRunner
{
    public class TranslationProject
    {
        List<TranslationLanguage> languages = new List<TranslationLanguage>();
        Dictionary<string, string> fileMd5 = new Dictionary<string, string>();
        public string name { get; set; }
        public string path { get {
                return Path.Combine(Directory.GetCurrentDirectory(), name);    
            }
        }

        public string GetLanguagePath(TranslationLanguage lang)
        {
            return Path.Combine(Path.Combine(path, "languages"), lang.code + ".lang");
        }
        public TranslationLanguage? GetLanguage(string code)
        {
            return languages.Where(x => x.code == code).First();
        }
        public List<TranslationLanguage> GetChanged()
        {
            List<TranslationLanguage> changed = new List<TranslationLanguage>();
            foreach (TranslationLanguage lang in languages)
            {
                if (lang.md5 != fileMd5[lang.code])
                    changed.Add(lang);
            }
            return changed;
        }
        public void UpdateLanguage(TranslationLanguage language)
        {
            var found = languages.Where(x => x.code == language.code).First();
            if (!(found is null))
            {
                languages.Remove(found);
            }
            languages.Add(language);
        }
        public string GetLatestKnownHash(TranslationLanguage lang)
        {
            return GetLatestKnownHash(lang.code);
        }
        public string GetLatestKnownHash(string code)
        {
            if (!fileMd5.ContainsKey(code))
                return "";
            return fileMd5[code];
        }
        public void ComputeChanges()
        {
            fileMd5.Clear();
            Mutex mutex = new Mutex();
            Parallel.ForEach(Directory.GetFiles(Path.Combine(path, "languages")), (langFull) => {

                if (!File.Exists(langFull))
                {
                    File.Copy(Path.Combine(path, Path.Combine("languages", "en_US.lang")), langFull);
                    System.Threading.Thread.Sleep(500);
                }
                mutex.WaitOne();
                fileMd5.Add(Path.GetFileNameWithoutExtension(langFull), Utils.GetFileMD5(langFull));
                mutex.ReleaseMutex();
            });
        }
        public List<TranslationLanguage> GetLanguages()
        {
            return languages;
        }
        public void Save()
        {
            JsonIndexFile index = new JsonIndexFile()
            {
                languages = languages
            };
            var jsonSerializerSettings = new JsonSerializerOptions();
            jsonSerializerSettings.WriteIndented = true;
            File.WriteAllText(Path.Combine(path, "index.json"), JsonSerializer.Serialize(index, jsonSerializerSettings));
        }
        public class JsonIndexFile
        {
            public List<TranslationLanguage> languages { get; set; }
        }
        public TranslationProject(string name)
        {
            this.name = name;
            this.languages = JsonSerializer.Deserialize<JsonIndexFile>(File.ReadAllText(Path.Combine(path, "index.json"))).languages;
            if (languages == null)
                languages = new List<TranslationLanguage>();
            foreach (TranslationLanguage lang in languages)
            {
                lang.path = GetLanguagePath(lang);
                if (string.IsNullOrEmpty(lang.md5))
                    lang.md5 = "";
            }

            ComputeChanges();
        }
    }
}
