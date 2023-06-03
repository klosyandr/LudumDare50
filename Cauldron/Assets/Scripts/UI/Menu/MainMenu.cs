using Save;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CauldronCodebase
{
    public class MainMenu : MonoBehaviour
    {
        public Button continueGame;
        public Button quit;
        public Button newGame;
        public Button settings;
        public SettingsMenu settingsMenu;

        [Inject] private DataPersistenceManager dataPersistenceManager;

        private void OnValidate()
        {
            if (!settingsMenu)
                settingsMenu = FindObjectOfType<SettingsMenu>();
        }
        
        private void Start()
        {
            if (!PlayerPrefs.HasKey(FileDataHandler.PrefSaveKey))
            {
                HideContinueButton();
            }
            continueGame.onClick.AddListener(ContinueClick);
            quit.onClick.AddListener(GameLoader.Exit);
            newGame.onClick.AddListener(NewGameClick);
            settings.onClick.AddListener(settingsMenu.Open);
        }

        private void Update()
        {
            //for playtests
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                ResetGameData();
            }
        }

        public void ResetGameData()
        {
            PlayerPrefs.DeleteAll();
            dataPersistenceManager.NewGame();
            HideContinueButton();
            Debug.LogWarning("All data cleared!");
        }

        private void HideContinueButton()
        {
            continueGame.gameObject.SetActive(false);
        }

        private void NewGameClick()
        {
            switch (PlayerPrefs.HasKey(FileDataHandler.PrefSaveKey))
            {
                case true:  // A place to open the menu to confirm the start of a new game
                    Debug.LogWarning("The saved data has been deleted and a new game has been started");
                    StartNewGame();
                    break;
                
                case false:
                    StartNewGame();
                    break;
            }
        }

        private void ContinueClick()
        {
            GameLoader.UnloadMenu();
        }

        private void StartNewGame()
        {
            GameLoader.ReloadGame();
            dataPersistenceManager.NewGame();
        }
    }
}