using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum GameMode
{
    PreGame,     // ����� ������� ����
    Loadind,     // ������ ���� ����������� � �������������
    MakeLevel,   // �������� ��������� WordLevel
    LevelPrep,   // �������� ������� � ���������� ��������������
    InLevel      // ������� �������
}

public class WordGame : MonoBehaviour
{
    static public WordGame S;

    [Header("Set in Inspector")]
    public GameObject PrefabLetter;
    public Rect WordArea = new Rect(-24, 19, 48, 28);
    public float LetterSize = 1.5f;
    public bool ShowAllWyrds = true;
    public float BigLetterSize = 4f;
    public Color BigColorDim = new Color(0.8f, 0.8f, 0.8f);
    public Color BigColorSelected = new Color(1f, 0.9f, 0.7f);
    public Vector3 BigLetterCenter = new Vector3(0, -16, 0);
    public Color[] WyrdPalette;

    [Header("Set Dynamically")]
    public GameMode Mode = GameMode.PreGame;
    public WordLevel CurrLevel;
    public List<Wyrd> Wyrds;
    public List<Letter> BigLetters;
    public List<Letter> BigLettersActive;
    public string TestWord;

    private string _upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private Transform _letterAnchor;
    private Transform _bigLetterAnchor;

    private void Awake()
    {
        S = this;
        _letterAnchor = new GameObject("LetterAnchor").transform;
        _bigLetterAnchor = new GameObject("BigLetterAnchor").transform;
    }

    private void Start()
    {
        Mode = GameMode.Loadind;
        // ������� ����������� ����� INIT() ������ WordList
        WordList.INIT();
    }

    private void Update()
    {
        // �������� ���� ��������������� ����������
        Letter letter;
        char c;

        switch (Mode)
        {
            case GameMode.InLevel:
                // ��������� ����� ���� ��������, �������� ������� � ���� �����
                foreach (char cIt in Input.inputString)
                {
                    // ������������� cIt � ������� �������
                    c = System.Char.ToUpperInvariant(cIt);

                    // ���������, ���� ����� ����� �������� ��������
                    if (_upperCase.Contains(c))   // ����� ����� �������� ��������
                    {
                        // ����� ��������� ������ � ���� ������ � BigLetters
                        letter = FindNextLetterByChar(c);
                        // ���� ������ �������
                        if (letter != null)
                        {
                            // ... �������� ���� ������ � TestWord � �����������
                            // �������������� ������ Letter � BigLetterActive
                            TestWord += c.ToString();
                            // ����������� �� ������ ���������� � ������ �������� ������
                            BigLettersActive.Add(letter);
                            BigLetters.Remove(letter);
                            letter.ColorProp = BigColorSelected; // ������� ������ �������� ���
                            ArrangeBigLetters();                 // ���������� ������
                        }
                    }

                    if (c == '\b') // Backspace
                    {
                        // ������� ��������� ������ Letter �� BigLetterActive
                        if (BigLettersActive.Count == 0)
                        {
                            return;
                        }
                        if (TestWord.Length > 1)
                        {
                            // ������� ��������� ����� �� TestWord
                            TestWord = TestWord.Substring(0, TestWord.Length - 1);
                        }
                        else
                        {
                            TestWord = "";
                        }

                        letter = BigLettersActive[BigLettersActive.Count - 1];
                        // ����������� �� ������ �������� � ������ ���������� ������
                        BigLettersActive.Remove(letter);
                        BigLetters.Add(letter);
                        letter.ColorProp = BigColorDim;   // ������� ������ ���������� ���
                        ArrangeBigLetters();              // ���������� ������
                    }

                    if (c == '\n' || c == '\r')   // Return/Enter ��� macOS/Windows
                    {
                        // ��������� ������� ������������������ ����� � WordLevel
                        CheckWord();
                    }

                    if (c == ' ')   // ������
                    {
                        // ���������� ������ � BigLetters
                        BigLetters = ShuffleLetters(BigLetters);
                        ArrangeBigLetters();
                    }
                }
                break;
        }
    }

    // ���� ����� ���������� ������ Letter � �������� c � BigLetters
    // ���� ����� ������ ��� - ���������� null
    private Letter FindNextLetterByChar(char c)
    {
        // ��������� ������ ������ Letter � BigLetters
        foreach (Letter letter in BigLetters)
        {
            // ���� �������� ��� �� ������, ��� � ������ � c
            if (letter.CharProp == c)
            {
                return letter;   // ... ������� �
            }
        }

        return null;             // ����� ������� null
    }

    public void CheckWord()
    {
        // ��������� ����������� ����� TestWord � ������ Level.SubWords
        string subWord;
        bool foundTestWord = false;

        // ������� ������ List<int> ��� �������� �������� ������ ����, �������������� � TestWord
        List<int> containedWords = new List<int>();

        // ������ ��� ����� � CurrLevel.SubWords
        for (int i = 0; i < CurrLevel.SubWords.Count; i++)
        {
            // ���������, ���� �� ��� ������� Wyrd
            if (Wyrds[i].Found)
            {
                continue;
            }

            subWord = CurrLevel.SubWords[i];
            // ���������, ������ �� ��� ����� subWord � TestWord
            if (string.Equals(TestWord,subWord))
            {
                HighlightWyrd(i);
                ScoreManager.SCORE(Wyrds[i], 1);   // ���������� ����
                foundTestWord = true;
            }
            else if (TestWord.Contains(subWord))
            {
                containedWords.Add(i);
            }
        }

        if (foundTestWord)   // ���� ����������� ����� ������������ � ������
        {
            // ... ���������� ������ �����, ������������ � TestWord
            int numContained = containedWords.Count;
            int ndx;
            // ������������ ����� � �������� �������
            for (int i = 0; i < containedWords.Count; i++)
            {
                ndx = numContained - i - 1;
                HighlightWyrd(containedWords[ndx]);
                ScoreManager.SCORE(Wyrds[containedWords[ndx]], i + 2);
            }

        }

        // �������� ������ �������� ������ Letters ���������� �� ����, �������� �� TestWord ����������
        ClearBigLettersActive();
    }

    // ������������ ��������� Wyrd
    private void HighlightWyrd(int ndx)
    {
        // ������������ �����
        Wyrds[ndx].Found = true;         // ���������� �������, ��� ��� �������
        // �������� ������
        Wyrds[ndx].ColorProp = (Wyrds[ndx].ColorProp + Color.white) / 2f;
        Wyrds[ndx].VisibleProp = true;   // ������� ��������� 3D Text �������
    }

    // ������� ��� ������ Letters �� BigLettersActive
    private void ClearBigLettersActive()
    {
        TestWord = "";   // �������� TestWord
        foreach (Letter letter in BigLettersActive)
        {
            BigLetters.Add(letter);           // �������� ������ ������ � BigLetters
            letter.ColorProp = BigColorDim;   // ������� �� ���������� ���
        }

        BigLettersActive.Clear();             // �������� ������
        ArrangeBigLetters();                  // �������� ������� ������ �� �����
    }

    // ���������� ������� SendMessage() �� WordList
    public void WordListParseComplete()
    {
        Mode = GameMode.MakeLevel;
        // ������� ������� � ��������� � currLevel ������� WordLevel
        CurrLevel = MakeWordLevel();
    }

    public WordLevel MakeWordLevel(int levelNum = -1)
    {
        WordLevel level = new WordLevel();
        if (levelNum == -1)
        {
            // ������� ��������� �������
            level.LongWordIndex = Random.Range(0, WordList.LONG_WORD_COUNT);
        }
        else
        {
            // �������...
        }

        level.LevelNum = levelNum;
        level.Word = WordList.GET_LONG_WORD(level.LongWordIndex);
        level.CharDict = WordLevel.MakeCharDict(level.Word);

        StartCoroutine(FindSubWordsCoroutine(level));
        return level;
    }

    // �����������, ������������ �����, ������� ����� ��������� �� ���� ������
    public IEnumerator FindSubWordsCoroutine(WordLevel level)
    {
        level.SubWords = new List<string>();
        string str;

        List<string> words = WordList.GET_WORDS();

        //��������� ����� ���� ���� � WordList
        for (int i = 0; i < WordList.WORD_COUNT; i++)
        {
            str = words[i];
            // ���������, ����� �� ��� ��������� �� �������� � level.CharDict
            if (WordLevel.CheckWordInLevel(str, level))
            {
                level.SubWords.Add(str);
            }

            // ��������������� ����� ������� ��������� ����� ���� � ���� �����
            if (i % WordList.NUM_TO_PARSE_BEFORE_YIELD == 0)
            {
                // ��������������� �� ���������� �����
                yield return null;
            }
        }

        level.SubWords.Sort();
        level.SubWords = SortWordsByLength(level.SubWords).ToList();

        // ����������� ��������� ������, ������� �������� SubWordSearchComplete()
        SubWordSearchComplete();
    }

    // ���������� LINQ ��� ���������� ������� � ���������� ��� �����
    public static IEnumerable<string> SortWordsByLength(IEnumerable<string> ws)
    {
        ws = ws.OrderBy(s => s.Length);
        return ws;
    }

    public void SubWordSearchComplete()
    {
        Mode = GameMode.LevelPrep;
        Layout();   // ������� Layout() ���� ��� ����� ���������� WordSearch
    }

    private void Layout()
    {
        // ��������� �� ����� ������ � ������� ������� ���������� ����� �������� ������
        Wyrds = new List<Wyrd>();

        // �������� ��������� ����������, ������� ����� �������������� �������
        GameObject gameObject;
        Letter letter;
        string word;
        Vector3 position;
        float left = 0;
        float columnWidth = 3;
        char c;
        Color color;
        Wyrd wyrd;

        // ����������, ������� ����� ������ ��������� �� ������
        int numRows = Mathf.RoundToInt(WordArea.height / LetterSize);

        // ������� ��������� Wyrd ��� ������� ����� � Level.SubWords
        for (int i = 0; i < CurrLevel.SubWords.Count; i++)
        {
            wyrd = new Wyrd();
            word = CurrLevel.SubWords[i];

            // ���� ����� ������, ��� columnWidth, ���������� ���
            columnWidth = Mathf.Max(columnWidth, word.Length);

            // ������� ��������� PrefabLetter ��� ������ ����� � �����
            for (int j = 0; j < word.Length; j++)
            {
                c = word[j];                                       // �������� j-� ������ �����
                gameObject = Instantiate<GameObject>(PrefabLetter);
                gameObject.transform.SetParent(_letterAnchor);
                letter = gameObject.GetComponent<Letter>();
                letter.CharProp = c;                               // ��������� ����� ������ Letter

                // ���������� ���������� ������ Letter
                position = new Vector3(WordArea.x + left + j * LetterSize, WordArea.y, 0);

                // �������� % �������� ��������� ������ �� ���������
                position.y -= (i % numRows) * LetterSize;

                // ����������� ������ letter ���������� �� ������� ���� ������
                letter.PosImmediate = position + Vector3.up * (20 + i % numRows);
                // ����� ������ � ����������� � ����� ������� position
                letter.PositionProp = position; // ������ ����������� ����� ���
                // ��������� letter.TimeStart ��� ����������� ���� � ������ �������
                letter.TimeStart = Time.time + i * 0.05f;

                gameObject.transform.localScale = Vector3.one * LetterSize;
                wyrd.Add(letter);
            }

            if (ShowAllWyrds)
            {
                wyrd.VisibleProp = true;
            }

            // ���������� ���� ����� ������ �� ��� �����
            wyrd.ColorProp = WyrdPalette[word.Length - WordList.WORD_LENGTH_MIN];
            Wyrds.Add(wyrd);

            // ���� �������� ��������� ��� � �������, ������ ����� �������
            if (i % numRows == numRows - 1)
            {
                left += (columnWidth + 0.5f) * LetterSize;
            }
        }

        // ��������� �� ����� ������� ������ � �������
        // ���������������� ������ ������� ����
        BigLetters = new List<Letter>();
        BigLettersActive = new List<Letter>();

        // ������� ������� ������ ��� ������ ����� � ������� �����
        for (int i = 0; i < CurrLevel.Word.Length; i++)
        {
            // ���������� ��������� �������� ��������� ������
            c = CurrLevel.Word[i];
            gameObject = Instantiate<GameObject>(PrefabLetter);
            gameObject.transform.SetParent(_bigLetterAnchor);
            letter = gameObject.GetComponent<Letter>();
            letter.CharProp = c;
            gameObject.transform.localScale = Vector3.one * BigLetterSize;

            // ������������� ��������� ������� ������ ���� ���� ������
            position = new Vector3(0, -100, 0);

            letter.PosImmediate = position;
            letter.PositionProp = position; // ������ ����������� ����� ���
            // ��������� letter.TimeStart, ����� ������� ������ � ������� ��������� ����������
            letter.TimeStart = Time.time + CurrLevel.SubWords.Count * 0.05f;
            letter.EasingCuve = Easing.Sin+"-0.18";   // Bouncy easing

            color = BigColorDim;
            letter.ColorProp = color;
            letter.VisibleProp = true; // ������ true ��� ������� ������
            letter.Big = true;
            BigLetters.Add(letter);
        }

        // ���������� ������
        BigLetters = ShuffleLetters(BigLetters);

        // ������� �� �����
        ArrangeBigLetters();

        // ���������� ����� Mode -- "� ����"
        Mode = GameMode.InLevel;
    }

    // ���� ����� ������������ �������� � ������ List<Letter> � ���������� ���������
    private List<Letter> ShuffleLetters(List<Letter> letters)
    {
        List<Letter> newLetters = new List<Letter>();
        int ndx;
        while (letters.Count > 0)
        {
            ndx = Random.Range(0, letters.Count);
            newLetters.Add(letters[ndx]);
            letters.RemoveAt(ndx);
        }

        return newLetters;
    }

    // ���� ����� ������� ������� ������ �� �����
    private void ArrangeBigLetters()
    {
        // ����� �������� ��� ������ ���� ������� ������ � �������������� �� �����������
        float halfWidth = ((float)BigLetters.Count) / 2f - 0.5f;
        Vector3 position;
        for (int i = 0; i < BigLetters.Count; i++)
        {
            position = BigLetterCenter;
            position.x += (i - halfWidth) * BigLetterSize;
            BigLetters[i].PositionProp = position;
        }

        // BigLettersActive
        halfWidth = ((float)BigLettersActive.Count) / 2f - 0.5f;
        for (int i = 0; i < BigLettersActive.Count; i++)
        {
            position = BigLetterCenter;
            position.x += (i - halfWidth) * BigLetterSize;
            position.y += BigLetterSize * 1.25f;
            BigLettersActive[i].PositionProp = position;
        }
    }
}
