using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordList : MonoBehaviour
{
    private static WordList S;

    [Header("Set in Inspector")]
    public TextAsset WordListText;
    public int NumToParseBeforeYield = 10000;
    public int WordLengthMin = 3;
    public int WordLengthMax = 7;

    [Header("Set Dynamically")]
    public int CurrLine = 0;
    public int TotalLines;
    public int LongWordCount;
    public int WordCount;

    private string[] _lines;
    private List<string> _longWords;
    private List<string> _words;

    private void Awake()
    {
        S = this;   // ���������� �������-�������� WordList
    }

    public void Init()
    {
        _lines = WordListText.text.Split('\n');
        TotalLines = _lines.Length;

        StartCoroutine(ParseLines());
    }

    static public void INIT()
    {
        S.Init();
    }

    // ��� ����������� ���������� �������� ���� IEnumerator
    public IEnumerator ParseLines()
    {
        string word;
        // ���������������� ������ ��� �������� ���������� ���� �� ����� ����������
        _longWords = new List<string>();
        _words = new List<string>();

        for (int currLine = 0; currLine < TotalLines; currLine++)
        {
            CurrLine = currLine;
            word = _lines[currLine];

            // ���� ����� ����� ����� WordLengthMax...
            if (word.Length == WordLengthMax)
            {
                _longWords.Add(word);   // ... ��������� ��� � LongWords
            }

            // ���� ����� ����� ����� WordLengthMin � WordLengthMax...
            if (word.Length >= WordLengthMin && word.Length <= WordLengthMax)
            {
                _words.Add(word);   // ... �������� ��� � ������ ���������� ����
            }

            // ����������, �� ���� �� ������� �������
            if (currLine % NumToParseBeforeYield == 0)
            {
                // ���������� ����� � ������ ������, ����� ��������, ��� ��������� ������� �������
                LongWordCount = _longWords.Count;
                WordCount = _words.Count;

                // ������������� ���������� ����������� �� ���������� �����
                yield return null;

                // ���������� yield ������������ ���������� ����� ������, ���� �����������
                // ����������� ������� ���� � ���������� ���������� ����������� � ���� �����,
                // ����� ��������� �������� ����� for
            }
        }

        LongWordCount = _longWords.Count;
        WordCount = _words.Count;

        // ������� �������� ������� gameObject ��������� �� ��������� �������
        gameObject.SendMessage("WordListParseComplete");
    }

    // ��� ������ ��������� ������ ������� ���������� � ������� ����� List<string>
    
    static public List<string> GET_WORDS()
    {
        return S._words;
    }

    static public string GET_WORD(int ndx)
    {
        return S._words[ndx];
    }

    static public List<string> GET_LONG_WORDS()
    {
        return S._longWords;
    }

    static public string GET_LONG_WORD(int ndx)
    {
        return S._longWords[ndx];
    }

    static public int WORD_COUNT
    {
        get { return S.WordCount; }
    }

    static public int LONG_WORD_COUNT
    {
        get { return S.LongWordCount; }
    }

    static public int NUM_TO_PARSE_BEFORE_YIELD
    {
        get { return S.NumToParseBeforeYield; }
    }

    static public int WORD_LENGTH_MIN
    {
        get { return S.WordLengthMin; }
    }

    static public int WORD_LENGTH_MAX
    {
        get { return S.WordLengthMax; }
    }
}
