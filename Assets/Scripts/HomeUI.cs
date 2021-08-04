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
    Button tipsButton;
    [SerializeField]
    Button closeTipsPopup;

    [SerializeField]
    GameObject tipsPanel;

    [SerializeField]
    Animator homeAnimator;

    [SerializeField]
    string gameSceneName;

    private void OnEnable()
    {
        GameEvent.Instance.RegisterEvent(GameEvent.ANIMATOR_EVENT, OnAnimatorEvent);
    }

    private void OnAnimatorEvent(IEventArgs obj)
    {
        if (obj != null)
        {
            IAnimatorEventArgs eventArgs = obj as IAnimatorEventArgs;

               switch (eventArgs.EventType)
                {
                    case AnimationEventType.OnTipsPopupClosed:
                    tipsPanel.SetActive(false);
                        break;
                }
        }
    }

    private void OnDisable()
    {
        GameEvent.Instance.UnRegisterEvent(GameEvent.ANIMATOR_EVENT, OnAnimatorEvent);
    }

    void Start()
    {
        playButton.onClick.AddListener(OnPlayButtonPressed);
        tipsButton.onClick.AddListener(OnTipsPressed);
        closeTipsPopup.onClick.AddListener(OnTipsClosePressed);
    }

    private void OnTipsClosePressed()
    {
        homeAnimator.SetTrigger("CloseTipPopUp");
    }

    private void OnTipsPressed()
    {
        tipsPanel.SetActive(true);
        homeAnimator.SetTrigger("ShowTips");
    }

    private void OnPlayButtonPressed()
    {
        SceneManager.LoadScene(gameSceneName);
    }


    
}
