namespace TagsToPath;

public static class Utils
{
    /** Move cover file if exists */
    public static void CoverSave(string oldPath, string newPath)
    {
        List<string> fileNames = new List<string> {
            "albumart", "Albumart",
            "Cover", "cover",
            "Folder", "folder",
            "Front", "front",
        };

        List<string> fileExtensions = new List<string> {
            "gif",
            "jpg", "jpeg",
            "png",
            "tiff",
            "webp",
        };

        string newFolder = Path.GetDirectoryName(newPath)!;
        string oldFolder = Path.GetDirectoryName(oldPath)!;

        foreach (var fName in fileNames)
        {
            foreach (var fExt in fileExtensions)
            {
                string oldCoverPath = Path.Combine(oldFolder, $"{fName}.{fExt}");
                string newCoverPath = Path.Combine(newFolder, $"folder.{fExt}");

                if (File.Exists(oldCoverPath))
                {
                    File.Move(oldCoverPath, newCoverPath);
                }
            }
        }
    }
}
