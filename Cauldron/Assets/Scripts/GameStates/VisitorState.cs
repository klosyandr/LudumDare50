﻿using System.Linq;
using UnityEngine;

namespace CauldronCodebase.GameStates
{
    public class VisitorState : BaseGameState
    {
        private readonly EncounterDeckBase cardDeck;
        private readonly MainSettings mainSettings;
        private readonly GameData gameData;
        private readonly VisitorManager visitorManager;
        private readonly Cauldron cauldron;
        private readonly GameStateMachine stateMachine;
        private readonly NightEventProvider nightEvents;
        private readonly SoundManager soundManager;

        private readonly EncounterResolver resolver;

        public VisitorState(EncounterDeckBase deck,
                            MainSettings settings,
                            GameData gameData,
                            VisitorManager visitorManager,
                            Cauldron cauldron,
                            GameStateMachine stateMachine,
                            NightEventProvider nightEventProvider, 
                            SoundManager soundManager)
        {
            cardDeck = deck;
            mainSettings = settings;
            this.gameData = gameData;
            this.visitorManager = visitorManager;
            this.visitorManager.VisitorLeft += VisitorLeft;
            this.cauldron = cauldron;
            this.stateMachine = stateMachine;
            nightEvents = nightEventProvider;
            this.soundManager = soundManager;

            resolver = new EncounterResolver(settings, gameData, deck, nightEvents);
        }
        
        public override void Enter()
        {
            Encounter currentCard = cardDeck.GetTopCard();
            gameData.currentCard = currentCard;
            //in case we run out of cards
            if (gameData.currentCard is null)
            {
                stateMachine.SwitchState(GameStateMachine.GamePhase.EndGame);
                return;
            }
            
            currentCard.Init(gameData, cardDeck, nightEvents);           
            visitorManager.Enter(currentCard);
            cauldron.PotionAccepted += EndEncounter;
        }
        
        public override void Exit()
        {
            gameData.cardsDrawnToday++;
            cauldron.PotionAccepted -= EndEncounter;
            visitorManager.Exit();           
        }

        private void EndEncounter(Potions potion)
        {
            PlayRelevantSound(potion);

            if (!resolver.EndEncounter(potion))
            {
                gameData.AddPotion(potion, true);
            }
            else
            {
                gameData.AddPotion(potion, false);
            }

            stateMachine.SwitchState(GameStateMachine.GamePhase.VisitorWaiting);
        }

        private void PlayRelevantSound(Potions potion)
        {
            var potionCoef = gameData.currentCard.resultsByPotion.FirstOrDefault(x => x.potion == potion)?.influenceCoef ?? 0;
            if (potionCoef > 0)
            {
                soundManager.Play(Sounds.PotionSuccess);
            }
            else if (potionCoef < 0)
            {
                soundManager.Play(Sounds.PotionFailure);
            }
        }

        private void VisitorLeft()
        {
            stateMachine.SwitchState(GameStateMachine.GamePhase.VisitorWaiting);
        }
    }
}