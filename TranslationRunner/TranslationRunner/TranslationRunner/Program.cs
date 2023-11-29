
// Sets the right directory for the program to run
using System.Threading.Channels;
using TranslationRunner;
using static TranslationRunner.LangFile;

Directory.SetCurrentDirectory(Directory.GetParent(Directory.GetCurrentDirectory()).ToString());



List<string> projectFolders = new List<string>();
foreach (string dir in Directory.GetDirectories(Directory.GetCurrentDirectory()))
{
    string dirName = Path.GetFileNameWithoutExtension(dir).ToLower();
    if (dirName == "translationrunner" || dirName == "automatedlanguages")
        continue;
    projectFolders.Add(dir);
}

List<TranslationProject> projects = new List<TranslationProject>();
foreach (string projectFolder in projectFolders)
{
    try
    {
        projects.Add(new TranslationProject(Path.GetFileNameWithoutExtension(projectFolder)));
    } catch (Exception e)
    {
        Console.WriteLine("Error: " + e.Message);
    }
}

Dictionary<string,AutomatedLanguage> automatedLanguages = new Dictionary<string, AutomatedLanguage>();
SortedSet<string> automatedTargets = new SortedSet<string>();
foreach (string automatedLanguageFile in Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "AutomatedLanguages")))
{
    try
    {
        var autoLang = new AutomatedLanguage(automatedLanguageFile);
        automatedLanguages.Add(autoLang.origin, autoLang);
        automatedTargets.Add(autoLang.target);
        Console.WriteLine($"Automated Language: {autoLang.origin} -> {autoLang.target}");

    }
    catch (Exception e)
    {
        Console.WriteLine($"Cant load Automated Language {Path.GetFileNameWithoutExtension(automatedLanguageFile)}: {e.Message}");
    }
}
if (automatedLanguages.Count > 0) Console.WriteLine();

foreach (TranslationProject project in projects)
{
    Console.WriteLine("Project: " + project.name);
    Console.WriteLine();

    foreach (var autoLang in automatedLanguages) {
		TranslationLanguage? targetLang = project.GetLanguage(autoLang.Value.target);
		TranslationLanguage? sourceLang = project.GetLanguage(autoLang.Value.origin);
		if (!(targetLang is null) && !(sourceLang is null)) {
			LangFile original = new LangFile(project.GetLanguagePath(sourceLang));
			LangFile target = new LangFile();
			target.AddAutoGenratedHeader(sourceLang.code);
			foreach (var value in original.lines) {

				if (value.IsEmpty || !value.HasKey) {
					target.lines.Add(value);
					continue;
				}
				string result = autoLang.Value.Correct(value.Key, value.Value);
				target.lines.Add(new LangFile.Line(value.Comment, value.Key, result));
			}
			target.Save(project.GetLanguagePath(targetLang));
		} else {
			Console.WriteLine("Could not find target lang in index: " + autoLang.Value.target);
		}
	}

	foreach (var changed in project.GetChanged())
    {
        if (automatedTargets.Contains(changed.code))
            continue; // Skip automated languages, as we will generate them later
        Console.WriteLine("Changed Language: " + changed.code);
    }

    //automated languages have been regenerated, now we can update the index
    project.ComputeChanges(); //to know about new regenerated languages

    if (!Directory.Exists(Path.Combine(project.path, "compressed_languages")))
		Directory.CreateDirectory(Path.Combine(project.path, "compressed_languages"));
    foreach (var changed in project.GetChanged())
    {
        string md5 = project.GetLatestKnownHash(changed.code);
        var newLang  = new TranslationLanguage()
        {
            code = changed.code,
            md5 = md5,
            path = changed.path,
            url = changed.url,
            version = changed.version+1,
            visual_name = changed.visual_name
        };
        project.UpdateLanguage(newLang);


        string compressedFilePath = changed.path.Replace("languages", "compressed_languages");
        if (File.Exists(compressedFilePath))
			File.Delete(compressedFilePath);
        Utils.CompressFile(changed.path, compressedFilePath);
    }

    LangFile mainLang = new LangFile(project.GetLanguagePath(project.GetLanguage("en_US")!));
    if (!Directory.Exists(Path.Combine(project.path, "missing")))
        Directory.CreateDirectory(Path.Combine(project.path, "missing"));
    Parallel.ForEach(project.GetLanguages(), (lang) =>
    {
        if (lang.code == "en_US")
            return; //source of missing keys
        if (automatedTargets.Contains(lang.code))
			return; // Skip automated languages as its because the origin language's problem
        LangFile currentLang = new LangFile(project.GetLanguagePath(lang));
        List<string> missingKeys = currentLang.GetMissingKeys(mainLang);

        //add the value from english when possible
        List<string> friendlyMisses = new List<string>();
        foreach (string key in missingKeys)
        {
            LangFile.Line? line = mainLang.GetLine(key);
            if (line is null)
                friendlyMisses.Add(key + "=");
            else
                friendlyMisses.Add(line.ToString());
        }

        string missPath = Path.Combine(project.path, Path.Combine("missing", lang.code + ".lang"));
        if (missingKeys.Count == 0)
            if (File.Exists(missPath))
                File.Delete(missPath);
            else
                return;
        else
        {
            File.WriteAllLines(missPath, friendlyMisses);
        }
    });
    project.Save();
}







