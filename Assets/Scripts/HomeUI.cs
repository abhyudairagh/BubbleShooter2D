using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HomeUI : MonoBehaviour
{
    [SerializeField]
    Button playButton;
    [SerializeField]
    string gameSceneName;

    void Start()
    {
        playButton.onClick.AddListener(OnPlayButtonPressed);
    }

    private void OnPlayButtonPressed()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    
}
