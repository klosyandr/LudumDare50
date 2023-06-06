using System.Collections;
using Cysharp.Threading.Tasks;
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
        
        [Header("Settings")]
        public Button settings;
        public SettingsMenu settingsMenu;
        
        
        [Header("Authors")]
        [SerializeField] private AuthorsMenu authorsMenu;
        [SerializeField] private Button authorsButton;
        [SerializeField] private GameObject authorsPanel;

        [Header("Fade In Out")]
        [SerializeField] [Inject] private FadeController fadeController;
        [SerializeField] [Tooltip("Fade in seconds")] private float fadeNewGameDuration;
        
        [Inject] private DataPersistenceManager dataPersistenceManager;


        private void OnValidate()
        {
            if (!authorsMenu) authorsMenu = FindObjectOfType<AuthorsMenu>(true);
            if (!settingsMenu) settingsMenu = FindObjectOfType<SettingsMenu>();
            if (!authorsButton) authorsButton = GameObject.Find("AuthorsButton").GetComponent<Button>();
            if (!authorsPanel) authorsPanel = GameObject.Find("Authors_panel");
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
            authorsButton.onClick.AddListener(authorsMenu.Open);
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

        private async void NewGameClick()
        {
            await fadeController.FadeIn(duration: fadeNewGameDuration );
            switch (PlayerPrefs.HasKey(FileDataHandler.PrefSaveKey))
            {
                case true: 
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

        private async void StartNewGame()
        {
            await fadeController.FadeIn(duration: fadeNewGameDuration);
            GameLoader.ReloadGame();
            dataPersistenceManager.NewGame();
        }
        
        
        
        

    }
}