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

    [Header("Set Dynamically")]
    public GameMode Mode = GameMode.PreGame;
    public WordLevel CurrLevel;
    public List<Wyrd> Wyrds;
    public List<Letter> BigLetters;
    public List<Letter> BigLettersActive;

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

                letter.PositionProp = position; // ...

                gameObject.transform.localScale = Vector3.one * LetterSize;
                wyrd.Add(letter);
            }

            if (ShowAllWyrds)
            {
                wyrd.VisibleProp = true;
            }

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
            letter.PositionProp = position; // ...

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
