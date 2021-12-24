using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]   // Экземпляры WordLevel можно просматривать/изменять в инспекторе
public class WordLevel
{
    public int LevelNum;
    public int LongWordIndex;
    public string Word;
    public Dictionary<char, int> CharDict;   // Словарь со всеми буквами в словаре
    public List<string> SubWords;   // Все слова, которые можно составить из букв в CharDict

    // Статическая функция, подсчитывает количество вхождений символов в строку
    // и возвращает словарь Dictionary<char, int> с этой информацией
    static public Dictionary<char, int> MakeCharDict(string w)
    {
        Dictionary<char, int> dictionary = new Dictionary<char, int>();
        char c;
        for (int i = 0; i < w.Length; i++)
        {
            c = w[i];
            if (dictionary.ContainsKey(c))
            {
                dictionary[c]++;
            }
            else
            {
                dictionary.Add(c, 1);
            }
        }

        return dictionary;
    }

    // Статический метод, проверяет возможность составить слово из символов в Level.CharDict
    public static bool CheckWordInLevel(string str, WordLevel level)
    {
        Dictionary<char, int> counts = new Dictionary<char, int>();
        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];
            // Если CharDict содержит символ c
            if (level.CharDict.ContainsKey(c))
            {
                // Если counts ещё не содержит ключа с символом c
                if (!counts.ContainsKey(c))
                {
                    // ... добавить новый ключ со значением 1
                    counts.Add(c, 1);
                }
                else
                {
                    // В противном случае приьавить 1 к текущему значению
                    counts[c]++;
                }

                // Если число вхождений символа c в строку str превысило доступное количество в Level.CharDict
                if (counts[c] > level.CharDict[c])
                {
                    // ... вернуть false
                    return false;
                }
            }
            else
            {
                // Символ c отсутствует в level.word, вернуть false
                return false;
            }
        }

        return true;
    }
}
