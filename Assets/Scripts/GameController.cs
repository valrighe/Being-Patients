using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{    
    private Object[] notes; // List of all notes
    private Dictionary<string, string> questionAnswer;  // Dict with couple question - right answer
    private List<string> answers;   // List of all possible answers
    private List<int> usedNotes; // List of answers already used for the game

    bool foundEqualAnswer;
    bool alreadyExisting;

    [SerializeField]
    private Image noteQuestion;
    [SerializeField]
    private Text med1Text;
    [SerializeField]
    private Text med2Text;
    [SerializeField]
    private Text med3Text;

    private int tempIndex;
    private string rightAnswerString;

    [SerializeField]
    private Text time;
    private float timeRemaining;
    private bool timeStop;
    private bool gameover;

    private int allPatientsCount;
    private int savedPatientsCount;
    private int deadPatientsCount;

    [SerializeField]
    private Text deadCountText;
    [SerializeField]
    private Text savedCountText;

    [SerializeField]
    private GameObject gameOverPanel;
    [SerializeField]
    private GameObject endGamePanel;

    [SerializeField]
    private GameObject patientSprite;
    [SerializeField]
    private GameObject deadPatientSprite;
    [SerializeField]
    private GameObject curedPatientSprite;

    [SerializeField]
    private AudioClip savedPatientClip;
    [SerializeField]
    private AudioClip deadPatientClip;
    private AudioSource audioSource;

    void Start()
    {
        foundEqualAnswer = false;
        alreadyExisting = true;

        answers = new List<string>();
        usedNotes = new List<int>();
        notes = Resources.LoadAll("Sprites/Notes", typeof(Sprite));

        Debug.Log(notes.Length);

        questionAnswer = new Dictionary<string, string>();

        tempIndex = 0;

        allPatientsCount = 0;
        savedPatientsCount = 0;
        deadPatientsCount = 0;

        timeStop = false;
        gameover = false;

        audioSource = GetComponent<AudioSource>();

        FillDictionary();
        TimeRemainingManager();
        StartCoroutine(SetGame());
    }

    void Update()
    {
        if (!gameover)
        {
            if (!timeStop)
            {
                UpdateTime();
            }
        }
    }

    private void UpdateTime()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            float minutes = Mathf.FloorToInt(timeRemaining / 60); 
            float seconds = Mathf.FloorToInt(timeRemaining % 60);
            time.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
        else 
        {
            Debug.Log("TIME'S UP!");
            timeStop = true;
            StartCoroutine(DeadPatientManager());
        }
    }

    private void SetGameOverPanel()
    {
        deadPatientSprite.SetActive(false);
        curedPatientSprite.SetActive(false);
        patientSprite.SetActive(false);

        gameOverPanel.SetActive(true);
    }

    private IEnumerator DeadPatientManager()
    {
        audioSource.clip = deadPatientClip;
        audioSource.Play();

        patientSprite.SetActive(false);
        deadPatientSprite.SetActive(true);

        deadPatientsCount++;
        allPatientsCount++;

        if (deadPatientsCount < 3)
        {
            deadCountText.text = deadPatientsCount.ToString() + "/3";
            TimeRemainingManager();

            yield return new WaitForSecondsRealtime(0.5f);

            StartCoroutine(SetGame());
        }
        else if (deadPatientsCount == 3)
        {
            gameover = true;
            SetGameOverPanel();
        }
    }

    private void TimeRemainingManager()
    {
        timeStop = false;

        if (allPatientsCount>=0 && allPatientsCount<= 5)
        {
            timeRemaining = 5;
        }
        else if (allPatientsCount>5 && allPatientsCount<=15)
        {
            timeRemaining = 4;
        }
        else if (allPatientsCount>15 && allPatientsCount<=25)
        {
            timeRemaining = 3;
        }
        else if (allPatientsCount>25 && allPatientsCount<=35)
        {
            timeRemaining = 2;
        }
        else if (allPatientsCount>35)
        {
            timeRemaining = 1.5f;
        }
    }

    // Fills all the dictionary/arrays with questions and answers
    private void FillDictionary()
    {
        foreach (Sprite note in notes)
        {
            string question = note.name;
            string answer;

            // Creates the answers
            if (question.EndsWith("1"))
            {
                answer = question.Substring(0, question.Length-1);
            }
            else
            {
                answer = question;
            }
            // Adds the couple question-answer to the Dictionary
            questionAnswer.Add(question, answer);

            // Creates the array of possible answers
            foreach (string ans in answers)
            {
                if (ans == answer)
                {
                    foundEqualAnswer = true;
                    break;
                }
            }
            if (!foundEqualAnswer)
            {
                answers.Add(answer);
            }
            else 
            {
                foundEqualAnswer = false;
            }
        }

        Debug.Log("DICTIONARY FILLED");
    }

    // Sets noteQuestion and three answers
    private IEnumerator SetGame()
    {
        timeStop = true;
        
        if (allPatientsCount == notes.Length-1)
        {
            deadPatientSprite.SetActive(false);
            curedPatientSprite.SetActive(false);
            patientSprite.SetActive(false);

            endGamePanel.SetActive(true);
        }
        else 
        {
            yield return new WaitForSecondsRealtime(1f);

            deadPatientSprite.SetActive(false);
            curedPatientSprite.SetActive(false);
            patientSprite.SetActive(true);
            
            // Sets note as a question
            while (alreadyExisting)
            {
                tempIndex = Random.Range(0, notes.Length-1);
                alreadyExisting = false;
                
                foreach (int n in usedNotes)
                {
                    if (n == tempIndex)
                    {
                        alreadyExisting = true;
                        break;
                    }
                }
            }

            usedNotes.Add(tempIndex);
            noteQuestion.GetComponent<Image>().sprite = (Sprite)notes[tempIndex];
            
            SetMedicineButtons(noteQuestion.sprite.name);

            timeStop = false;
        }

        alreadyExisting = true;
    }

    private string FindAnswerInDictionary(string noteName)
    {
        // string rightAnswer = questionAnswer[noteName];
        if (questionAnswer.TryGetValue(noteName, out string rightAnswer))
        {
            rightAnswerString = rightAnswer;
            return rightAnswer;
        }
        else 
        {
            return "";  // Should never be called
        }
    }
    
    private void SetMedicineButtons(string noteName)
    {
        int tempRightAnswer = Random.Range(1, 4);

        int tempWrongAnswer1 = Random.Range(0, answers.Count-1);
        int tempWrongAnswer2 = Random.Range(0, answers.Count-1);
        while (tempWrongAnswer1 == tempWrongAnswer2 || answers[tempWrongAnswer1] == rightAnswerString)
        {
            tempWrongAnswer1 = Random.Range(0, answers.Count-1);
            tempWrongAnswer2 = Random.Range(0, answers.Count-1);
        }

        if (tempRightAnswer == 1)
        {
            med1Text.text = FindAnswerInDictionary(noteName);
            med2Text.text = answers[tempWrongAnswer1];
            med3Text.text = answers[tempWrongAnswer2];
        }
        else if (tempRightAnswer == 2)
        {
            med1Text.text = answers[tempWrongAnswer1];
            med2Text.text = FindAnswerInDictionary(noteName);
            med3Text.text = answers[tempWrongAnswer2];
        }
        else if (tempRightAnswer == 3)
        {
            med1Text.text = answers[tempWrongAnswer1];
            med2Text.text = answers[tempWrongAnswer2];
            med3Text.text = FindAnswerInDictionary(noteName);
        }
    }

    public void SubmitAnswer()
    {
        string selectedMedicine = EventSystem.current.currentSelectedGameObject.GetComponentInChildren<Text>().text;

        if (selectedMedicine == rightAnswerString)
        {
            StartCoroutine(WaitSomeSecondsRightAnswer());
            
            allPatientsCount++;
            savedPatientsCount++;
            savedCountText.text = savedPatientsCount.ToString();
            
            TimeRemainingManager();
            StartCoroutine(SetGame());
        }
        else
        {
            StartCoroutine(DeadPatientManager());
        }
    }

    private IEnumerator WaitSomeSecondsRightAnswer()
    {
        audioSource.clip = savedPatientClip;
        audioSource.Play();
        
        patientSprite.SetActive(false);
        curedPatientSprite.SetActive(true);
        
        yield return new WaitForSecondsRealtime(0.5f);
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene("Game");
    }

    public void ExitGame()
    {
        SceneManager.LoadScene("MainMenu");
    }
}