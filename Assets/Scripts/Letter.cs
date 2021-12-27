using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Letter : MonoBehaviour
{
    [Header("Set Dynamically")]
    public TextMesh TextMesh;       // TextMesh отображает символ
    public Renderer TextRenderer;   // Компонент Renderer объекта 3D Text. Определяет видимость символа
    public bool Big = false;        // Большие и малые плитки действуют по-разному

    private char _char;             // Символ, отображаемый на этой плитке
    private Renderer _renderer;

    private void Awake()
    {
        TextMesh = GetComponentInChildren<TextMesh>();
        TextRenderer = TextMesh.GetComponent<Renderer>();
        _renderer = GetComponent<Renderer>();
        VisibleProp = false;
    }

    // Свойство для чтения/записи буквы в поле _char, отображаемой объектом 3D Text
    public char CharProp
    {
        get { return _char; }
        set
        {
            _char = value;
            TextMesh.text = _char.ToString();
        }
    }

    // Свойство для чтения/записи буквы в поле _char в виде строки
    public string StringProp
    {
        get { return _char.ToString(); }
        set { CharProp = value[0]; }
    }

    // Разрешает или запрещает отображение 3D Text, что делает букву видимой или невидимой соответственно
    public bool VisibleProp
    {
        get { return TextRenderer.enabled; }
        set { TextRenderer.enabled = value; }
    }

    // Свойство для чтения/записи цвета плитки
    public Color ColorProp
    {
        get { return _renderer.material.color; }
        set { _renderer.material.color = value; }
    }

    // Свойство для чтения/записи координат плитки
    public Vector3 PositionProp
    {
        set
        {
            transform.position = value;
            // ...
        }
    }
}
