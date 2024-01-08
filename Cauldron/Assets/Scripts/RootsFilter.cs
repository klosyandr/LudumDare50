﻿using System;
using CauldronCodebase;
using UnityEngine;
using UnityEngine.UI;

public class RootsFilter : MonoBehaviour
{
    [SerializeField] private Button rootTypeButton;
    [SerializeField] private FilterButton root1Button;
    [SerializeField] private FilterButton root2Button;
    [SerializeField] private Image gauge;

    public bool IsShow { get; private set; }
    
    public event Action AddedFilter;
    public event Action<IngredientsData.Ingredient> Show;

    private void Awake()
    {
        DisableButton();
    }
    
    private void OnEnable()
    {
        rootTypeButton.onClick.AddListener(EnableButtons);
        root1Button.SwitchFilter += OnSwitchFilter;
        root2Button.SwitchFilter += OnSwitchFilter;
    }

    private void OnDisable()
    {
        rootTypeButton.onClick.RemoveListener(EnableButtons);
        root1Button.SwitchFilter -= OnSwitchFilter;
        root2Button.SwitchFilter -= OnSwitchFilter;
    }

    public void DisableButton()
    {
        root1Button.gameObject.SetActive(false);
        root2Button.gameObject.SetActive(false);
        IsShow = false;
    }

    private void EnableButtons()
    {
        AddedFilter?.Invoke();
        root1Button.gameObject.SetActive(true);
        root2Button.gameObject.SetActive(true);
        IsShow = true;
    }

    private void OnSwitchFilter(IngredientsData.Ingredient ingredient)
    {
        Show?.Invoke(ingredient);
        gauge.gameObject.SetActive(true);
    }

    public void ClearFilter(IngredientsData.Ingredient lastIngredient)
    {
        gauge.gameObject.SetActive(false);
        root1Button.DisableFilter();
        root2Button.DisableFilter();
        
        if (root1Button.Ingredient == lastIngredient)
        {
            gauge.gameObject.SetActive(true);
            root1Button.EnableDisableFilter();
        }
        else if (root2Button.Ingredient == lastIngredient)
        {
            gauge.gameObject.SetActive(true);
            root2Button.EnableDisableFilter();
        }
    }
}