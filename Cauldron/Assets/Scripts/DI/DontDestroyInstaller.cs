using EasyLoc;
using Save;
using UnityEngine;
using Zenject;

namespace CauldronCodebase
{
    public class DontDestroyInstaller : MonoInstaller
    {
        [SerializeField] private GameObject mainCamera;
        [SerializeField] private MainSettings mainSettings;
        [SerializeField] private GameObject dataPersistenceManager;
        [SerializeField] private SODictionary soDictionary;
        [SerializeField] private CatTipsProvider catTipsProvider;

        [SerializeField] private SoundManager soundManager;
        [SerializeField] private FadeController fadeController;
        public override void InstallBindings()
        {
            Camera mainCameraScript = Container.InstantiatePrefab(mainCamera).GetComponent<Camera>();
            Container.Bind<Camera>().FromInstance(mainCameraScript).AsSingle();
            Container.Bind<CatTipsProvider>().FromInstance(catTipsProvider).AsSingle();
            Container.Bind<SODictionary>().FromInstance(soDictionary).AsSingle();
            soDictionary.LoadDictionary();

            Container.Bind<MainSettings>().FromInstance(mainSettings).AsSingle().NonLazy();
            
            Container.Bind<DataPersistenceManager>().FromComponentInNewPrefab(dataPersistenceManager).AsSingle().NonLazy();
            Container.Bind<SoundManager>().FromInstance(soundManager).NonLazy();
            Container.Bind<FadeController>().FromComponentInNewPrefab(fadeController).AsSingle();
            Container.Bind<LocalizationTool>().FromNew().AsSingle();
            
            SetInitialResolution();
        }

        private static void SetInitialResolution()
        {
            bool fullscreenMode = true;
            if (PlayerPrefs.HasKey(PrefKeys.FullscreenModeSettings))
            {
                fullscreenMode = PlayerPrefs.GetInt(PrefKeys.FullscreenModeSettings) == 1;
            }

            if (PlayerPrefs.HasKey(PrefKeys.ResolutionSettings))
            {
                var resolutions = Screen.resolutions;
                int newResolution = PlayerPrefs.GetInt(PrefKeys.ResolutionSettings);
                Screen.SetResolution(resolutions[newResolution].width, resolutions[newResolution].height, fullscreenMode);
            }
            else
            {
                Screen.SetResolution(1920, 1080, fullscreenMode);
            }
        }
    }
}