using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum GameMode
{
    PreGame,     // Перед началом игры
    Loadind,     // Список слов загружается и анализируется
    MakeLevel,   // Создаётся отдельный WordLevel
    LevelPrep,   // Создаётся уровень с визуальным представлением
    InLevel      // Уровень запущен
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
        // Вызвать статический метод INIT() класса WordList
        WordList.INIT();
    }

    // Вызывается методом SendMessage() из WordList
    public void WordListParseComplete()
    {
        Mode = GameMode.MakeLevel;
        // Создать уровень и сохранить в currLevel текущий WordLevel
        CurrLevel = MakeWordLevel();
    }

    public WordLevel MakeWordLevel(int levelNum = -1)
    {
        WordLevel level = new WordLevel();
        if (levelNum == -1)
        {
            // Выбрать случайный уровень
            level.LongWordIndex = Random.Range(0, WordList.LONG_WORD_COUNT);
        }
        else
        {
            // позднее...
        }

        level.LevelNum = levelNum;
        level.Word = WordList.GET_LONG_WORD(level.LongWordIndex);
        level.CharDict = WordLevel.MakeCharDict(level.Word);

        StartCoroutine(FindSubWordsCoroutine(level));
        return level;
    }

    // Сопрограмма, отыскивающая слова, которые можно составить на этом уровне
    public IEnumerator FindSubWordsCoroutine(WordLevel level)
    {
        level.SubWords = new List<string>();
        string str;

        List<string> words = WordList.GET_WORDS();

        //Выполнить обход всех слов в WordList
        for (int i = 0; i < WordList.WORD_COUNT; i++)
        {
            str = words[i];
            // Проверить, можно ли его составить из символов в level.CharDict
            if (WordLevel.CheckWordInLevel(str, level))
            {
                level.SubWords.Add(str);
            }

            // Приостановиться после анализа заданного числа слов в этом кадре
            if (i % WordList.NUM_TO_PARSE_BEFORE_YIELD == 0)
            {
                // Приостановиться до следующего кадра
                yield return null;
            }
        }

        level.SubWords.Sort();
        level.SubWords = SortWordsByLength(level.SubWords).ToList();

        // Сопрограмма завершила анализ, поэтому вызываем SubWordSearchComplete()
        SubWordSearchComplete();
    }

    // Использует LINQ для сортировки массива и возвращает его копию
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
