﻿using System.Collections.Generic;
using System;
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

        [HideInInspector] public EndingsProvider.Unlocks currentEnding = EndingsProvider.Unlocks.None;
        [HideInInspector] public GamePhase currentGamePhase = GamePhase.VisitorWaiting;

        private readonly Dictionary<GamePhase, BaseGameState> gameStates = new Dictionary<GamePhase, BaseGameState>();

        private BaseGameState currentGameState;

        private DataPersistenceManager dataPersistenceManager;
        private GameDataHandler gameData;

        private GameFXManager gameFXManager;

        public event Action<GamePhase> OnChangeState;


        [Inject]
        public void Construct(StateFactory factory, DataPersistenceManager persistenceManager, GameDataHandler gameData, GameFXManager fxManager)
        {
            gameStates.Add(GamePhase.VisitorWaiting, factory.CreateVisitorWaitingState());
            gameStates.Add(GamePhase.Visitor, factory.CreateVisitorState());
            gameStates.Add(GamePhase.Night, factory.CreateNightState());
            gameStates.Add(GamePhase.EndGame, factory.CreateEndGameState());
            
            dataPersistenceManager = persistenceManager;
            this.gameData = gameData;
            gameFXManager = fxManager;
        }

        private void Start()
        {
            RunStateMachine();
        }

        public void RunStateMachine()
        {
            gameFXManager.ShowStartGameFX();
            dataPersistenceManager.LoadDataPersistenceObj();
            currentGameState = gameStates[gameData.gamePhase];
            PlayerPrefs.SetInt("FirstTime", 1);
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
    }
}
