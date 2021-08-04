using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnimationEventType
{
    OnPlayAgain,
    OnTipsPopupClosed
}

public class AnimatorEvent : MonoBehaviour
{
    public void OnAnimationEvent(AnimationEventType type)
    {
        GameEvent.Instance.SendEvent(GameEvent.ANIMATOR_EVENT, new AnimatorEventArgs(type));
    }

}
