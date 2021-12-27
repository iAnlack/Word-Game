using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wyrd
{
    public string String; // ��������� ������������� �����
    public List<Letter> Letters = new List<Letter>();
    public bool Found = false; // ������� false, ���� ����� ����� ��� �����

    // ��������, ����������� ���������� ���������� 3D Text ������ ������ Letter
    public bool VisibleProp
    {
        get
        {
            if (Letters.Count == 0)
            {
                return false;
            }

            return (Letters[0].VisibleProp);
        }
        set
        {
            foreach (Letter letter in Letters)
            {
                letter.VisibleProp = value;
            }
        }
    }

    // �������� ��� ���������� ����� ������ ������ Letter
    public Color ColorProp
    {
        get
        {
            if (Letters.Count == 0)
            {
                return (Color.black);
            }

            return (Letters[0].ColorProp);
        }
        set
        {
            foreach (Letter letter in Letters)
            {
                letter.ColorProp = value;
            }
        }
    }

    // ��������� ������ � ������ Letters
    public void Add(Letter letter)
    {
        Letters.Add(letter);
        String += letter.CharProp.ToString();
    }
}
