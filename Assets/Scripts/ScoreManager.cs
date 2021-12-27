using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    static private ScoreManager _S;   // Ещё один скрытый объект-одиночка

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

    // Этот метод можно вызвать как ScoreManager.Score() из любого места
    static public void SCORE(Wyrd wyrd, int combo)
    {
        _S.Score(wyrd, combo);
    }

    // Добавить очки за это слово
    // int combo - номер этого слова в комбинации
    private void Score(Wyrd wyrd, int combo)
    {
        // Создать список List<Vector2> с точками, определяющими кривую Безье для FloatingScore
        List<Vector2> points = new List<Vector2>();

        // Получить позицию плитки с первой буквой в Wyrd
        Vector3 point = wyrd.Letters[0].transform.position;
        point = Camera.main.WorldToViewportPoint(point);

        points.Add(point);   // Сделать point первой точкой кривой Безье

        // Добавить вторую точку кривой Безье
        points.Add(ScoreMidPoint);

        // Сделать Scoreboard последней точкой кривой Безье
        points.Add(_rectTransform.anchorMax);

        // Определить значение для FloatingScore
        int value = wyrd.Letters.Count * combo;
        FloatingScore floatingScore = Scoreboard.S.CreateFloatingScore(value, points);

        floatingScore.TimeDuration = ScoreTravelTime;
        floatingScore.TimeStart = Time.time + combo * ScoreComboDelay;
        floatingScore.FontSizes = ScoreFontSizes;

        // Удвоить эффект InOut из Easing
        floatingScore.EasingCurve = Easing.InOut + Easing.InOut;

        // Вывести в FloatingScore текст вида "3 x 2"
        string text = wyrd.Letters.Count.ToString();
        if (combo > 1)
        {
            text += " x " + combo;
        }

        floatingScore.GetComponent<Text>().text = text;
    }
}
