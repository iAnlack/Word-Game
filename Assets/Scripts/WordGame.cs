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
        // Вызвать статический метод INIT() класса WordList
        WordList.INIT();
    }

    private void Update()
    {
        // Объявить пару вспомогательных переменных
        Letter letter;
        char c;

        switch (Mode)
        {
            case GameMode.InLevel:
                // Выполнить обход всех символов, введённых игроком в этом кадре
                foreach (char cIt in Input.inputString)
                {
                    // Преобразовать cIt в верхний регистр
                    c = System.Char.ToUpperInvariant(cIt);

                    // Проверить, если такая буква верхнего регистра
                    if (_upperCase.Contains(c))   // Любая буква верхнего регистра
                    {
                        // Найти доступную плитку с этой буквой в BigLetters
                        letter = FindNextLetterByChar(c);
                        // Если плитка найдена
                        if (letter != null)
                        {
                            // ... добавить этот символ в TestWord и переместить
                            // соответстующую плитку Letter в BigLetterActive
                            TestWord += c.ToString();
                            // Переместить их списка неактивных в список активных плиток
                            BigLettersActive.Add(letter);
                            BigLetters.Remove(letter);
                            letter.ColorProp = BigColorSelected; // Придать плитке активный вид
                            ArrangeBigLetters();                 // Отобразить плитки
                        }
                    }

                    if (c == '\b') // Backspace
                    {
                        // Удалить последнюю плитку Letter из BigLetterActive
                        if (BigLettersActive.Count == 0)
                        {
                            return;
                        }
                        if (TestWord.Length > 1)
                        {
                            // Удалить последнюю букву из TestWord
                            TestWord = TestWord.Substring(0, TestWord.Length - 1);
                        }
                        else
                        {
                            TestWord = "";
                        }

                        letter = BigLettersActive[BigLettersActive.Count - 1];
                        // Переместить из списка активных в список неактивных плиток
                        BigLettersActive.Remove(letter);
                        BigLetters.Add(letter);
                        letter.ColorProp = BigColorDim;   // Придать плитке неактивный вид
                        ArrangeBigLetters();              // Отобразить плитки
                    }

                    if (c == '\n' || c == '\r')   // Return/Enter для macOS/Windows
                    {
                        // Проверить наличие сконструированного слова в WordLevel
                        CheckWord();
                    }

                    if (c == ' ')   // Пробел
                    {
                        // Перемешать плитки в BigLetters
                        BigLetters = ShuffleLetters(BigLetters);
                        ArrangeBigLetters();
                    }
                }
                break;
        }
    }

    // Этот метод отыскивает плитку Letter с символом c в BigLetters
    // Если такой плитки нет - возвращает null
    private Letter FindNextLetterByChar(char c)
    {
        // Проверить каждую плитку Letter в BigLetters
        foreach (Letter letter in BigLetters)
        {
            // Если содержит тот же символ, что и указан в c
            if (letter.CharProp == c)
            {
                return letter;   // ... вернуть её
            }
        }

        return null;             // иначе вернуть null
    }

    public void CheckWord()
    {
        // Проверяет присутствие слова TestWord в списке Level.SubWords
        string subWord;
        bool foundTestWord = false;

        // Создать список List<int> для хранения индексов других слов, присутствующих в TestWord
        List<int> containedWords = new List<int>();

        // Обойти все слова в CurrLevel.SubWords
        for (int i = 0; i < CurrLevel.SubWords.Count; i++)
        {
            // Проверить, было ли уже найдено Wyrd
            if (Wyrds[i].Found)
            {
                continue;
            }

            subWord = CurrLevel.SubWords[i];
            // Проверить, входит ли это слово subWord в TestWord
            if (string.Equals(TestWord,subWord))
            {
                HighlightWyrd(i);
                ScoreManager.SCORE(Wyrds[i], 1);   // Подсчитать очки
                foundTestWord = true;
            }
            else if (TestWord.Contains(subWord))
            {
                containedWords.Add(i);
            }
        }

        if (foundTestWord)   // Если проверяемое слово присутствует в списке
        {
            // ... подсветить другие слова, содержащиеся в TestWord
            int numContained = containedWords.Count;
            int ndx;
            // Подсвечивать слова в обратном порядке
            for (int i = 0; i < containedWords.Count; i++)
            {
                ndx = numContained - i - 1;
                HighlightWyrd(containedWords[ndx]);
                ScoreManager.SCORE(Wyrds[containedWords[ndx]], i + 2);
            }

        }

        // Очистить список активных плиток Letters независимо от того, является ли TestWord допустимым
        ClearBigLettersActive();
    }

    // Подсвечивает экземпляр Wyrd
    private void HighlightWyrd(int ndx)
    {
        // Активировать слово
        Wyrds[ndx].Found = true;         // Установить признак, что оно найдено
        // Выделить цветом
        Wyrds[ndx].ColorProp = (Wyrds[ndx].ColorProp + Color.white) / 2f;
        Wyrds[ndx].VisibleProp = true;   // Сделать компонент 3D Text видимым
    }

    // Удаляет все плитки Letters из BigLettersActive
    private void ClearBigLettersActive()
    {
        TestWord = "";   // Очистить TestWord
        foreach (Letter letter in BigLettersActive)
        {
            BigLetters.Add(letter);           // Добавить каждую плитку в BigLetters
            letter.ColorProp = BigColorDim;   // Придать ей неактивный вид
        }

        BigLettersActive.Clear();             // Очистить список
        ArrangeBigLetters();                  // Повторно вывести плитки на экран
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
        Layout();   // Вызвать Layout() один раз после выполнения WordSearch
    }

    private void Layout()
    {
        // Поместить на экран плитки с буквами каждого возможного слова текущего уровня
        Wyrds = new List<Wyrd>();

        // Объявить локальные переменные, которые будут использоваться методом
        GameObject gameObject;
        Letter letter;
        string word;
        Vector3 position;
        float left = 0;
        float columnWidth = 3;
        char c;
        Color color;
        Wyrd wyrd;

        // Определить, сколько рядов плиток уместится на экране
        int numRows = Mathf.RoundToInt(WordArea.height / LetterSize);

        // Создать экземпляр Wyrd для каждого слова в Level.SubWords
        for (int i = 0; i < CurrLevel.SubWords.Count; i++)
        {
            wyrd = new Wyrd();
            word = CurrLevel.SubWords[i];

            // Если слово длинее, чем columnWidth, развернуть его
            columnWidth = Mathf.Max(columnWidth, word.Length);

            // Создать экземпляр PrefabLetter Для каждой буквы в слове
            for (int j = 0; j < word.Length; j++)
            {
                c = word[j];                                       // Получить j-й символ слова
                gameObject = Instantiate<GameObject>(PrefabLetter);
                gameObject.transform.SetParent(_letterAnchor);
                letter = gameObject.GetComponent<Letter>();
                letter.CharProp = c;                               // Назначить букву плитке Letter

                // Установить координаты плитки Letter
                position = new Vector3(WordArea.x + left + j * LetterSize, WordArea.y, 0);

                // Оператор % помогает выстроить плитки по вертикали
                position.y -= (i % numRows) * LetterSize;

                // Переместить плтику letter немедленно за верхний край экрана
                letter.PosImmediate = position + Vector3.up * (20 + i % numRows);
                // Затем начать её перемещение в новую позицию position
                letter.PositionProp = position; // вокруг образовался новый код
                // Увеличить letter.TimeStart для перемещения слов в разные времена
                letter.TimeStart = Time.time + i * 0.05f;

                gameObject.transform.localScale = Vector3.one * LetterSize;
                wyrd.Add(letter);
            }

            if (ShowAllWyrds)
            {
                wyrd.VisibleProp = true;
            }

            // Определить цвет слова исходя из его длины
            wyrd.ColorProp = WyrdPalette[word.Length - WordList.WORD_LENGTH_MIN];
            Wyrds.Add(wyrd);

            // Если достинут последний ряд в столбце, начать новый столбец
            if (i % numRows == numRows - 1)
            {
                left += (columnWidth + 0.5f) * LetterSize;
            }
        }

        // Поместить на экран большие плитки с буквами
        // Инициализировать список больших букв
        BigLetters = new List<Letter>();
        BigLettersActive = new List<Letter>();

        // Создать большую плитку для каждой буквы в целевом слове
        for (int i = 0; i < CurrLevel.Word.Length; i++)
        {
            // Напоминает процедуру создания маленьких плиток
            c = CurrLevel.Word[i];
            gameObject = Instantiate<GameObject>(PrefabLetter);
            gameObject.transform.SetParent(_bigLetterAnchor);
            letter = gameObject.GetComponent<Letter>();
            letter.CharProp = c;
            gameObject.transform.localScale = Vector3.one * BigLetterSize;

            // Первоначально поместить большие плитки ниже края экрана
            position = new Vector3(0, -100, 0);

            letter.PosImmediate = position;
            letter.PositionProp = position; // вокруг образовался новый код
            // Увеличить letter.TimeStart, чтобы большие плитки с буквами появились последними
            letter.TimeStart = Time.time + CurrLevel.SubWords.Count * 0.05f;
            letter.EasingCuve = Easing.Sin+"-0.18";   // Bouncy easing

            color = BigColorDim;
            letter.ColorProp = color;
            letter.VisibleProp = true; // Всегда true для больших плиток
            letter.Big = true;
            BigLetters.Add(letter);
        }

        // Перемешать плитки
        BigLetters = ShuffleLetters(BigLetters);

        // Вывести на экран
        ArrangeBigLetters();

        // Установить режим Mode -- "в игре"
        Mode = GameMode.InLevel;
    }

    // Этот метод перемешивает элементы в списке List<Letter> и возвращает результат
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

    // Этот метод выводит большие плитки на экран
    private void ArrangeBigLetters()
    {
        // Найди середину для вывода ряда больших плиток с центрированием по горизонтали
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
