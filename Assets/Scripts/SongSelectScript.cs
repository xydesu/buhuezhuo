using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System.IO;

public class SongSelectScript : MonoBehaviour
{
    public TextAsset jsonFile;
    public Sprite SongSelectSprite;
    //public TextMeshProUGUI temp;
    public GameObject SongSelectButton;
    // Start is called before the first frame update
    public Image Jacket;
    public TextMeshProUGUI nameobj, artistobj;
    public AudioSource preview;

    private PlayButton playButton;

    private static string songName;

    private string saveFilePath;

    void Start()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "lastSelectedSong.json");

        string jsonText = jsonFile.text;
        Song song = JsonUtility.FromJson<Song>(jsonText);
        GameObject Canvas = GameObject.FindGameObjectWithTag("Canvas");
        GameObject songlist = GameObject.FindGameObjectWithTag("songlist");
        if (jsonFile != null)
        {
            //Debug.Log("JSON Text: " + jsonText);

            foreach (Songs songs in song.songs)
            {
                //Debug.Log(songs.id + " " + songs.name + " " + songs.artist);
                foreach (Level level in songs.levels)
                {
                    //Debug.Log("Diff: " + level.diff + ", Level: " + level.level);
                }
            }
        }
        else
        {
            //Debug.LogError("JSON file is null!");
        }
        int index = 0;
        foreach (Songs songs in song.songs)
        {
            foreach (Level level in songs.levels)
            {
                if (level.diff == 2)
                {
                    GameObject songobj = GameObject.Instantiate(SongSelectButton);
                    songobj.name = songs.id;
                    songobj.transform.SetParent(songlist.transform, false);
                    RectTransform rt = songobj.transform.GetComponent<RectTransform>();
                    rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, rt.anchoredPosition.y - (index * 90));
                    //Debug.Log(index);
                    songobj.SetActive(true);
                    songobj.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = level.level.ToString();
                    songobj.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = songs.name;
                    songobj.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = songs.artist;
                    StartCoroutine(CheckAndStartMarquee(songobj.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>()));
                    StartCoroutine(CheckAndStartMarquee(songobj.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>()));
                    //Debug.Log(songobj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text);
                    //Debug.Log(songobj.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text);
                    //Debug.Log(songobj.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text);
                    /*songobj.AddComponent<SongSelectScript>();
                    songobj.GetComponent<SongSelectScript>().jsonFile = jsonFile;
                    songobj.GetComponent<SongSelectScript>().SongSelectSprite = SongSelectSprite;
                    songobj.GetComponent<SongSelectScript>().SongSelectButton = SongSelectButton;
                    songobj.GetComponent<SongSelectScript>().Jacket = Jacket;*/

                    Button btn = songobj.GetComponent<Button>();
                    btn.onClick.AddListener(() =>
                    {
                        UpdateSong(songs.id, songs.name, songs.artist);
                        SaveLastSelectedSong(songs.id, songs.name, songs.artist);
                    });

                }

            }
            index++;

        }

        LoadLastSelectedSong();
    }

    public string GetSongName()
    {
        return songName;
    }

    // Update is called once per frame
    void Update()
    {
    }

    void UpdateSong(string SongID, string SongName, string SongArtist)
    {
        if (playButton == null)
        {
            playButton = gameObject.AddComponent<PlayButton>();
        }
        playButton.SetPlaySong(SongID);
        var _jacket = Resources.Load<Sprite>("Songs/" + SongID + "/Jacket");
        Jacket.GetComponent<Image>().sprite = _jacket;
        nameobj.GetComponent<TextMeshProUGUI>().text = SongName;
        artistobj.GetComponent<TextMeshProUGUI>().text = SongArtist;
        songName = SongName;
        var audioClip = Resources.Load<AudioClip>("Songs/" + SongID + "/preview");
        preview.clip = audioClip;
        preview.volume = 1;
        preview.Play();
        StartCoroutine(Fade(true, preview, audioClip.length, 1f));
        StartCoroutine(Fade(false, preview, audioClip.length, 0f));

        // 檢查並啟動跑馬燈效果
        StartCoroutine(CheckAndStartMarquee(nameobj.GetComponent<TextMeshProUGUI>()));
        StartCoroutine(CheckAndStartMarquee(artistobj.GetComponent<TextMeshProUGUI>()));
    }

    IEnumerator CheckAndStartMarquee(TextMeshProUGUI textObj)
    {
        yield return null; // 等待一幀以確保文字已經更新

        if (textObj.preferredWidth > textObj.rectTransform.rect.width)
        {
            StartCoroutine(MarqueeText(textObj));
        }
    }

    IEnumerator MarqueeText(TextMeshProUGUI textObj)
    {
        textObj.overflowMode = TextOverflowModes.Overflow; // 設置 overflowMode 為 Overflow

        float scrollSpeed = 50f; // 跑馬燈速度
        Vector3 startPos = textObj.rectTransform.localPosition + new Vector3(textObj.rectTransform.rect.width, 0, 0);
        Vector3 endPos = textObj.rectTransform.localPosition - new Vector3(textObj.rectTransform.rect.width, 0, 0);

        while (true)
        {
            // 檢查文字寬度是否超過 GameObject 寬度
            if (textObj.preferredWidth <= textObj.rectTransform.rect.width)
            {
                textObj.rectTransform.localPosition = new Vector3(0, textObj.rectTransform.localPosition.y, textObj.rectTransform.localPosition.z);
                yield break; // 停止協程
            }

            float elapsedTime = 0f;
            while (elapsedTime < (textObj.preferredWidth / scrollSpeed))
            {
                textObj.rectTransform.localPosition = Vector3.Lerp(startPos, endPos, elapsedTime / (textObj.preferredWidth / scrollSpeed));
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            textObj.rectTransform.localPosition = startPos;
        }
    }

    public IEnumerator Fade(bool fadeIn, AudioSource source, float duration, float targetVolume)
    {
        if (!fadeIn)
        {
            double lengthOfSource = (double)source.clip.samples / source.clip.frequency;
            yield return new WaitForSecondsRealtime((float)(lengthOfSource - duration));
        }

        float time = 0f;
        float startVol = source.volume;
        while (time < duration)
        {
            time += Time.deltaTime;
            source.volume = Mathf.Lerp(startVol, targetVolume, time / duration);
            yield return null;
        }

        yield break;
    }

    void SaveLastSelectedSong(string songID, string songName, string songArtist)
    {
        LastSelectedSong lastSelectedSong = new LastSelectedSong
        {
            SongID = songID,
            SongName = songName,
            SongArtist = songArtist
        };

        string json = JsonUtility.ToJson(lastSelectedSong);
        File.WriteAllText(saveFilePath, json);
    }

    void LoadLastSelectedSong()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            LastSelectedSong lastSelectedSong = JsonUtility.FromJson<LastSelectedSong>(json);
            UpdateSong(lastSelectedSong.SongID, lastSelectedSong.SongName, lastSelectedSong.SongArtist);
        }
    }
}
