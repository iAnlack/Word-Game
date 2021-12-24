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

    [Header("Set Dynamically")]
    public GameMode Mode = GameMode.PreGame;
    public WordLevel CurrLevel;

    private void Awake()
    {
        S = this;
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
    }
}
