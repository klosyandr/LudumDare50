using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CauldronCodebase
{
    public class VisitorTextIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Image icon;
        public Sprite fame, fear, money, question, item;
        public GameObject hint;

        private string hintText;

        public void DisplayItem(Villager villager)
        {
            icon.sprite = item;
            gameObject.SetActive(true);
            switch (villager.name)
            {
                case "DarkStranger":
                    hintText = "Договор подписан, и теперь вы с ним связаны.";
                    break;
                case "WitchMemory":
                    hintText = "Мои воспоминания не дают мне покоя.";
                    break;
                case "Cat":
                    hintText = "Мой котик Бобби. Мы всегда будем вместе.";
                    break;
            }
        }
        
        public void Display(Statustype type, bool hidden = false)
        {
            if (hidden)
            {
                icon.sprite = question;
                gameObject.SetActive(true);
                hintText = null;
                return;
            }
            switch (type)
            {
                case Statustype.None:
                    Hide();
                    break;
                case Statustype.Money:
                    gameObject.SetActive(true);
                    icon.sprite = money;
                    hintText = "Этот персонаж даст вам денег, если вы поможете ему.";
                    break;
                case Statustype.Fear:
                    gameObject.SetActive(true);
                    icon.sprite = fear;
                    hintText = "Если вы поможете или навредите, изменится шкала страха.";
                    break;
                case Statustype.Fame:
                    gameObject.SetActive(true);
                    hintText = "Если вы поможете или навредите, изменится шкала славы.";
                    icon.sprite = fame;
                    break;
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!string.IsNullOrEmpty(hintText))
            {
                hint.SetActive(true);
                hint.GetComponentInChildren<TMP_Text>().text = hintText;
            } 
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hint.SetActive(false);
        }
    }
}