using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private int index=0; // 0 = intro, 1 = Devils Checkmate, and so on
    private GameSaveData saveData;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSaveFile();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        Application.targetFrameRate = 60;
        Application.runInBackground = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }
    void Updatee()
    {
        
    }
    private void LoadSaveFile()
    {
        saveData = SaveSystem.Instance.CurrentSaveDataDataData;
        index = saveData.CurrentGame;
    }
    private void SaveCurrentSaveFile()
    {
        SaveSystem.Instance.SaveGame();
    }
    private void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }


}
