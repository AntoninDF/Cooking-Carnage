using GameSystem;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class Subtitle : MonoBehaviour
{
    #region Properties
    /// <summary>
    /// To play the subtitle, use Play() or PlayWithDelay()
    /// </summary>
    public bool IsPlaying => _isPlaying;
    public float Delay { get => _delay; set => _delay = value; }
    public float Duration { get => _duration; set => _duration = value; }
    public string Text { get => _text; set => _text = value; }
    public Subtitle NextSubtitle { get => _nextSubtitle; set => _nextSubtitle = value; }

    #endregion
    #region Fields
    [Header("Parameters")]
    [SerializeField]
    [TextArea(15, 20)]
    private string _text = "[Empty]";
    [SerializeField]
    private float _duration = 5f;
    [SerializeField, Tooltip("How long this subtitle will wait after being played before showing up")]
    private float _delay = 0f;

    [Header("References")]
    [SerializeField]
    private TMP_Text _textComponent;
    [SerializeField, Tooltip("Reference another subtitle component here if you want it to play after this one")]
    private Subtitle _nextSubtitle;

    // Private fields
    private bool _isPlaying = false;
    TimerManager tm;


    #endregion

    #region Methods

    private void Start()
    {
        if (_textComponent == null)
            _textComponent = GetComponent<TMP_Text>();
        if (_textComponent == null)
            throw new System.Exception("[Subtitle] : Please reference the text component in the inspector");
        tm = ReferenceManager.Get<TimerManager>();
        if (tm == null)
            throw new System.Exception("[Subtitle] No timer manager found via the reference manager");
    }

    public void PlayWithDelay() => PlayWithDelay(_delay);
    public void PlayWithDelay(float delay) => tm.StartTimer("Play delayed subtitle", delay, Play);
    public void Play()
    {
        _isPlaying = true;

        _textComponent.gameObject.SetActive(true);
        tm.StartTimer("Stop subtitles", _duration, Stop);
    }

    public void Stop()
    {
        _isPlaying = false;
    }

    public void PlayNextSubtitle()
    {
        if (_nextSubtitle == null) return;
        _nextSubtitle.gameObject.SetActive(true);
        _nextSubtitle.Play();
    }

    public void SetText(string  text) => _textComponent.text = text;
    #endregion
}
