// ReSharper disable CanSimplifyDictionaryLookupWithTryGetValue
using ATL;

namespace TagsToPath;

public static class Generators
{
    public static string GenerateFileName(Track theTrack)
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

    public static string GenerateFolderName(Track theTrack)
    {
        string album = string.Concat(
            theTrack.Album.Split(Path.GetInvalidFileNameChars())
        );

        if (theTrack.Date!.Value.Year != 1)
        {
            string date = ((DateTime)theTrack.Date).ToString("yyyy-MM-dd");
            return $"[{date}] {album}";
        }
        
        return $"[{theTrack.Year}] {album}";
    }
}