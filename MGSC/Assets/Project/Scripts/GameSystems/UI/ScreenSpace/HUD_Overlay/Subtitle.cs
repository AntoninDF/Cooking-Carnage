using GameSystem;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace UI
{
	public class Subtitle : UIWidget
	{
		public Timer CurrentTimer
		{
			get => _currentTimer;
			set
			{
				if (value == _currentTimer) return;
				if (_currentTimer != null)
					_currentTimer.Pause();
				_currentTimer = value;
			}
		}
		private Timer _currentTimer;

        #region Fields
        public TMP_Text L_BodyText;

		[Tooltip("If enabled, this component will control the visibility of it's content. Only disable it if you control visibility elsewhere, otherwise subtitles won't show/disappear.")]
		public bool ControlVisibility = true;
		[ShowIf("ControlVisibility")]
		public CanvasGroup CanvasGroup;

		public bool AllowAnimatedText;
		public UnityEvent OnShow;
		public UnityEvent OnHide;

		#endregion

		#region Methods

		public void DisplayText(string text, float charactersPerSecond = 16)
		{
			Debug.Log("Display Animated Text");
			int characterCount = text.Length;
			float duration = characterCount / charactersPerSecond;
			CurrentTimer = TimerManager.StartTimer("Display subtitle text", duration, () => DisplayText(text), elapsed => DisplayPartialText(text, elapsed / duration));
		}
		public void DisplayText(string text)
		{
			Debug.Log("Display Text");
			if (L_BodyText == null) return;
			L_BodyText.text = text;
			Debug.Log("Display Text for real");
		}

		void DisplayPartialText(string text, int characterCount)
		{
			Debug.Log("Display Partial Text (int)");
			string result = text.Length > characterCount ? text.Substring(0, characterCount) : text;
			DisplayText(result);
		}
		void DisplayPartialText(string text, float percentage)
		{
			Debug.Log("Display Partial Text (float)");
			percentage = Mathf.Clamp01(percentage);
			int characterCount = (int)(text.Length * percentage);
			DisplayPartialText(text, characterCount);
		} 
		#endregion

		void OnValidate()
		{
			if (ControlVisibility && !TryGetComponent(out CanvasGroup temp))
				CanvasGroup = gameObject.AddComponent<CanvasGroup>();
		}
	}

}
