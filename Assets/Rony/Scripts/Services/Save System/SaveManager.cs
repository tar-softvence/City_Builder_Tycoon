using UnityEngine;
using UnityEngine.SceneManagement; // Required for Scene Index
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour, ISaveService
{

    public static SaveManager Instance { get; private set; }

    private string _saveFilePath;

    // Holds the data for ALL cities (scenes) currently loaded from disk
    public GameSaveData GlobalSaveData { get; private set; } = new GameSaveData();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _saveFilePath = Path.Combine(Application.persistentDataPath, "gamedata.json");

        // Load immediately on startup so data is ready for the first scene
        LoadGame();
    }

    public bool HasSaveFile() => File.Exists(_saveFilePath);

    public void SaveGame()
    {
        // 1. Identify which City (Scene) we are in
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // 2. Prepare the data container for THIS city
        CitySaveData cityData = new CitySaveData();

        // 3. Gather Data from Services
        var landDb = LandService.Instance.GetAllLandData();
        var buildDb = BuildingService.Instance.GetAllBuildings();

        foreach (var kvp in landDb)
        {
            string plotID = kvp.Key;
            LandData liveLand = kvp.Value;

            LandSaveData landSave = new LandSaveData
            {
                PlotID = plotID,
                IsOwned = liveLand.IsOwned,
                Building = null
            };

            // Add Building Data if present
            if (buildDb.ContainsKey(plotID))
            {
                BuildingData liveBuild = buildDb[plotID];
                landSave.Building = new BuildingSaveData
                {
                    Level = liveBuild.Level,
                    CurrentTenants = liveBuild.CurrentTenants,
                    StoredIncome = liveBuild.StoredIncome,
                    LocalMultiplier = liveBuild.LocalMultiplier,
                    BoostTimeRemaining = liveBuild.BoostTimeRemaining,

                    TenantManagerData = liveBuild.TenantManagerData
                };

            }

            cityData.Lands.Add(plotID, landSave);
        }

        // 4. Update the Global State (Merge Logic)
        if (GlobalSaveData.Cities.ContainsKey(currentSceneIndex))
        {
            GlobalSaveData.Cities[currentSceneIndex] = cityData; // Overwrite existing city data
        }
        else
        {
            GlobalSaveData.Cities.Add(currentSceneIndex, cityData); // Add new city data
        }

        // 5. Write to Disk
        string json = JsonConvert.SerializeObject(GlobalSaveData, Formatting.Indented);
        File.WriteAllText(_saveFilePath, json);

        Debug.Log($"[SaveManager] Saved city data for Scene Index {currentSceneIndex}.  in {_saveFilePath}");
    }



    public void LoadGame()
    {
        if (!HasSaveFile()) return;

        try
        {
            string json = File.ReadAllText(_saveFilePath);
            GlobalSaveData = JsonConvert.DeserializeObject<GameSaveData>(json) ?? new GameSaveData();
            Debug.Log("[SaveManager] Global save data loaded.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to load save: {e.Message}");
            GlobalSaveData = new GameSaveData(); // Recover with empty data
        }
    }

    public void ResetGame()
    {
        if (File.Exists(_saveFilePath)) File.Delete(_saveFilePath);
        GlobalSaveData = new GameSaveData();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Helper for LandService to get ONLY relevant data
    public CitySaveData GetCurrentCityData()
    {
        int idx = SceneManager.GetActiveScene().buildIndex;
        if (GlobalSaveData.Cities.TryGetValue(idx, out var data))
        {
            return data;
        }
        return null;
    }


}
