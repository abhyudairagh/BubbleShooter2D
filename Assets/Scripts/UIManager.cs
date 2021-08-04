using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour, IManagerDependencies
{
    [Header("Gameplay UI")]
    [SerializeField]
    TextMeshProUGUI ammoCountText;
    [SerializeField]
    TextMeshProUGUI scoreCount;
    [SerializeField]
    Button homeButton;

    [Header("GameOver UI")]
    [SerializeField]
    GameObject gameOverScreen;
    [SerializeField]
    TextMeshProUGUI g_ScoreCount;
    [SerializeField]
    TextMeshProUGUI g_WinLoseText;
    [SerializeField]
    Button playAgainButton;
    [SerializeField]
    Button g_homeButton;

    [SerializeField]
    Button resetButton;
    IGameManager _gameManager;
    public void Init(IGameManager manager)
    {
        _gameManager = manager;
        resetButton.onClick.AddListener(OnGameResetClicked);
        homeButton.onClick.AddListener(OnHomeClicked);
        playAgainButton.onClick.AddListener(OnPlayAgainClicked);
        g_homeButton.onClick.AddListener(OnHomeClicked);
    }

    private void OnPlayAgainClicked()
    {
        gameOverScreen.SetActive(false);
        GameEvent.Instance.SendEvent(GameEvent.GAMERESET_EVENT, new EventArgs());
    }

    private void OnGameResetClicked()
    {
        GameEvent.Instance.SendEvent(GameEvent.GAMERESET_EVENT, new EventArgs());
    }
    private void OnHomeClicked()
    {
        GameEvent.Instance.SendEvent(GameEvent.HOMEPRESSED_EVENT, new EventArgs());
    }
    private void OnEnable()
    {
        GameEvent.Instance.RegisterEvent(GameEvent.GAMEOVER_EVENT, OnGameOverEvent);
        GameEvent.Instance.RegisterEvent(GameEvent.GAMESCORE_EVENT, OnGameEvent);
    }

    private void OnGameOverEvent(IEventArgs obj)
    {
        if(obj != null)
        {
            gameOverScreen.SetActive(true);

            IGameOverEventArgs eventArgs = obj as IGameOverEventArgs;
            string text = (eventArgs.IsWin) ? "You Win" : "You Lost";
            g_WinLoseText.text = text;
            g_ScoreCount.text = eventArgs.Score.ToString();
        }
    }
    private void OnGameEvent(IEventArgs obj)
    {
        IGameEventArgs gameData = obj as IGameEventArgs;
        scoreCount.text = gameData.Score.ToString();
        ammoCountText.text = gameData.RemainingBubbles.ToString();

    }

    private void OnDisable()
    {
        GameEvent.Instance.UnRegisterEvent(GameEvent.GAMEOVER_EVENT, OnGameOverEvent);
        GameEvent.Instance.UnRegisterEvent(GameEvent.GAMESCORE_EVENT, OnGameEvent);
    }

  


}
