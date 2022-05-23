using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class IngredientDroppable: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, 
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [FormerlySerializedAs("type")] public Ingredients ingredient;
        public float rotateAngle = 10f;
        public float rotateSpeed = 0.3f;
        public float returntime = 0.5f;
        public IngredientsData dataList;
        
        [SerializeField, HideInInspector]
        private Text tooltip;
        [SerializeField, HideInInspector]
        private SpriteRenderer image;

        ParticleSystem ingredientParticle;
        bool isHighlighted = false;
        private Vector3 initialPosition;
        private bool dragging;

        private void OnValidate()
        {
            tooltip = GetComponentInChildren<Text>();
            if (tooltip != null && tooltip.text == String.Empty)
            {
                tooltip.text = dataList?.Get(ingredient)?.friendlyName ?? "not specified";
                //Debug.Log("override ingredient text!");
            }

            image = GetComponentInChildren<SpriteRenderer>();
            image.sprite = dataList?.Get(ingredient)?.image;
            ingredientParticle = GetComponentInChildren<ParticleSystem>();
        }

        private void Start()
        {
            if (tooltip != null)
            {
                tooltip.text = dataList?.Get(ingredient)?.friendlyName ?? "not specified";
                tooltip.gameObject.SetActive(false);
            }

            initialPosition = transform.position;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (tooltip is null)
                return;
            tooltip.gameObject.SetActive(true);
            if (!Cauldron.instance.mix.Contains(ingredient))
                image.gameObject.transform.
                    DORotate(new Vector3(0,0, rotateAngle), rotateSpeed).
                    SetLoops(-1, LoopType.Yoyo).
                    From(new Vector3(0, 0, -rotateAngle)).
                    SetEase(Ease.InOutSine);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (tooltip is null)
                return;
            tooltip.gameObject.SetActive(false);
            image.transform.DOKill();
            image.transform.DORotate(new Vector3(0, 0, 0), rotateSpeed);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (Cauldron.instance.mix.Contains(ingredient))
                return;
            dragging = true;
            image.transform.DOKill(true);
            Cauldron.instance.MouseEnterCauldronZone += OverCauldron;
        }

        void OverCauldron()
        {
            Cauldron.instance.AddToMix(ingredient);
            dragging = false;
            transform.position = initialPosition;
            Cauldron.instance.MouseEnterCauldronZone -= OverCauldron;
            transform.DOScale(transform.localScale, rotateSpeed).From(Vector3.zero);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!dragging)
                return;
            transform.position =
                Camera.main.ScreenToWorldPoint((Vector3)eventData.position + Vector3.forward * Camera.main.nearClipPlane);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!dragging)
                return;
            transform.DOMove(initialPosition, returntime);
            dragging = false;
            Cauldron.instance.MouseEnterCauldronZone -= OverCauldron;
        }

        public void EnableHighlight()
        {
            if(!isHighlighted)
            {
                ingredientParticle.Play();
                isHighlighted = true;
            }
        }

        public void DisableHighlight()
        {
            if(isHighlighted)
            {
                ingredientParticle.Stop();
                isHighlighted = false;
            }
        }
        
    }
}