using System.Collections;
using System.Collections.Generic;
using Cooking;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Orders
{
    public class OrderDisplayer : MonoBehaviour
    {

        [SerializeField] private Animation _animation;
        [SerializeField] private TMP_Text _price;
        [SerializeField] private List<Image> _ingredients;
        [SerializeField] private List<RectTransform> _cookingMethodsPositions;
        [SerializeField] private Image _timeSlider;

        
        [SerializeField] private AnimationClip _playTV;
        [SerializeField] private AnimationClip _stopTV;

        public UnityEvent OnStartDisplay;
        public UnityEvent On2StarFailed;
        public UnityEvent On1StarFailed;
        public UnityEvent OnStopDisplay;
        
        private bool _hasTriggered1Star;
        private bool _hasTriggered2Star;
        

        void Start()
        {
            foreach (Image image in _ingredients)
            {
                image.preserveAspect = true;
            }
        }
        private void OnEnable() => GameLoop.OnReset += UndisplayOrder;
        private void OnDisable() => GameLoop.OnReset -= UndisplayOrder;
        public void DisplayOrder(SO_Recipe recipe)
        {
            _hasTriggered1Star = false;
            _hasTriggered2Star = false;
            
            if(recipe.Ingredients.Count > 4) Debug.Log("What the fuck ??");
            for (int i = 0; i < _ingredients.Count; i++)
            {
                if(i< recipe.Ingredients.Count)
                {
                    _ingredients[i].sprite = recipe.Ingredients[i].Sprite;
                    _ingredients[i].color = Color.white;
                }
                else
                {
                    _ingredients[i].color = new Color(1, 1, 1, 0);
                }
            }

            _price.text = recipe.Price.ToString() + " $";
            
            StartDisplay();
        }
        
        public void UpdateTimeSlider(float progress01)
        {
            if (_timeSlider != null)
            {
                _timeSlider.fillAmount = 1f - progress01; 
                if (!_hasTriggered1Star && progress01 >= Globals.Gameplay.Threshold1Star)
                {
                    _hasTriggered1Star = true;
                    On1StarFailed?.Invoke();
                }
                
                if (!_hasTriggered2Star && progress01 >= Globals.Gameplay.Threshold2Star)
                {
                    _hasTriggered2Star = true;
                    On2StarFailed?.Invoke();
                }
            }
                
        }

        public void UndisplayOrder()
        {
            StopDisplay();
            StartCoroutine(StopDisplayingOrder());
        }

        IEnumerator StopDisplayingOrder()
        {
            yield return new WaitForSeconds(_stopTV.length);
            foreach (Image image in _ingredients)
            {
                image.sprite = null;
            }
        }

        private void StartDisplay()
        {
            _animation.Play(_playTV?.name);
            OnStartDisplay?.Invoke();
        }
        
        
        private void StopDisplay()
        {
            _animation.Play(_stopTV?.name);
            OnStopDisplay?.Invoke();
        }
        
        public void PlayAnimation(Animation animationComponent)
        {
            animationComponent.Play();
        } 

    }
}