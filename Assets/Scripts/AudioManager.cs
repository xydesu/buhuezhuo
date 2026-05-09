using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class AudioManager : MonoBehaviour
{
    public Slider audioSlider;
    public Button audioButton;
    public Sprite Audio100;
    public Sprite Audio60;
    public Sprite Audio0;
    private float audioVolume;
    private float previousVolume;
    private string savePath;
    private bool isMuted;
    private bool isToggling;

    private AudioSource _audioSource;

    void Start()
    {
        // 初始化音量設置
        savePath = Path.Combine(Application.persistentDataPath, "audioSettings.json");
        LoadAudioSettings();
        audioSlider.onValueChanged.AddListener(delegate { OnVolumeChange(); });
        audioButton.onClick.AddListener(delegate { ToggleMute(); });
        _audioSource = GetComponent<AudioSource>();
        _audioSource.volume = audioVolume;
    }

    void OnVolumeChange()
    {
        if (audioSlider.value == 0)
        {
            isMuted = true;
            Debug.Log("Muted due to slider value being 0");
        }
        else if (isMuted)
        {
            isMuted = false;
            Debug.Log("Unmuted due to slider change");
        }

        // 更新音量值
        audioVolume = audioSlider.value;
        UpdateAudioIcon();
        SaveAudioSettings();
        // 這裡可以添加代碼來設置實際的音量，例如：
        // AudioListener.volume = audioVolume;
    }

    void UpdateAudioIcon()
    {
        // 根據音量值更新按鈕圖示
        if (isMuted || audioVolume == 0)
        {
            audioButton.image.sprite = Audio0;
        }
        else if (audioVolume > 0.6f)
        {
            audioButton.image.sprite = Audio100;
        }
        else
        {
            audioButton.image.sprite = Audio60;
        }
    }

    void SaveAudioSettings()
    {
        AudioManagerSettings settings = new AudioManagerSettings();
        settings.volume = audioVolume;
        settings.isMuted = isMuted;
        string json = JsonUtility.ToJson(settings);
        File.WriteAllText(savePath, json);
    }

    void LoadAudioSettings()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            AudioManagerSettings settings = JsonUtility.FromJson<AudioManagerSettings>(json);
            audioVolume = settings.volume;
            isMuted = settings.isMuted;
            audioSlider.value = audioVolume;
            UpdateAudioIcon();
        }
    }

    void ToggleMute()
    {
        if (isToggling) return; // 防止重複點擊
        isToggling = true;

        if (isMuted)
        {
            previousVolume = audioVolume;
            audioSlider.value = 0;
            Debug.Log("Audio muted, slider value set to 0");
            Debug.Log("Previous volume: " + previousVolume);
        }
        else
        {
            audioSlider.value = previousVolume;
            audioVolume = previousVolume;
            Debug.Log("Audio unmuted, slider value set to " + audioVolume);
        }

        isMuted = !isMuted;
        Debug.Log("Mute toggled: " + isMuted);

        UpdateAudioIcon();
        SaveAudioSettings();

        // 重置防重複點擊標誌
        isToggling = false;
        // 這裡可以添加代碼來設置實際的音量，例如：
        // AudioListener.volume = isMuted ? 0 : audioVolume;
    }
}
