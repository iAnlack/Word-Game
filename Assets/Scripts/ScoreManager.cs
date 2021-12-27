using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    static private ScoreManager _S;   // ��� ���� ������� ������-��������

    [Header("Set in Inspector")]
    public List<float> ScoreFontSizes = new List<float> { 36, 64, 64, 1 };
    public Vector3 ScoreMidPoint = new Vector3(1, 1, 0);
    public float ScoreTravelTime = 3f;
    public float ScoreComboDelay = 0.5f;

    private RectTransform _rectTransform;

    private void Awake()
    {
        _S = this;
        _rectTransform = GetComponent<RectTransform>();
    }

    // ���� ����� ����� ������� ��� ScoreManager.Score() �� ������ �����
    static public void SCORE(Wyrd wyrd, int combo)
    {
        _S.Score(wyrd, combo);
    }

    // �������� ���� �� ��� �����
    // int combo - ����� ����� ����� � ����������
    private void Score(Wyrd wyrd, int combo)
    {
        // ������� ������ List<Vector2> � �������, ������������� ������ ����� ��� FloatingScore
        List<Vector2> points = new List<Vector2>();

        // �������� ������� ������ � ������ ������ � Wyrd
        Vector3 point = wyrd.Letters[0].transform.position;
        point = Camera.main.WorldToViewportPoint(point);

        points.Add(point);   // ������� point ������ ������ ������ �����

        // �������� ������ ����� ������ �����
        points.Add(ScoreMidPoint);

        // ������� Scoreboard ��������� ������ ������ �����
        points.Add(_rectTransform.anchorMax);

        // ���������� �������� ��� FloatingScore
        int value = wyrd.Letters.Count * combo;
        FloatingScore floatingScore = Scoreboard.S.CreateFloatingScore(value, points);

        floatingScore.TimeDuration = ScoreTravelTime;
        floatingScore.TimeStart = Time.time + combo * ScoreComboDelay;
        floatingScore.FontSizes = ScoreFontSizes;

        // ������� ������ InOut �� Easing
        floatingScore.EasingCurve = Easing.InOut + Easing.InOut;

        // ������� � FloatingScore ����� ���� "3 x 2"
        string text = wyrd.Letters.Count.ToString();
        if (combo > 1)
        {
            text += " x " + combo;
        }

        floatingScore.GetComponent<Text>().text = text;
    }
}
