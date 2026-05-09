using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game
{
    public partial class SongPlaying
    {
        void Update()
        {
            UpdateMaxCombo();
            CheckSongEnd();
            UpdateScoreDisplay();
            HandleInput();
            HandleTargetVisibility(KeyCode.D, TargetNote_1);
            HandleTargetVisibility(KeyCode.F, TargetNote_2);
            HandleTargetVisibility(KeyCode.J, TargetNote_3);
            HandleTargetVisibility(KeyCode.K, TargetNote_4);
            HandleKeyPress();
        }

        void FixedUpdate()
        {
            if (!isPause)
            {
                UpdateSongTime();
                UpdateNotes();
            }
        }

        void UpdateSongTime()
        {
            songTime = BGM.GetComponent<AudioSource>().time;
            songTimeOBJ.GetComponent<TextMeshProUGUI>().text = songTime.ToString();
        }

        void UpdateNotes()
        {
            foreach (var note in notes)
            {
                float timeToSpawn = note.Time;
                timeToSpawnOBJ.GetComponent<TextMeshProUGUI>().text = timeToSpawn.ToString();
                if (songTime >= timeToSpawn - 2f && !noteSpawned[note])
                {
                    Debug.Log("Spawning note at lane: " + note.Lane);
                    CreateNote(note.Lane);
                    noteSpawned[note] = true;
                }
                else if (songTime >= timeToSpawn && !noteSpawned[note])
                {
                    // 容錯機制，確保音符能夠準時生成
                    Debug.LogWarning("Late spawning note at lane: " + note.Lane);
                    CreateNote(note.Lane);
                    noteSpawned[note] = true;
                }
            }

            MoveNotes(_Note_1_List, TargetNote_1);
            MoveNotes(_Note_2_List, TargetNote_2);
            MoveNotes(_Note_3_List, TargetNote_3);
            MoveNotes(_Note_4_List, TargetNote_4);
        }

        void UpdateMaxCombo()
        {
            if (combo > maxcombo)
            {
                maxcombo = combo;
            }
        }

        void CheckSongEnd()
        {
            if (BGM.GetComponent<AudioSource>().time >= BGM.GetComponent<AudioSource>().clip.length)
            {
                ShowResult();
            }
        }

        void ShowResult()
        {
            result.SetActive(true);
            result_Score.GetComponent<TextMeshProUGUI>().text = score.ToString();
            result_Perfect.GetComponent<TextMeshProUGUI>().text = countPerfect.ToString();
            result_Great.GetComponent<TextMeshProUGUI>().text = countGreat.ToString();
            result_Miss.GetComponent<TextMeshProUGUI>().text = countMiss.ToString();
            result_Combo.GetComponent<TextMeshProUGUI>().text = "Combo: " + maxcombo.ToString();
            UpdateRank();
            UpdateAPFC();
        }

        void UpdateRank()
        {
            if (score == 1000000)
            {
                result_Rank.GetComponent<TextMeshProUGUI>().text = "SSS+";
                result_Rank.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.5f, 0f);
            }
            else if (score >= 990000)
            {
                result_Rank.GetComponent<TextMeshProUGUI>().text = "SSS";
                result_Rank.GetComponent<TextMeshProUGUI>().color = Color.yellow;
            }
            else if (score >= 980000)
            {
                result_Rank.GetComponent<TextMeshProUGUI>().text = "SS";
                result_Rank.GetComponent<TextMeshProUGUI>().color = Color.yellow;
            }
            else if (score >= 970000)
            {
                result_Rank.GetComponent<TextMeshProUGUI>().text = "S";
                result_Rank.GetComponent<TextMeshProUGUI>().color = Color.yellow;
            }
            else if (score >= 950000)
            {
                result_Rank.GetComponent<TextMeshProUGUI>().text = "A";
                result_Rank.GetComponent<TextMeshProUGUI>().color = Color.green;
            }
            else if (score >= 900000)
            {
                result_Rank.GetComponent<TextMeshProUGUI>().text = "B";
                result_Rank.GetComponent<TextMeshProUGUI>().color = Color.blue;
            }
            else if (score >= 800000)
            {
                result_Rank.GetComponent<TextMeshProUGUI>().text = "C";
                result_Rank.GetComponent<TextMeshProUGUI>().color = Color.red;
            }
            else
            {
                result_Rank.GetComponent<TextMeshProUGUI>().text = "D";
                result_Rank.GetComponent<TextMeshProUGUI>().color = Color.red;
            }
        }

        void UpdateAPFC()
        {
            if (countGreat == 0 && countMiss == 0)
            {
                result_APFC.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.5f, 0f);
                result_APFC.GetComponent<TextMeshProUGUI>().text = "AP";
            }
            else if (countMiss == 0)
            {
                result_APFC.GetComponent<TextMeshProUGUI>().color = Color.green;
                result_APFC.GetComponent<TextMeshProUGUI>().text = "FC";
            }
            else
            {
                result_APFC.GetComponent<TextMeshProUGUI>().text = "";
            }
        }

        void UpdateScoreDisplay()
        {
            countPerfectOBJ.GetComponent<TextMeshProUGUI>().text = "Perfect: " + countPerfect;
            countGreatOBJ.GetComponent<TextMeshProUGUI>().text = "Great: " + countGreat;
            countMissOBJ.GetComponent<TextMeshProUGUI>().text = "Miss: " + countMiss;
            countComboOBJ.GetComponent<TextMeshProUGUI>().text = combo.ToString();

            countComboOBJ.SetActive(combo != 0);

            if (displayedScore < score)
            {
                displayedScore += Mathf.CeilToInt((score - displayedScore) * 0.1f);
                if (displayedScore > score)
                {
                    displayedScore = score;
                }
            }
            scoreOBJ.GetComponent<TextMeshProUGUI>().text = displayedScore.ToString("D7");
        }

        void HandleInput()
        {
            if (Input.GetKeyDown(keys[4]))
            {
                pause.SetActive(!pause.activeSelf);
                isPause = !isPause;
                if (isPause)
                {
                    BGM.GetComponent<AudioSource>().Pause();
                }
                else
                {
                    BGM.GetComponent<AudioSource>().UnPause();
                }
            }
        }

        void HandleKeyPress()
        {
            for (int i = 0; i < keys.Length; i++)
            {
                if (Input.GetKeyDown(keys[i]))
                {
                    HandleJudgment(i + 1);
                }
            }
        }

        IEnumerator StartSongPlaying()
        {
            yield return new WaitForSeconds(1f);
            BGM.GetComponent<AudioSource>().Play();
            playing = true;
            isPause = false;
        }

        void MoveNotes(List<GameObject> noteList, GameObject target)
        {
            for (int i = noteList.Count - 1; i >= 0; i--)
            {
                if (noteList[i] != null && target != null)
                {
                    noteList[i].transform.position = Vector3.MoveTowards(noteList[i].transform.position, target.transform.position, speed * Time.deltaTime);
                    JudgeTime.GetComponent<TextMeshProUGUI>().text = Mathf.Round(noteList[i].transform.position.y) + "/" + Mathf.Round(target.transform.position.y);
                    // Destroy note if it reaches the target and remove it from the list
                    if (Mathf.Round(noteList[i].transform.position.y) == Mathf.Round(target.transform.position.y))
                    {
                        Destroy(noteList[i]);
                        noteList.RemoveAt(i);
                        DisplayJudgeResult(Judge_Miss);
                        countMiss++; // 增加 miss 計數
                        combo = 0; // 重置 combo 計數
                    }
                }
            }
        }

        IEnumerator JudgeReset(GameObject judge)
        {
            yield return new WaitForSeconds(0.5f);
            judge.SetActive(false);
        }

        IEnumerator TestNote()
        {
            for (; ; )
            {
                yield return new WaitForSeconds(.5f);
                CreateNote(1);
                yield return new WaitForSeconds(.5f);
                CreateNote(2);
                yield return new WaitForSeconds(.5f);
                CreateNote(3);
                yield return new WaitForSeconds(.5f);
                CreateNote(4);
            }
        }

        void HandleTargetVisibility(KeyCode key, GameObject target)
        {
            if (Input.GetKey(key))
            {
                target.SetActive(true);
            }
            else
            {
                target.SetActive(false);
            }
        }

        void CreateNote(int note)
        {
            GameObject TagNote = GameObject.FindGameObjectWithTag("TagNote");
            GameObject newNote = null;

            switch (note)
            {
                case 1:
                    newNote = Instantiate(Note_1);
                    _Note_1_List.Add(newNote);
                    _Note_1_Times.Add(Time.time); // Store the creation time of the note
                    break;
                case 2:
                    newNote = Instantiate(Note_2);
                    _Note_2_List.Add(newNote);
                    _Note_2_Times.Add(Time.time);
                    break;
                case 3:
                    newNote = Instantiate(Note_3);
                    _Note_3_List.Add(newNote);
                    _Note_3_Times.Add(Time.time);
                    break;
                case 4:
                    newNote = Instantiate(Note_4);
                    _Note_4_List.Add(newNote);
                    _Note_4_Times.Add(Time.time);
                    break;
            }

            if (newNote != null)
            {
                newNote.SetActive(true);
                newNote.transform.SetParent(TagNote.transform, false);
            }
        }

        void HandleJudgment(int note)
        {
            List<GameObject> currentNoteList = null;
            List<float> currentNoteTimes = null;
            GameObject target = null;

            switch (note)
            {
                case 1:
                    currentNoteList = _Note_1_List;
                    currentNoteTimes = _Note_1_Times;
                    target = TargetNote_1;
                    break;
                case 2:
                    currentNoteList = _Note_2_List;
                    currentNoteTimes = _Note_2_Times;
                    target = TargetNote_2;
                    break;
                case 3:
                    currentNoteList = _Note_3_List;
                    currentNoteTimes = _Note_3_Times;
                    target = TargetNote_3;
                    break;
                case 4:
                    currentNoteList = _Note_4_List;
                    currentNoteTimes = _Note_4_Times;
                    target = TargetNote_4;
                    break;
            }

            if (currentNoteList != null && currentNoteList.Count > 0)
            {
                GameObject noteObject = currentNoteList[0]; // Check the first note in the list
                float timeDiff = Mathf.Abs(noteObject.transform.position.y - target.transform.position.y);

                if (timeDiff <= perfectWindow * 2)
                {
                    DisplayJudgeResult(Judge_Perfect);
                    countPerfect++;
                    combo++;
                    score += Mathf.CeilToInt((float)maxScore / totalNotes); // Perfect score
                }
                else if (timeDiff <= greatWindow * 2)
                {
                    DisplayJudgeResult(Judge_Great);
                    countGreat++;
                    combo++;
                    score += Mathf.CeilToInt((float)maxScore / totalNotes * 0.6f); // Great score
                }
                else if (timeDiff <= missWindow * 2)
                {
                    DisplayJudgeResult(Judge_Miss);
                    countMiss++;
                    combo = 0; // Reset combo on miss
                }
                else
                {
                    JudgeTime.GetComponent<TextMeshProUGUI>().text = timeDiff.ToString();
                    return;
                }

                JudgeTime.GetComponent<TextMeshProUGUI>().text = timeDiff.ToString();

                // Play judge audio
                JudgeAudio.GetComponent<AudioSource>().Play();

                // Remove note from list and destroy it
                Destroy(noteObject);
                currentNoteList.RemoveAt(0);
                currentNoteTimes.RemoveAt(0);
            }

            // 確保分數不超過最大值
            if (score > maxScore)
            {
                score = maxScore;
            }
        }

        void DisplayJudgeResult(Sprite judgmentSprite)
        {
            Judge.GetComponent<Image>().sprite = judgmentSprite;
            Judge.SetActive(true);

            // Reset the coroutine if it's already running
            if (judgeResetCoroutine != null)
            {
                StopCoroutine(judgeResetCoroutine);
            }

            judgeResetCoroutine = StartCoroutine(JudgeReset(Judge));
        }

        public void ResumeButton()
        {
            isPause = false;
            pause.SetActive(false);
            BGM.GetComponent<AudioSource>().UnPause();
        }

        public void RemuseButton()
        {
            ResumeButton();
        }

        public void RestartButton()
        {
            SceneManager.LoadScene("SongPlaying");
        }

        public void ExitButton()
        {
            SceneManager.LoadScene("SongSelect");
        }
    }
}
