using System;
using System.IO;
using UnityEngine;

[Serializable]
public class GameSaveData
{
    public string SaveVersion = "1.0.0";
    public string PlayerName = "Player";
    public DateTime LastSavedTimestamp;
    public float TotalPlaytimeSeconds = 0f;
    public int CurrentGame = 0;
    //0 = intro, 1 = Devils Checkmate, and so on


    public DevilsCheckmate Game1 = new();
    public MyDarkFantasy Game2 = new();
    public Endgame Game3 = new();
    public AnarchyChess Game4 = new();
    public TurboGrafx16 Game5 = new();
    public WAR Game6 = new();
    public ComeToLife Game7 = new();
    public SeeMeAgain FinalGame = new();
}

[Serializable]
public class KingsGambit
{
    public bool IsCompleted = false;
    public bool hasWon = false;
    public bool secretEndingUnlocked = false;
}

[Serializable]
public class MyDarkFantasy
{
    public bool IsCompleted = false;
    public int act = 0;
    public bool twistedEndingUnlocked = false;
    public bool secretEndingUnlocked = false;

}

[Serializable]
public class Endgame
{
    public bool IsCompleted = false;
    public int currentStage = 0;
    public int Scene = 0;
    public bool secretEndingUnlocked = false;
}

[Serializable]
public class AnarchyChess
{
    public bool IsCompleted = false;
    public int minutesSpent = 0;
    public bool secretEndingUnlocked = false;
}

[Serializable]
public class TurboGrafx16
{
    public bool IsCompleted = false;
    public int TotalBossesDefeated = 0;
    public float SkillTreeProgress = 0f;
    public bool secretEndingUnlocked = false;
}

[Serializable]
public class WAR
{
    public bool IsCompleted = false;
    public int Casualties = 0;
    public int Stage = 0;
    public bool dontFearTheReaper = false;
    public bool secretEndingUnlocked = false;
}

[Serializable]
public class ComeToLife
{
    public bool IsCompleted = false;
    public int scene = 0;
    public Vector3 PlayerPosition = Vector3.zero; 
    public Quaternion PlayerRotation = Quaternion.identity;
    public bool secretEndingUnlocked = false;
}

[Serializable]
public class SeeMeAgain
{
    //0 = Prologue, 1 = Chapter 1, 2 = Chapter 2, etc. 8 = Never See Me Again
    public int CurrentChapter = 0;
    public int currentDay = 0;
    public bool isNight = true;
    public Vector3 PlayerPosition = Vector3.zero; //Pls edit
    public int CurrentHealth = 100; //Dont even need
    public int MaxHealth = 100;
    public int Currency = 0; //same
    public int CurrentConversationIndex = 0; 
    public bool specialEndingUnlocked = false;
}

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    public GameSaveData CurrentSaveDataDataData { get; private set; } = new GameSaveData();

    private string SaveFilePath => Path.Combine(Application.persistentDataPath, "SaveFile.json");

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void SaveGame()
    {
        try
        {
            CurrentSaveDataDataData.LastSavedTimestamp = DateTime.UtcNow;

            string json = JsonUtility.ToJson(CurrentSaveDataDataData, true);
            File.WriteAllText(SaveFilePath, json);

            Debug.Log($"[SaveSystem] Game successfully saved to: {SaveFilePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveSystem] Failed to save game: {ex.Message}");
        }
    }
    public bool LoadGame()
    {
        try
        {
            if (!File.Exists(SaveFilePath))
            {
                Debug.LogWarning("[SaveSystem] Save file not found. Creating a new default save.");
                CurrentSaveDataDataData = new GameSaveData();
                SaveGame();
                return true;
            }

            string json = File.ReadAllText(SaveFilePath);
            CurrentSaveDataDataData = JsonUtility.FromJson<GameSaveData>(json);

            Debug.Log("[SaveSystem] Save file successfully loaded.");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveSystem] Failed to load save file: {ex.Message}");
            return false;
        }
    }

    public void DeleteSaveFile()
    {
        if (File.Exists(SaveFilePath))
        {
            File.Delete(SaveFilePath);
            CurrentSaveDataDataData = new GameSaveData();
            Debug.Log("[SaveSystem] Save file deleted.");
        }
    }
}