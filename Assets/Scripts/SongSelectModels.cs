[System.Serializable]
public class Song
{
    public Songs[] songs;
}

[System.Serializable]
public class Songs
{
    public string id;
    public string name;
    public string artist;
    public Level[] levels;
}

[System.Serializable]
public class Level
{
    public int diff;
    public string level;
}

[System.Serializable]
public class LastSelectedSong
{
    public string SongID;
    public string SongName;
    public string SongArtist;
}
