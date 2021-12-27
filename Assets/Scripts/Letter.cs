using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Letter : MonoBehaviour
{
    [Header("Set Dynamically")]
    public TextMesh TextMesh;       // TextMesh ���������� ������
    public Renderer TextRenderer;   // ��������� Renderer ������� 3D Text. ���������� ��������� �������
    public bool Big = false;        // ������� � ����� ������ ��������� ��-�������

    private char _char;             // ������, ������������ �� ���� ������
    private Renderer _renderer;

    private void Awake()
    {
        TextMesh = GetComponentInChildren<TextMesh>();
        TextRenderer = TextMesh.GetComponent<Renderer>();
        _renderer = GetComponent<Renderer>();
        VisibleProp = false;
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
    public Vector3 PositionProp
    {
        set
        {
            transform.position = value;
            // ...
        }
    }
}
