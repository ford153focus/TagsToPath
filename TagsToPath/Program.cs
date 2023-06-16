using System.Runtime.InteropServices;
using ATL;

namespace TagsToPath
{
    internal static class Program
    {
        private static string GenerateFileName(Track theTrack)
        {
            int? dn = theTrack.DiscNumber != 0 ? theTrack.DiscNumber : 1;
            int? tn = theTrack.TrackNumber != 0 ?  theTrack.TrackNumber : 1;
            string tnS = tn.ToString()!.PadLeft(2, '0');
            string ext = theTrack.AudioFormat.ShortName.ToLower();

            string title = string.Concat(
                theTrack.Title.Split(Path.GetInvalidFileNameChars())
            );

            if (theTrack.AudioFormat.IsValidExtension(".flac")) ext = "flac";
            if (theTrack.AudioFormat.IsValidExtension(".mp3")) ext =  "mp3";
            if (theTrack.AudioFormat.IsValidExtension(".m4a")) ext =  "m4a";
            if (theTrack.AudioFormat.IsValidExtension(".ogg")) ext = "ogg";

            string fName = $"[{dn}.{tnS}] {title}.{ext}";

            return fName;
        }

        private static string GenerateFolderName(Track theTrack)
        {
            string album = string.Concat(
                theTrack.Album.Split(Path.GetInvalidFileNameChars())
            );

            // ReSharper disable once CanSimplifyDictionaryLookupWithTryGetValue
            if (theTrack.AdditionalFields.ContainsKey("ORIGINALDATE"))
            {
                return $"[{theTrack.AdditionalFields["ORIGINALDATE"]}] {album}";
            }

            if (theTrack.Date!.Value.Year != 1 && theTrack.Date!.Value.Month != 1 && theTrack.Date!.Value.Day != 1)
            {
                string date = ((DateTime)theTrack.Date).ToString("yyyy-MM-dd");
                return $"[{date}] {album}";
            }

            return $"[{theTrack.Year}] {album}";
        }

        private static void FixDualArtistTags(Track theTrack)
        {
            int ftPosition = theTrack.Artist.IndexOf(" feat. ");
            if (ftPosition == -1) return;

            int trackAdditionPosition = theTrack.Title.IndexOf('(');

            string artist1 = theTrack.Artist.Substring(0,ftPosition);
            string artist2 = theTrack.Artist.Substring(ftPosition).Trim();

            theTrack.Artist = artist1;

            if (trackAdditionPosition == -1) {
                theTrack.Title = $"{theTrack.Title} ({artist2})";
            } else {
                string title = theTrack.Title.Substring(0, trackAdditionPosition).Trim();
                string addition = theTrack.Title.Substring(trackAdditionPosition);
                theTrack.Title = $"{title} ({artist2}) {addition}";
            }

            theTrack.Save();
        }

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

                if (theTrack.Year == 0 && theTrack.AdditionalFields.TryGetValue("ORIGINALYEAR", out var oyField))
                {
                    theTrack.Year = int.Parse(oyField);
                    theTrack.Save();
                }

                if (theTrack.Date!.Value.Year == 1 && theTrack.AdditionalFields.TryGetValue("ORIGINALDATE", out var odField))
                {
                    theTrack.Date = DateTime.Parse(odField);
                    theTrack.Save();
                }

                FixDualArtistTags(theTrack);

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
                    GenerateFolderName(theTrack),
                    GenerateFileName(theTrack),
                });

                Directory.CreateDirectory(Path.GetDirectoryName(newPath)!);
                if (path == newPath) throw new Exception("Old and new paths are same");
                File.Move(path, newPath, true);

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
}
