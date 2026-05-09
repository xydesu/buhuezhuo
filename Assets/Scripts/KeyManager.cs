using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public partial class KeyManager : MonoBehaviour
{
    public Button button1;
    public Button button2;
    public Button button3;
    public Button button4;
    public Button button5;

    public GameObject input;

    private string filePath;
    private KeyData keyData;
    private int currentButtonIndex = -1;

    public void Start()
    {
        filePath = Path.Combine(Application.persistentDataPath, "keys.json");
        LoadKeys();

        button1.onClick.AddListener(() => OnButtonClick(1));
        button2.onClick.AddListener(() => OnButtonClick(2));
        button3.onClick.AddListener(() => OnButtonClick(3));
        button4.onClick.AddListener(() => OnButtonClick(4));
        button5.onClick.AddListener(() => OnButtonClick(5));
    }

    private void Update()
    {
        if (currentButtonIndex != -1 && Input.anyKeyDown)
        {
            foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(keyCode))
                {
                    OnKeyInputChanged(keyCode, currentButtonIndex);
                    break;
                }
            }
        }
    }

    private void LoadKeys()
    {
        filePath = Path.Combine(Application.persistentDataPath, "keys.json");
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            if (string.IsNullOrEmpty(json))
            {
                CreateNewKeys();
            }
            else
            {
                keyData = JsonUtility.FromJson<KeyData>(json);
                UpdateButtonLabels();
            }
        }
        else
        {
            CreateNewKeys();
        }
    }

    private void CreateNewKeys()
    {
        keyData = new KeyData
        {
            key1 = KeyCode.D,
            key2 = KeyCode.F,
            key3 = KeyCode.J,
            key4 = KeyCode.K,
            key5 = KeyCode.Escape
        };

        string json = JsonUtility.ToJson(keyData);
        File.WriteAllText(filePath, json);
        UpdateButtonLabels();
    }

    private void UpdateButtonLabels()
    {
        //only working in option scene
        if (SceneManager.GetActiveScene().name == "Option")
        {
            button1.GetComponentInChildren<TextMeshProUGUI>().text = keyData.key1.ToString();
            button2.GetComponentInChildren<TextMeshProUGUI>().text = keyData.key2.ToString();
            button3.GetComponentInChildren<TextMeshProUGUI>().text = keyData.key3.ToString();
            button4.GetComponentInChildren<TextMeshProUGUI>().text = keyData.key4.ToString();
            button5.GetComponentInChildren<TextMeshProUGUI>().text = keyData.key5.ToString();
        }
    }

    private void OnButtonClick(int buttonIndex)
    {
        currentButtonIndex = buttonIndex;
        input.SetActive(true);
    }

    private void OnKeyInputChanged(KeyCode newKey, int buttonIndex)
    {
        Debug.Log("New key: " + newKey);

        switch (buttonIndex)
        {
            case 1:
                keyData.key1 = newKey;
                break;
            case 2:
                keyData.key2 = newKey;
                break;
            case 3:
                keyData.key3 = newKey;
                break;
            case 4:
                keyData.key4 = newKey;
                break;
            case 5:
                keyData.key5 = newKey;
                break;
        }

        currentButtonIndex = -1;
        UpdateButtonLabels();
        SaveKeys();
        input.SetActive(false);
    }

    private void SaveKeys()
    {
        string json = JsonUtility.ToJson(keyData);
        File.WriteAllText(filePath, json);
    }

    public KeyCode[] GetKeyCodes()
    {
        LoadKeys();
        //load key data from file and return as KeyCode array
        return new KeyCode[]
        {
            keyData.key1,
            keyData.key2,
            keyData.key3,
            keyData.key4,
            keyData.key5
        };
    }

    public void initkey()
    {
        LoadKeys();
    }

}
