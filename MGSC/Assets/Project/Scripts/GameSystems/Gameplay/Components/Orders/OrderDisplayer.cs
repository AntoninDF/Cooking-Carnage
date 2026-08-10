using Cooking;
using System.Collections;
using System.Collections.Generic;
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
        public void DisplayOrder(Order order)
        {
            if (order == null) return;
            SO_Recipe recipe = order.Recipe;
            string debug = "[OrderDisplayer] DisplayOrder : ";
            debug += "\nIngredient image count = " + _ingredients.Count;
            debug += "\nIngredient in recipe count = " + recipe.Ingredients.Count;
            _hasTriggered1Star = false;
            _hasTriggered2Star = false;
            
            if(recipe.Ingredients.Count > 4) Debug.Log("What the fuck ??");
            for (int i = 0; i < _ingredients.Count; i++)
            {
                if(i< recipe.Ingredients.Count)
                {
                    _ingredients[i].sprite = recipe.Ingredients[i].Sprite;
                    _ingredients[i].color = Color.white;
                    debug += "\nSet image n°" + i + " to sprite = " + _ingredients[i].sprite.name + " and color = " + _ingredients[i].color;
                }
                else
                {
                    _ingredients[i].color = new Color(1, 1, 1, 0);
                    debug += "\nSet image n°" + i + " to empty (color = " + _ingredients[i].color + ")";
                }
            }

            if (order.Owner != null && order.Owner.PatienceTimer != null)
            {
                var timer = order.Owner.PatienceTimer;
                order.Owner.PatienceTimer.onUpdate += elapsed => UpdatePriceText(recipe, elapsed, (timer != null ? timer.Duration : 60));
            }
            Debug.Log(debug);
            StartDisplay();
        }
        void UpdatePriceText(SO_Recipe recipe, float patienceElapsed, float totalPatience)
        {
            _price.text = GetPrice(recipe, patienceElapsed, totalPatience).ToString() + " $";
        }
        private float GetPrice(SO_Recipe recipe, float elapsedTime, float totalPatience)
        {
            float price = OrderEvaluator.CalculateBasePrice(recipe);
            float tip = OrderEvaluator.CalculateTip(price, OrderEvaluator.CalculateStars(elapsedTime, totalPatience));
            return price + tip;
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

        // Note, this method is also called via Unity events, in OrderingArea.OnCustomerKilled and ClientSeat.OnClientLeaving
        public void UndisplayOrder()
        {
            StopDisplay();
            StartCoroutine(StopDisplayingOrder());
            Debug.Log("[OrderDisplayer] Undisplay order");
        }

        IEnumerator StopDisplayingOrder()
        {
            yield return new WaitForSeconds(_stopTV.length);
            string debug = "[OrderDisplayer] Stop displaying order";
            foreach (Image image in _ingredients)
            {
                debug += "\n\tSetting " + image.name + "'s sprite to null (currently '" + image.sprite + "')";
                image.sprite = null;
            }
            Debug.Log(debug);
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