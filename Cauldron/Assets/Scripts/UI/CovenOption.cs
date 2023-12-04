using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace CauldronCodebase
{
    public class CovenOption : MonoBehaviour, IPointerClickHandler
    {
        public CovenNightEventProvider covenNightEventProvider;
        public Statustype status = Statustype.Fear;
        public bool high;
        public CanvasGroup fader;
        private GameDataHandler gameDataHandler;
        private NightPanel nightPanel;
        private bool interactable;
        
        [Inject]
        private void Construct(GameDataHandler dataHandler, NightPanel panel)
        {
            gameDataHandler = dataHandler;
            nightPanel =panel;
        }

        private void OnEnable()
        {
            fader.alpha = gameDataHandler.IsEnoughMoney() ? 1 : 0.3f;
            interactable = gameDataHandler.IsEnoughMoney();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!interactable)
            {
                return;
            }
            nightPanel.AddCovenEvent(covenNightEventProvider.GetRandom(status, high));
        }
    }
}