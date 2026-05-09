using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace Game
{
    public partial class SongPlaying : MonoBehaviour
    {
        public GameObject Note_1, Note_2, Note_3, Note_4;
        public GameObject TargetNote_1, TargetNote_2, TargetNote_3, TargetNote_4;
        public GameObject Judge;
        //public Sprite Judge_Perfect, Judge_Perfect_Plus, Judge_Perfect_Minus;
        //public Sprite Judge_Great, Judge_Great_Plus, Judge_Great_Minus, Judge_Miss;
        public Sprite Judge_Perfect, Judge_Great, Judge_Good, Judge_Miss;
        public GameObject JudgeAudio;
        public GameObject JudgeTime, Playing;
        public float speed = 1f; // Speed at which the note moves

        // Time windows in seconds (convert milliseconds to seconds)
        public float perfectWindow = 33;  // 33ms
        public float greatWindow = 66;    // 66ms
        public float missWindow = 200;

        private List<GameObject> _Note_1_List = new List<GameObject>();
        private List<GameObject> _Note_2_List = new List<GameObject>();
        private List<GameObject> _Note_3_List = new List<GameObject>();
        private List<GameObject> _Note_4_List = new List<GameObject>();

        // Track the time each note is created
        private List<float> _Note_1_Times = new List<float>();
        private List<float> _Note_2_Times = new List<float>();
        private List<float> _Note_3_Times = new List<float>();
        private List<float> _Note_4_Times = new List<float>();

        public GameObject BGM;

        public GameObject timeToSpawnOBJ, songTimeOBJ, BPMOBJ;

        public GameObject pause;

        Coroutine judgeResetCoroutine;

        public GameObject result;
        public GameObject result_Score, result_Perfect, result_Great, result_Miss, result_Combo, result_Rank, result_APFC;

        private bool playing = false;
        private bool isPause = true;

        private float bpm;
        private float secPerBeat;
        private float songTime = 0f;
        private readonly Dictionary<Note, bool> noteSpawned = new();

        int countPerfect = 0, countGreat = 0, countMiss = 0, combo = 0;
        int totalNotes;
        int score = 0;
        const int maxScore = 1000000;

        int maxcombo = 0;
        int displayedScore = 0; // 用於顯示動畫的分數

        public GameObject countPerfectOBJ, countGreatOBJ, countMissOBJ, countComboOBJ, scoreOBJ;

        List<Note> notes;

        private KeyCode[] keys;

        void Start()
        {
            playing = false;
            KeyManager keyManager = new KeyManager();
            keys = keyManager.GetKeyCodes();
            Playing.GetComponent<TextMeshProUGUI>().text = gameObject.AddComponent<SongSelectScript>().GetSongName();
            AudioClip _BGM = Resources.Load<AudioClip>("Songs/" + gameObject.AddComponent<PlayButton>().GetPlaySong() + "/track");

            BGM.GetComponent<AudioSource>().clip = _BGM;
            var ChartData = Resources.Load<TextAsset>("Songs/" + gameObject.AddComponent<PlayButton>().GetPlaySong() + "/chart");
            if (ChartData == null)
            {
                SceneManager.LoadScene("SongSelect");
                return;
            }
            string chart = ChartData.ToString();
            secPerBeat = 60f / bpm;
            var tokens = LexicalAnalysis(chart, out var tokenWarnings);
            foreach (var warning in tokenWarnings)
                PrintWarning(chart, warning);

            notes = ParseTokens(tokens, out var noteWarnings);
            foreach (var note in notes)
                Debug.Log($"{note.Time - 1.6f}: {note.Lane}");

            foreach (var warning in noteWarnings)
                PrintWarning(chart, warning);
            foreach (var note in notes)
            {
                noteSpawned[note] = false;
            }

            totalNotes = notes.Count; // 計算總音符數量
            StartCoroutine(StartSongPlaying());
        }
    }
}
