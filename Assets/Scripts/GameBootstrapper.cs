using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Auto-bootstrapper that creates the LevelBuilder and GameUIManager automatically when the game starts or changes scenes.
/// Also sets up the MainMenu scene with MainMenuUI.
/// </summary>
public static class GameBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        // Register to scene load event so we can setup UI/Level every time a scene loads (e.g. on Restart)
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;

        // MainMenu scene — setup menu UI
        if (sceneName == "MainMenu")
        {
            if (Object.FindFirstObjectByType<MainMenuUI>() == null)
            {
                GameObject menuObj = new GameObject("_MainMenuUI");
                menuObj.AddComponent<MainMenuUI>();
                Debug.Log("[GameBootstrapper] MainMenuUI auto-created.");
            }
            return;
        }

        // MiniGame scene — setup gameplay enhancements
        if (Object.FindFirstObjectByType<PlayerController>() == null)
            return;

        // Create LevelBuilder (visual enhancements, level geometry)
        if (Object.FindFirstObjectByType<LevelBuilder>() == null)
        {
            GameObject builderObj = new GameObject("_LevelBuilder");
            builderObj.AddComponent<LevelBuilder>();
        }

        // Create GameUIManager (HUD + End Game popup)
        if (GameUIManager.Instance == null)
        {
            GameObject uiObj = new GameObject("_GameUIManager");
            uiObj.AddComponent<GameUIManager>();
        }

        Debug.Log("[GameBootstrapper] LevelBuilder + GameUIManager auto-created for scene: " + sceneName);
    }
}
