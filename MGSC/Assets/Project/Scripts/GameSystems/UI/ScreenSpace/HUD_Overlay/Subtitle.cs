using GameSystem;
using NaughtyAttributes;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace UI
{
	public class Subtitle : UIWidget
	{
		#region Properties
		private Timer TextDisplayTimer
		{
			get => _textDisplayTimer;
			set
			{
				if (value == _textDisplayTimer) return;
				if (_textDisplayTimer != null)
					_textDisplayTimer.Pause();
				_textDisplayTimer = value;
			}
		}
		private Timer _textDisplayTimer;

		private Timer FadeInOutTimer;
		private float Visibility
		{
			get => _visibility;
			set => SetVisibility(value);
			
		}
		private float _visibility;
		#endregion

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

		public void WaitAndDo(float waitDuration, Action action)
		{
			TimerManager.StartGameplayTimer("Subtitle WaitAndDo", waitDuration, action);
		}

		#region Visibility
		public void SetVisibility(float value)
		{
			_visibility = value;
			if (ControlVisibility && CanvasGroup != null)
				CanvasGroup.alpha = value;
		}
		public void FadeIn(float duration = 0.5f, Action OnFadeEnd = null)
			=> VisibilityFade(Visibility, 1f, (1f - Visibility) * duration, OnFadeEnd);
		public void FadeOut(float duration = 0.5f, Action OnFadeEnd = null)
			=> VisibilityFade(Visibility, 0f, Visibility * duration, OnFadeEnd);
		public void VisibilityFade(float startVisibility, float endVisibility, float duration, Action OnFadeEnd = null)
		{
			OnFadeEnd += () => Visibility = endVisibility;
			OnFadeEnd += () => GameLoop.UnbindTimerFromGamePause(FadeInOutTimer);

			if (FadeInOutTimer != null) FadeInOutTimer.Pause();
			FadeInOutTimer = TimerManager.StartGameplayTimer(
				"Subtitle opacity fade", // Fixed name so that new timers replace the previous one
				duration,
				OnFadeEnd,
				elapsed => Visibility = Mathf.Lerp(startVisibility, endVisibility, elapsed / duration));
		} 
		#endregion

		#region Text Display
		public void DisplayText(string text, float charactersPerSecond = 16, Action OnTextEnd = null)
		{
			int characterCount = text.Length;
			float duration = characterCount / charactersPerSecond;
			OnTextEnd += () => DisplayText(text);
			OnTextEnd += () => GameLoop.UnbindTimerFromGamePause(TextDisplayTimer);
            TextDisplayTimer = TimerManager.StartGameplayTimer("Display subtitle text", duration, OnTextEnd, elapsed => DisplayPartialText(text, elapsed / duration));
			GameLoop.BindTimerToGamePause(TextDisplayTimer);
		}
		public void DisplayText(string text)
		{
			if (L_BodyText == null) return;
			L_BodyText.text = text;
		}

		void DisplayPartialText(string text, int characterCount)
		{
			string result = "";

			int offset = 0;
			for (int i = 0; i < characterCount;i++)
			{
				if (i == characterCount)
					break;
				if (i + offset >= text.Length)
					break;

				int index = i + offset;
				char c = text[index];
				result += c;

				// Handle markups
				if (c == '<')
				{
					int markupEndIndex = text.IndexOf('>', index);
					if (markupEndIndex == -1)
						continue;
					result += text.Substring(index + 1, markupEndIndex - index);
					offset += markupEndIndex - index;
				}
			}

			DisplayText(result);
		}
		void DisplayPartialText(string text, float percentage)
		{
			percentage = Mathf.Clamp01(percentage);
			int characterCount = (int)(text.Length * percentage);
			DisplayPartialText(text, characterCount);
		}  
		#endregion
		#endregion

		void OnValidate()
		{
			if (ControlVisibility && !TryGetComponent(out CanvasGroup temp))
				CanvasGroup = gameObject.AddComponent<CanvasGroup>();
		}
	}

}
