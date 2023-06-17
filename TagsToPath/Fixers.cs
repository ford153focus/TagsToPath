using ATL;

namespace TagsToPath;

public static class Fixers
{
    /** Use full release date with month and day */
    public static void FixYear(Track theTrack)
    {
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
    }
    
    /** if disc info not presented fill with stub */
    public static void FixDiscNumber(Track theTrack)
    {
        if (theTrack.DiscNumber != 0) return;
        if (theTrack.DiscTotal != 0) return;
        
        theTrack.DiscNumber = 1;
        theTrack.DiscTotal = 1;
        
        theTrack.Save();
    }
    
    /** Move second 'feat.'-artist to title */
    public static void FixDualArtistTags(Track theTrack)
    {
        int ftPosition = theTrack.Artist.IndexOf(" feat. ", StringComparison.InvariantCultureIgnoreCase);
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
}