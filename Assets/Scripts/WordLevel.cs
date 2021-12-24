using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]   // ���������� WordLevel ����� �������������/�������� � ����������
public class WordLevel
{
    public int LevelNum;
    public int LongWordIndex;
    public string Word;
    public Dictionary<char, int> CharDict;   // ������� �� ����� ������� � �������
    public List<string> SubWords;   // ��� �����, ������� ����� ��������� �� ���� � CharDict

    // ����������� �������, ������������ ���������� ��������� �������� � ������
    // � ���������� ������� Dictionary<char, int> � ���� �����������
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

    // ����������� �����, ��������� ����������� ��������� ����� �� �������� � Level.CharDict
    public static bool CheckWordInLevel(string str, WordLevel level)
    {
        Dictionary<char, int> counts = new Dictionary<char, int>();
        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];
            // ���� CharDict �������� ������ c
            if (level.CharDict.ContainsKey(c))
            {
                // ���� counts ��� �� �������� ����� � �������� c
                if (!counts.ContainsKey(c))
                {
                    // ... �������� ����� ���� �� ��������� 1
                    counts.Add(c, 1);
                }
                else
                {
                    // � ��������� ������ ��������� 1 � �������� ��������
                    counts[c]++;
                }

                // ���� ����� ��������� ������� c � ������ str ��������� ��������� ���������� � Level.CharDict
                if (counts[c] > level.CharDict[c])
                {
                    // ... ������� false
                    return false;
                }
            }
            else
            {
                // ������ c ����������� � level.word, ������� false
                return false;
            }
        }

        return true;
    }
}
