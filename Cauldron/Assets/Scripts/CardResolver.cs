using System.Linq;
using CauldronCodebase.Sounds;
using UnityEngine;

namespace CauldronCodebase
{
    public class CardResolver
    {
        private readonly GameData game;
        private readonly EncounterDeckBase deck;
        private readonly NightEventProvider nightEvents;
        private readonly MainSettings settings;

        public CardResolver(MainSettings settings, GameData game, EncounterDeckBase deck, NightEventProvider nightEvents)
        {
            this.game = game;
            this.deck = deck;
            this.nightEvents = nightEvents;
            this.settings = settings;
        }
        
        public bool EndEncounter(Potions potion)
        {
            Encounter encounter = game.currentCard;
            //compare distinct potion
            foreach (var filter in encounter.resultsByPotion)
            {
                if (potion == filter.potion)
                {
                    ApplyResult(filter);
                    return true;
                }
            }

            //evaluate potion filters
            if (PotionInFilter(Potions.ALCOHOL)) return true;
            if (PotionInFilter(Potions.DRINK)) return true;
            if (PotionInFilter(Potions.FOOD)) return true;
            if (PotionInFilter(Potions.MAGIC)) return true;
            if (PotionInFilter(Potions.NONMAGIC)) return true;
            var filterResult = encounter.resultsByPotion.FirstOrDefault(result => result.potion == Potions.DEFAULT);
            if (filterResult != null)
            {
                ApplyResult(filterResult);
                return true;
            }
            return false;

            void ModifyStat(Statustype type, float statCoef, float potionCoef)
            {
                if (type == Statustype.None)
                {
                    return;
                }

                float defaultStatChange = type == Statustype.Money
                    ? settings.gameplay.defaultMoneyChangeCard
                    : settings.gameplay.defaultStatChange;
                
                game.Add(type,
                    Mathf.FloorToInt(defaultStatChange
                                     * statCoef
                                     * potionCoef));
            }

            void ApplyResult(Encounter.PotionResult potionResult)
            {
                if (potionResult.influenceCoef > 0) SoundManager.theOne.PlaySuccess();
                else if (potionResult.influenceCoef < 0) SoundManager.theOne.PLayFailure();
                ModifyStat(encounter.primaryInfluence, encounter.primaryCoef, potionResult.influenceCoef);
                ModifyStat(encounter.secondaryInfluence, encounter.secondaryCoef, potionResult.influenceCoef);
                if (potionResult.bonusCard != null)
                    deck.AddCardToPool(potionResult.bonusCard);
                if (potionResult.bonusEvent != null)
                    nightEvents.storyEvents.Add(potionResult.bonusEvent);
            }

            bool PotionInFilter(Potions filter)
            {
                Encounter.PotionResult filterValue = encounter.resultsByPotion.FirstOrDefault(result => result.potion == filter);
                if (filterValue != null && PotionFilter.Get(filter).Contains(potion))
                {
                    ApplyResult(filterValue);
                    return true;
                }
                return false;
            }
        }
    }
}