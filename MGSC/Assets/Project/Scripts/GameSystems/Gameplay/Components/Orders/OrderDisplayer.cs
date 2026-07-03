using System.Collections;
using System.Collections.Generic;
using Cooking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Orders
{
    public class OrderDisplayer : MonoBehaviour
    {

        [SerializeField] private Animation _animation;
        [SerializeField] private TMP_Text _price;
        [SerializeField] private List<Image> _ingredients;
        [SerializeField] private List<RectTransform> _cookingMethodsPositions;
        
        [SerializeField] private AnimationClip _playTV;
        [SerializeField] private AnimationClip _stopTV;

        void Start()
        {
            foreach (Image image in _ingredients)
            {
                image.preserveAspect = true;
            }
        }
        
        public void DisplayOrder(SO_Recipe recipe)
        {
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
        }
        
        
        private void StopDisplay()
        {
            _animation.Play(_stopTV?.name);
        }

    }
}