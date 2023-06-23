using ATL;

namespace TagsToPath;

internal static class Program
{
    private static void ProcessFile(string path, string folder)
    {
        if (Directory.Exists(path)) {
            ProcessFolder(path, folder);
            return;
        }

        try
        {
            if (!File.Exists(path)) throw new Exception($"File '{path}' not found");
            Track theTrack = new Track(path);

            Fixers.FixDiscNumber(theTrack);
            Fixers.FixDualArtistTags(theTrack);
            Fixers.FixDiscogsArtistTag(theTrack);
            Fixers.FixYear(theTrack);

            #region Check tags
            if (string.IsNullOrEmpty(theTrack.Artist)) throw new Exception($"Tag 'Artist' is not presented in file {path}");
            if (theTrack.Year == 0 && theTrack.Date!.Value.Year == 1) throw new Exception($"Tags 'Year' and|or 'Date' are not presented in file {path}");
            if (string.IsNullOrEmpty(theTrack.Album)) throw new Exception($"Tag 'Album' is not presented in file {path}");
            if (theTrack.DiscNumber == 0) throw new Exception($"Tag 'DiscNumber' is not presented in file {path}");
            if (theTrack.TrackNumber == 0) throw new Exception($"Tag 'TrackNumber' is not presented in file {path}");
            if (string.IsNullOrEmpty(theTrack.Title)) throw new Exception($"Tag 'Title' is not presented in file {path}");
            #endregion

            string newPath = Path.Combine(new [] {
                folder,
                string.Concat(
                    theTrack.Artist.Split(Path.GetInvalidFileNameChars())
                ),
                Generators.GenerateFolderName(theTrack),
                Generators.GenerateFileName(theTrack),
            });

            if (path == newPath) throw new Exception("Old and new paths are same");
            Directory.CreateDirectory(Path.GetDirectoryName(newPath)!);
            File.Move(path, newPath, true);

            Utils.CoverSave(path, newPath);

            Console.WriteLine("Moved");
            Console.WriteLine(path);
            Console.WriteLine("to");
            Console.WriteLine(newPath);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    private static void ProcessFolder(string source, string target)
    {
        if (Directory.Exists(source))
        {
            foreach (string file in Directory.GetFiles(source))
            {
                ProcessFile(file, target);
            }

            foreach (string directory in Directory.GetDirectories(source))
            {
                ProcessFolder(directory, target);
            }
        }

        if (File.Exists(source))
        {
            ProcessFile(source, target);
        }
    }

    private static void Main(string[] args)
    {
        if (args.Length == 2)
        {
            string source = args[0];
            string target = args[1];
            ProcessFolder(source, target);
            return;
        }

        Console.Write("Enter full path target folder: ");
        string folder = Console.ReadLine() ?? throw new InvalidOperationException();

        while (true)
        {
            Console.Write("Drag files: ");
            string inputStr = Console.ReadLine() ?? throw new InvalidOperationException();
            var inputPaths = inputStr.Trim('"').Split("\"\"");

            foreach (string path in inputPaths)
            {
                ProcessFile(path, folder);
            }
        }
    }
}
