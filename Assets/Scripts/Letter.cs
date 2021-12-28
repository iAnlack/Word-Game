using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Letter : MonoBehaviour
{
    [Header("Set in Inspector")]
    public float TimeDuration = 0.5f;
    public string EasingCuve = Easing.InOut;   // Функция сглаживания из Utils.cs

    [Header("Set Dynamically")]
    public TextMesh TextMesh;       // TextMesh отображает символ
    public Renderer TextRenderer;   // Компонент Renderer объекта 3D Text. Определяет видимость символа
    public bool Big = false;        // Большие и малые плитки действуют по-разному

    // Поля для линейной интерполяции
    public List<Vector3> Points = null;
    public float TimeStart = -1;

    private char _char;             // Символ, отображаемый на этой плитке
    private Renderer _renderer;

    private void Awake()
    {
        TextMesh = GetComponentInChildren<TextMesh>();
        TextRenderer = TextMesh.GetComponent<Renderer>();
        _renderer = GetComponent<Renderer>();
        VisibleProp = false;
    }

    // Код, реализующий анимационный эффект
    private void Update()
    {
        if (TimeStart == -1)
        {
            return;
        }

        // Стандартная линейная интерполяция
        float u = (Time.time - TimeStart) / TimeDuration;
        u = Mathf.Clamp01(u);
        float u1 = Easing.Ease(u, EasingCuve);
        Vector3 v = Utils.Bezier(u1, Points);
        transform.position = v;

        // Если интерполяция закончена, записать -1 в TimeStart
        if (u == 1)
        {
            TimeStart = -1;
        }
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
    // Теперь настраивает кривую Безье для плавного перемещения в новые координаты
    public Vector3 PositionProp
    {
        set
        {
            //transform.position = value;

            // Найти среднюю точку на случайном расстоянии от фактической средней точки
            // между текущей и новой (value) позициями

            Vector3 mid = (transform.position + value) / 2f;

            // Случайное расстояние не превышает 1/4 расстояния до фактической средней точки
            float mag = (transform.position - value).magnitude;
            mid += Random.insideUnitSphere * mag * 0.25f;

            // Создать List<Vector3> точек, определяющих кривую Безье
            Points = new List<Vector3>() { transform.position, mid, value };

            // Если TimeStart содержит значение по умолчанию -1, устновить текущее время
            if (TimeStart == -1)
            {
                TimeStart = Time.time;
            }
        }
    }

    // Немедленно перемещает в новую позицию
    public Vector3 PosImmediate
    {
        set
        {
            transform.position = value;
        }
    }
}
