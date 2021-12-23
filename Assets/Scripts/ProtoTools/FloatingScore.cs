using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// An enum to track the possible states of a FloatingScore
public enum eFSState 
{
    Idle,
    Pre,
    Active,
    Post
}

// FloatingScore can move itself on screen following a Bézier curve
public class FloatingScore : MonoBehaviour 
{
    [Header("Set Dynamically")]
    public eFSState         State = eFSState.Idle;

    [SerializeField] private int _score = 0;
    public string                ScoreString;

    // The score property sets both _score and scoreString 
    public int ScoreSetter 
    {
        get 
        {
            return(_score); 
        }
        set 
        {
            _score = value;
            ScoreString = _score.ToString("N0"); // "N0" adds commas to the num
            // Search "C# Standard Numeric Format Strings" for ToString formats
            GetComponent<Text>().text = ScoreString;
        }
    }

    public List<Vector2>    BezierPts; // Bézier points for movement 
    public List<float>      FontSizes; // Bézier points for font scaling 
    public float            TimeStart = -1f;
    public float            TimeDuration = 1f;
    public string           EasingCurve = Easing.InOut; // Uses Easing in Utils.cs

    // The GameObject that will receive the SendMessage when this is done moving
    public GameObject       ReportFinishTo = null;

    private RectTransform   _rectTransform;
    private Text            _text;

    // Set up the FloatingScore and movement
    // Note the use of parameter defaults for eTimeS & eTimeD
    public void Init(List<Vector2> ePts, float eTimeS = 0, float eTimeD = 1) 
    {
        _rectTransform = GetComponent<RectTransform>();
        _rectTransform.anchoredPosition = Vector2.zero;

        _text = GetComponent<Text>();

        BezierPts = new List<Vector2>(ePts);

        if (ePts.Count == 1) // If there's only one point
        { 
            // ...then just go there.
            transform.position = ePts[0];
            return;
        }

        // If eTimeS is the default, just start at the current time
        if (eTimeS == 0)
        {
            eTimeS = Time.time;
        }

        TimeStart = eTimeS;
        TimeDuration = eTimeD;
        State = eFSState.Pre; // Set it to the pre state, ready to start moving
    }

    public void FSCallback(FloatingScore fs) 
    {
        // When this callback is called by SendMessage,
        //   add the score from the calling FloatingScore
        ScoreSetter += fs.ScoreSetter;
    }

    // Update is called once per frame
    void Update () 
    {
        // If this is not moving, just return
        if (State == eFSState.Idle)
        {
            return;
        }

        // Get u from the current time and duration
        // u ranges from 0 to 1 (usually)
        float u = (Time.time - TimeStart)/TimeDuration;
        // Use Easing class from Utils to curve the u value
        float uC = Easing.Ease (u, EasingCurve);
        if (u<0) // If u<0, then we shouldn't move yet.
        { 
            State = eFSState.Pre;
            _text.enabled= false; // Hide the score initially
        } 
        else 
        {
            if (u>=1) // If u>=1, we're done moving
            { 
                uC = 1; // Set uC=1 so we don't overshoot
                State = eFSState.Post;
                if (ReportFinishTo != null) //If there's a callback GameObject
                { 
                    // Use SendMessage to call the FSCallback method
                    //   with this as the parameter.
                    ReportFinishTo.SendMessage("FSCallback", this);
                    // Now that the message has been sent,
                    //   Destroy this gameObject
                    Destroy (gameObject);
                }
                else 
                { // If there is nothing to callback
                    // ...then don't destroy this. Just let it stay still.
                    State = eFSState.Idle;
                }
            }
            else 
            {
                // 0<=u<1, which means that this is active and moving
                State = eFSState.Active;
                _text.enabled = true; // Show the score once more
            }

            // Use Bézier curve to move this to the right point
            Vector2 pos = Utils.Bezier(uC, BezierPts);
            // RectTransform anchors can be used to position UI objects relative
            //   to total size of the screen
            _rectTransform.anchorMin = _rectTransform.anchorMax = pos;
            if (FontSizes != null && FontSizes.Count>0) 
            {
                // If fontSizes has values in it
                // ...then adjust the fontSize of this GUIText
                int size = Mathf.RoundToInt( Utils.Bezier(uC, FontSizes) );
                GetComponent<Text>().fontSize = size;
            } 
        }
    } 
}