using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private int index=0;
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
}
