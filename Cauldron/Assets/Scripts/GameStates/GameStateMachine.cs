﻿using System.Collections.Generic;
using System;
using Cysharp.Threading.Tasks;
using Save;
using UnityEngine;
using Zenject;

namespace CauldronCodebase.GameStates
{
    public class GameStateMachine : MonoBehaviour
    {
        public enum GamePhase
        {
            VisitorWaiting,
            Visitor,
            Night,
            EndGame
        }

        
        [SerializeField] private Transform uiOverlayRoot;
        public GamePhase currentGamePhase = GamePhase.VisitorWaiting;

        private readonly Dictionary<GamePhase, BaseGameState> gameStates = new Dictionary<GamePhase, BaseGameState>();

        private BaseGameState currentGameState;

        private DataPersistenceManager dataPersistenceManager;
        private GameDataHandler gameData;
        private SoundManager sounds;
        private FadeController fadeController;

        private GameFXManager gameFXManager;

        public event Action<GamePhase> OnChangeState;
        public event Action OnGameStarted;


        [Inject]
        public void Construct(StateFactory factory, DataPersistenceManager persistenceManager, 
            GameDataHandler gameData, GameFXManager fxManager, SoundManager sounds, FadeController fadeController)
        {
            gameStates.Add(GamePhase.VisitorWaiting, factory.CreateVisitorWaitingState());
            gameStates.Add(GamePhase.Visitor, factory.CreateVisitorState());
            gameStates.Add(GamePhase.Night, factory.CreateNightState());
            gameStates.Add(GamePhase.EndGame, factory.CreateEndGameState());
            
            
            dataPersistenceManager = persistenceManager;
            this.gameData = gameData;
            gameFXManager = fxManager;
            this.sounds = sounds;
            this.fadeController = fadeController;
        }

        private void Start()
        {
            RunStateMachine();
        }

        public async void RunStateMachine()
        {
            dataPersistenceManager.LoadDataPersistenceObj();
            currentGameState = gameStates[gameData.gamePhase];
            if (dataPersistenceManager.IsNewGame)
            {
                if (GameLoader.IsMenuOpen())
                {
                    return;
                }
                fadeController.FadeOut(0, 1f).Forget();
                await gameFXManager.ShowStartGame();
            }
            else
            {
                await UniTask.NextFrame(); //to avoid injection problems
                while (GameLoader.IsMenuOpen())
                {
                    await UniTask.NextFrame(); 
                }
            }
            OnGameStarted?.Invoke();
            dataPersistenceManager.SaveGame();
            currentGameState.Enter();
        }
 
        public void SwitchState(GamePhase phase)
        {
            currentGameState.Exit();
            currentGamePhase = phase;
            currentGameState = gameStates[phase];
            gameData.gamePhase = phase;
            currentGameState.Enter();
            OnChangeState?.Invoke(phase);
            dataPersistenceManager.SaveGame();
        }

        public void SwitchToEnding(string tag)
        {
            var night = gameStates[GamePhase.EndGame] as EndGameState;
            night?.SetEnding(tag, uiOverlayRoot);
            gameData.RememberRound();
            SwitchState(GamePhase.EndGame);
        }
    }
}
