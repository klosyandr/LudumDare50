using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using NaughtyAttributes;
using Save;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CauldronCodebase
{
    [Serializable]
    public struct CardPoolByRound
    {
        [UsedImplicitly] [HideInInspector] public string title;
        public int round;
        public Encounter[] cards;

        public CardPoolByRound(int round, Encounter[] cards)
        {
            this.round = round;
            this.cards = cards;
            title = $"Round {round}: {cards.Length} cards";
        }
    }
    
    [CreateAssetMenu]
    public class EncounterDeck : ScriptableObject, IDataPersistence
    {
        public Encounter[] introCards;
        public CardPoolByRound[] cardPoolsByRound;
        public LinkedList<Encounter> deck;

        [Header("DEBUG"), HorizontalLine]
        [SerializeField] private Encounter currentCard;
        [SerializeField, UsedImplicitly] private Encounter[] deckInfo;
        [SerializeField] private List<Encounter> cardPool;
        [SerializeField] private List<string> rememberedCards;

        private RecipeProvider recipeProvider;
        private GameDataHandler gameDataHandler;
        private SODictionary soDictionary;
        private Encounter loadedCard;
        private MainSettings mainSettings;
        private int lastExtendedRoundNumber;

        private void OnValidate()
        {
            for (var index = 0; index < cardPoolsByRound.Length; index++)
            {
                ref var pool = ref cardPoolsByRound[index];
                pool.title = $"Round {pool.round}: {pool.cards.Length} cards";
            }
        }

        /// <summary>
        /// Form new deck and starting card pool.
        /// </summary>
        public void Init(GameDataHandler game, DataPersistenceManager dataPersistenceManager,
            SODictionary dictionary, MainSettings settings, RecipeProvider recipes)
        {
            gameDataHandler = game;
            soDictionary = dictionary;
            mainSettings = settings;
            recipeProvider = recipes;
            dataPersistenceManager.AddToDataPersistenceObjList(this);

            InitRememberedCards();
        }

        /// <summary>
        /// Form card pool, adding cards sets up to the given game round and excluding unique cards.
        /// </summary>
        /// <param name="round">Round — card set number</param>
        private void InitCardPool(int round)
        {
            lastExtendedRoundNumber = gameDataHandler.currentRound;
            List<Encounter> totalPool = new List<Encounter>();
            foreach (var pool in cardPoolsByRound)
            {
                if (pool.round <= round)
                {
                    if (round == 0 || (round > mainSettings.gameplay.roundsWithUniqueStartingCards))
                    {
                        totalPool.AddRange(pool.cards);
                    }
                    else
                    {
                        totalPool.AddRange(pool.cards.Except(rememberedCards.
                            Select(x => (Encounter) soDictionary.AllScriptableObjects[x])).ToArray());
                    }
                }
            }
            cardPool = Shuffle(totalPool);
        }

        private void InitRememberedCards()
        {
            string rememberedCardsJson = PlayerPrefs.GetString(PrefKeys.UniqueCards);
            if (!string.IsNullOrEmpty(rememberedCardsJson))
            {
                var wrapper = JsonUtility.FromJson<EncounterListWrapper>(rememberedCardsJson);
                rememberedCards.Clear();
                rememberedCards.AddRange(wrapper.encounters);
            }
            else
            {
                rememberedCards = new List<string>();
            }
        }

        private static List<Encounter> Shuffle(List<Encounter> deck)
        {
            var newDeckList = new List<Encounter>(deck.Count);
            while (deck.Count > 0)
            {
                int random = Random.Range(0, deck.Count);
                newDeckList.Add(deck[random]);
                deck.RemoveAt(random);
            }
            return newDeckList;
        }

        public void ShuffleDeck()
        {
            List<Encounter> deckList = deck.ToList();
            var newDeckList = new LinkedList<Encounter>();
            while (deckList.Count > 0)
            {
                int random = Random.Range(0, deckList.Count);
                newDeckList.AddFirst(deckList[random]);
                deckList.RemoveAt(random);
            }
            deck = newDeckList;
        }


        /// <summary>
        /// Add random cards from pool to deck until deck count reaches target.
        /// </summary>
        /// <param name="target">X - target number of cards in deck</param>
        public void DealCardsTo(int target)
        {
            if (target - deck.Count <= 0)
            {
                return;
            }
            DealCards(target - deck.Count);
        }

        private void DealCards(int num)
        {
            if (cardPool.Count < num)
            {
                ExtendPool();
            }
            for (int i = 0; i < num; i++)
            {
                bool cardFound = false;
                for (int j = 0; j< 10; j++)
                {
                    int randomIndex = Random.Range(0, cardPool.Count);
                    if (StoryTagHelper.Check(cardPool[randomIndex], gameDataHandler))
                    {
                        deck.AddLast(cardPool[randomIndex]);
                        cardPool.RemoveAt(randomIndex);
                        cardFound = true;
                        break;
                    }
                }

                if (!cardFound)
                {
                    Debug.LogWarning("No suitable card found in pool");
                }
            }

            cardPool.TrimExcess();
            deckInfo = deck.ToArray();
        }

        private void ExtendPool()
        {
            var nextPools = cardPoolsByRound.Where(x => x.round == lastExtendedRoundNumber+1).ToArray();
            if (nextPools.Length == 0)
            {
                return;
            }
            foreach (var pool in nextPools)
            {
                cardPool.AddRange(pool.cards);
            }
            lastExtendedRoundNumber++;
        }

        public void AddToPool(Encounter card)
        {
            cardPool.Add(card);
        }

        public bool AddToDeck(Encounter card, bool asFirst = false)
        {
            if (card is null)
            {
                return true;
            }
            
            if (!StoryTagHelper.Check(card, gameDataHandler) || deck.Contains(card))
            {
                return false;
            }
            
            if (asFirst)
            {
                deck.AddFirst(card);
            }
            else
            {
                deck.AddLast(card);
            }
            deckInfo = deck.ToArray();
            return true;
        }

        public Encounter GetTopCard()
        {
            if (deck.Count == 0)
            {
                return null;
            }
            if (loadedCard != null)
            {
                currentCard = loadedCard;
                loadedCard = null;
            }
            else
            {
                var topCard = deck.First();
                deck.RemoveFirst();
                currentCard = topCard;
            }

            if (gameDataHandler.currentDay < mainSettings.gameplay.daysWithUniqueStartingCards
                && gameDataHandler.currentRound < mainSettings.gameplay.roundsWithUniqueStartingCards
                && !EncounterIdents.GetAllSpecialCharacters().Contains(currentCard.villager.name))
            {
                SaveCurrentCardAsUnique();
            }

            return currentCard;
        }

        void SaveCurrentCardAsUnique()
        {
            rememberedCards.Add(currentCard.name);
            EncounterListWrapper wrapper = new EncounterListWrapper { encounters = rememberedCards };
            string json = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString(PrefKeys.UniqueCards, json);
            Debug.Log("unique cards: "+json);
        }
        

        public void LoadData(GameData data, bool newGame)
        {
            loadedCard = gameDataHandler.currentCard;

            deck = new LinkedList<Encounter>();
            cardPool = new List<Encounter>(15);

            switch (newGame)
            {
                case true:
                    SetStartingDecks();
                    break;
                case false:
                    cardPool = new List<Encounter>();
                    if (data.CardPool != null)
                    {
                        foreach (var key in data.CardPool)
                        {
                            cardPool.Add((Encounter)soDictionary.AllScriptableObjects[key]);
                        }
                    }

                    lastExtendedRoundNumber = data.LastExtendedPoolNumber;

                    if (data.CurrentDeck != null)
                    {
                        List<Encounter> currentDeck = new List<Encounter>();
                        foreach (var key in data.CurrentDeck)
                        {
                            currentDeck.Add((Encounter)soDictionary.AllScriptableObjects[key]);
                        }

                        deck = new LinkedList<Encounter>(currentDeck);
                        Debug.Log("New deck");
                    }

                    break;
            }
        }

        private void SetStartingDecks()
        {
            int round = PlayerPrefs.GetInt(PrefKeys.CurrentRound);
            InitCardPool(round);
            //if not first time
            if (round != 0)
            {
                DealCards(2);
                List<Recipe> loadRecipes = recipeProvider.LoadRecipes().ToList();
                if (loadRecipes.Count < 20)
                {
                    deck.AddFirst(introCards[3]);
                }
                else
                {
                    deck.AddFirst(introCards[2]);
                }
            }
            else
            {
                DealCards(1);
                deck.AddFirst(introCards[0]);
                deck.AddLast(introCards[1]);
            }
        }

        public void SaveData(ref GameData data)
        {
            if (data == null) return;
            data.LastExtendedPoolNumber = lastExtendedRoundNumber;
            
            data.CardPool.Clear();
            foreach (var card in cardPool)
            {
                data.CardPool.Add(card.name);
            }

            data.CurrentDeck.Clear();
            foreach (var card in deck)
            {
                data.CurrentDeck.Add(card.name);
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Export Cards Tool")]
        void ExportCards()
        {
            var file = File.CreateText(Application.dataPath + "/Localize/Cards.csv");
            file.WriteLine("id;description_RU;description_EN");
            foreach (Encounter unit in Resources.FindObjectsOfTypeAll<Encounter>())
            {
                file.WriteLine(unit.name + ";" + unit.text);
            }

            file.Close();
        }

        [ContextMenu("Update round for all cards")]
        void UpdateCards()
        {
            foreach (var pool in cardPoolsByRound)
            {
                foreach (var card in pool.cards)
                {
                    card.addToDeckOnRound = pool.round;
                    EditorUtility.SetDirty(card);
                }
            }
        }
#endif
    }
}