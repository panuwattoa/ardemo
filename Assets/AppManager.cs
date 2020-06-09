using System;
using System.Collections;
using System.Collections.Generic;
using JoystickLab;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AppManager : SingleToneManager<AppManager>
{
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    // Start is called before the first frame update
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.name.Equals("UXManagerScene"))
            {
                DestroyImmediate(GameObject.Find("exportModels"));
                SceneManager.LoadScene("UXManagerScene");
            }
        }
    }
}
