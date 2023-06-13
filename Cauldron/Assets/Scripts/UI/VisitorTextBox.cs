using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using EasyLoc;
using TMPro;
using UnityEngine;
using Zenject;

namespace CauldronCodebase
{
    public class VisitorTextBox : MonoBehaviour
    {
        const string DEVIL = "devil";

        [Localize]
        public string devilDefault = "Привет, милая. Как делишки? Покажи-ка, что ты умеешь.";
        public float offScreen = -1000;
        public float animTime = 0.5f;
        public TMP_Text text;
        public VisitorTextIcon[] iconObjects = new VisitorTextIcon[3];
        
        [Inject] private RecipeBook recipeBook;
        [Inject] private RecipeProvider recipeProvider;
        [Inject] private IngredientsData ingredients;
        [Inject] private LocalizationTool locTool;

        private Encounter currentEncounter;

        
        [ContextMenu("Find Icon Objects")]
        private void FindIconObjects()
        {
            iconObjects = GetComponentsInChildren<VisitorTextIcon>();
        }

        private void Start()
        {
            locTool.OnLanguageChanged += ReloadVisitorText;
        }

        private void ReloadVisitorText()
        {
            if (currentEncounter != null)
            {
                Display(currentEncounter);
                //fix devil?
            }
        }

        public void Hide()
        {
            currentEncounter = null;
            gameObject.SetActive(false);
        } 
        
        public void Display(Encounter card)
        {
            gameObject.transform.DOLocalMoveX(gameObject.transform.localPosition.x, animTime)
                .From(offScreen);

            currentEncounter = card;
            if (card.name.Contains(DEVIL))
            {
                Recipe unlockRecipe = GetRecipeToUnlock();
                if (unlockRecipe == null)
                {
                    text.text = devilDefault;
                }
                else
                {
                    text.text = Format(card, unlockRecipe);
                }
            }
            else
            {
                text.text = card.text;
            }

            iconObjects[0]?.Display(card.primaryInfluence, card.hidden);
            iconObjects[1]?.Display(card.secondaryInfluence, card.hidden);
            if (card.quest)
            {
                iconObjects[2]?.DisplayItem(card.villager);
                text.fontStyle = FontStyles.Italic;
            }
            else
            {
                iconObjects[2]?.Hide();
                text.fontStyle = FontStyles.Normal;
            }

            string Format(Encounter encounter, Recipe unlockRecipe)
            {
                string name1 = ingredients.Get(unlockRecipe.RecipeIngredients[0]).friendlyName.ToLower();
                string name2 = ingredients.Get(unlockRecipe.RecipeIngredients[1]).friendlyName.ToLower();
                string name3 = ingredients.Get(unlockRecipe.RecipeIngredients[2]).friendlyName.ToLower();
                return String.Format(encounter.text, name1, name2, name3);
            }

            gameObject.SetActive(true);
        }
        
        //move?
        private Recipe GetRecipeToUnlock()
        {
            return recipeProvider.allRecipes.Where(x => x.magical).FirstOrDefault((x =>
                !recipeBook.IsRecipeInBook(x)));
        }
    }
}