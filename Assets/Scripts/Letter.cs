using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Letter : MonoBehaviour
{
    [Header("Set in Inspector")]
    public float TimeDuration = 0.5f;
    public string EasingCuve = Easing.InOut;   // ������� ����������� �� Utils.cs

    [Header("Set Dynamically")]
    public TextMesh TextMesh;       // TextMesh ���������� ������
    public Renderer TextRenderer;   // ��������� Renderer ������� 3D Text. ���������� ��������� �������
    public bool Big = false;        // ������� � ����� ������ ��������� ��-�������

    // ���� ��� �������� ������������
    public List<Vector3> Points = null;
    public float TimeStart = -1;

    private char _char;             // ������, ������������ �� ���� ������
    private Renderer _renderer;

    private void Awake()
    {
        TextMesh = GetComponentInChildren<TextMesh>();
        TextRenderer = TextMesh.GetComponent<Renderer>();
        _renderer = GetComponent<Renderer>();
        VisibleProp = false;
    }

    // ���, ����������� ������������ ������
    private void Update()
    {
        if (TimeStart == -1)
        {
            return;
        }

        // ����������� �������� ������������
        float u = (Time.time - TimeStart) / TimeDuration;
        u = Mathf.Clamp01(u);
        float u1 = Easing.Ease(u, EasingCuve);
        Vector3 v = Utils.Bezier(u1, Points);
        transform.position = v;

        // ���� ������������ ���������, �������� -1 � TimeStart
        if (u == 1)
        {
            TimeStart = -1;
        }
    }

    // �������� ��� ������/������ ����� � ���� _char, ������������ �������� 3D Text
    public char CharProp
    {
        get { return _char; }
        set
        {
            _char = value;
            TextMesh.text = _char.ToString();
        }
    }

    // �������� ��� ������/������ ����� � ���� _char � ���� ������
    public string StringProp
    {
        get { return _char.ToString(); }
        set { CharProp = value[0]; }
    }

    // ��������� ��� ��������� ����������� 3D Text, ��� ������ ����� ������� ��� ��������� ��������������
    public bool VisibleProp
    {
        get { return TextRenderer.enabled; }
        set { TextRenderer.enabled = value; }
    }

    // �������� ��� ������/������ ����� ������
    public Color ColorProp
    {
        get { return _renderer.material.color; }
        set { _renderer.material.color = value; }
    }

    // �������� ��� ������/������ ��������� ������
    // ������ ����������� ������ ����� ��� �������� ����������� � ����� ����������
    public Vector3 PositionProp
    {
        set
        {
            //transform.position = value;

            // ����� ������� ����� �� ��������� ���������� �� ����������� ������� �����
            // ����� ������� � ����� (value) ���������

            Vector3 mid = (transform.position + value) / 2f;

            // ��������� ���������� �� ��������� 1/4 ���������� �� ����������� ������� �����
            float mag = (transform.position - value).magnitude;
            mid += Random.insideUnitSphere * mag * 0.25f;

            // ������� List<Vector3> �����, ������������ ������ �����
            Points = new List<Vector3>() { transform.position, mid, value };

            // ���� TimeStart �������� �������� �� ��������� -1, ��������� ������� �����
            if (TimeStart == -1)
            {
                TimeStart = Time.time;
            }
        }
    }

    // ���������� ���������� � ����� �������
    public Vector3 PosImmediate
    {
        set
        {
            transform.position = value;
        }
    }
}
