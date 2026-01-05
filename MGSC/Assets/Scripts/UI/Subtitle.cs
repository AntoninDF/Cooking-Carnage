using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using TMPro;
using Localization;
using GameSystem;
using FMODUnity;

namespace UI
{

    public class Subtitle : MonoBehaviour
    {
        #region Properties

        #endregion

        #region Fields

        [Header("Subtitle parameters")]
        [SerializeField]
        private List<SubtitleParam> _subtitles;

        [Header("Subtitle references")]
        [SerializeField]
        private LocalizedTextDatabase _textDatabase;
        [SerializeField]
        private AvailableLanguages _language;
        [SerializeField]
        private GameObject _subtitleContainer;
        [SerializeField]
        private TMP_Text _textComponent;



        // Hidden fields
        private Dictionary<string, SubtitleParam> _subtitlesDict;
        private SubtitleParam _currentSubtitle;
        private TimerManager _timerManager;
        private GameLoop _gameLoop;
        #endregion

        #region Methods
        private void Start()
        {
            if (_subtitleContainer == null || _textComponent == null)
                throw new System.NullReferenceException("[Subtitle] : Missing references, please assign them in the inspector");

            InitSubtitleDict();
            LocalizedText.EnforceDatabaseSetup(_textDatabase);
            _timerManager = ReferenceManager.Get<TimerManager>();
            _gameLoop = ReferenceManager.Get<GameLoop>();
        }

        public void Play(string subtitleName)
        {
            EnforceSubtitleDict();

            if (_subtitlesDict.ContainsKey(subtitleName))
            {
                SubtitleParam sp = _subtitlesDict[subtitleName];
                _subtitleContainer.SetActive(true);
                _currentSubtitle = sp;
                _textComponent.text = LocalizedText.GetText(_textDatabase, sp.TextID, _language);
                if (sp.VoiceLine != null)
                    sp.VoiceLine.Play();
                _currentSubtitle.OnStart?.Invoke();
                _gameLoop.OnPause.AddListener(PauseVoiceLine);
                _gameLoop.OnResume.AddListener(ResumeVoiceLine);
                //Debug.Log("Playing subtitle " + _currentSubtitle.name);
                _timerManager.StartTimer("Subtitle", sp.Duration, Stop);
            }
        }

        private void PauseVoiceLine()
        {
            if (_currentSubtitle.VoiceLine != null)
                _currentSubtitle.VoiceLine.EventInstance.setPaused(true);
            //Debug.Log("Pause detected from the subtitle script");
        }
        private void ResumeVoiceLine()
        {
            if (_currentSubtitle.VoiceLine != null)
                _currentSubtitle.VoiceLine.EventInstance.setPaused(false);
            //Debug.Log("Resume detected from the subtitle script");
        }
        public void Stop()
        {
            _subtitleContainer.SetActive(false);
            _currentSubtitle.VoiceLine?.Stop();
            _gameLoop.OnPause.RemoveListener(PauseVoiceLine);
            _gameLoop.OnResume.RemoveListener(ResumeVoiceLine);
            _currentSubtitle.OnEnd?.Invoke();
        }
        private void EnforceSubtitleDict()
        {
            if (_subtitlesDict == null)
                InitSubtitleDict();
        }
        private void InitSubtitleDict()
        {
            if (_subtitlesDict == null)
                _subtitlesDict = new();
            foreach(var param in _subtitles)
                _subtitlesDict[param.name] = param;
        }
        #endregion
    }

    [System.Serializable]
    public struct SubtitleParam
    {
        public string name;
        public string TextID;
        public float Duration;
        public StudioEventEmitter VoiceLine;
        public UnityEvent OnStart;
        public UnityEvent OnEnd;
    }
}