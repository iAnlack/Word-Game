using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// The Scoreboard class manages showing the score to the player
public class Scoreboard : MonoBehaviour {
    public static Scoreboard S; // The singleton for Scoreboard

    [Header("Set in Inspector")]
    public GameObject       PrefabFloatingScore;

    [Header("Set Dynamically")]
    [SerializeField] private int    _score = 0;
    [SerializeField] private string _scoreString;

    private Transform       _canvasTransform;

    // The score property also sets the scoreString
    public int ScoreSetter 
    {
        get 
        {
            return(_score);
        }
        set 
        {
            _score = value;
            ScoreStringSetter = _score.ToString("N0");
        }
    }

    // The scoreString property also sets the Text.text
    public string ScoreStringSetter 
    {
        get 
        {
            return(_scoreString);
        }
        set {
            _scoreString = value;
            GetComponent<Text>().text = _scoreString;
        }
    }

    private void Awake() 
    {
        if (S == null) 
        {
            S = this; // Set the private singleton
        } 
        else 
        {
            Debug.LogError("ERROR: Scoreboard.Awake(): S is already set!");
        }
        _canvasTransform = transform.parent;
    }

    // When called by SendMessage, this adds the fs.score to this.score
    public void FSCallback(FloatingScore fs) 
    {
        ScoreSetter += fs.ScoreSetter;
    }

    // This will Instantiate a new FloatingScore GameObject and initialize it.
    // It also returns a pointer to the FloatingScore created so that the
    //   calling function can do more with it (like set fontSizes, and so on)
    public FloatingScore CreateFloatingScore(int amt, List<Vector2> pts) 
    {
        GameObject go = Instantiate<GameObject>(PrefabFloatingScore);
        go.transform.SetParent( _canvasTransform );
        FloatingScore fs = go.GetComponent<FloatingScore>();
        fs.ScoreSetter = amt;
        fs.ReportFinishTo = this.gameObject; // Set fs to call back to this
        fs.Init(pts);
        return(fs);
    }
}
