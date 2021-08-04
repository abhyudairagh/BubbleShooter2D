using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AimAssistantTransform
{
    Vector2 direction;
    Vector2 hitPoint;
    public AimAssistantTransform(Vector2 direction, Vector2 hitPoint)
    {
        this.direction = direction;
        this.hitPoint = hitPoint;
    }

    public bool IsPlacingLeft(Vector2 hitCenter)
    {
            return  (hitPoint.x < hitCenter.x) ;
    }

    public bool IsPlacingNear(Vector2 center)
    {
        return (hitPoint.y >= (center.y - 0.06f));
    }

    public Vector2 BubblePostion(float bubbleRadius, Vector2 hitCenter)
    {
        Vector2 position = hitCenter;

        if (IsPlacingNear(hitCenter))
        {
            bubbleRadius *= 2;
        }
        else
        {
            position.y -= BubbleFactory.Instance.BubblePlaceHeight;
        }
        if (IsPlacingLeft(hitCenter))
        {

            position.x -= bubbleRadius;
        }
        else
        {
            position.x += bubbleRadius;
        }
        Bound bound = BubbleFactory.Instance.BorderBounds;
        if(position.x < bound.minx)
        {
            position.x += (bubbleRadius * 2f);
        }
        else if(position.x > bound.maxx)
        {
            position.x -= (bubbleRadius * 2f);
        }

        return position;
    }

}
/// <summary>
/// Class used for Aiming purpose and updating aiming directions/postions
/// </summary>
public class AimAssistant : MonoBehaviour,IAimAssistant
{
    [SerializeField]
    LineRenderer rayRenderer;

    Transform bubbleHelper;

    Vector2 targetPosition;

    public Vector2 TargetPosition
    {
        get
        {
            return targetPosition;
        }
    }

    void Start()
    {
        BubbleShooter shooter = GetComponentInParent<BubbleShooter>();
        if (shooter != null)
        {
            shooter.SetAimAssistant(this);
        }
        bubbleHelper = BubbleFactory.Instance.GetBubbleAssistant();
    }

    public void SetRayPositionCount(int count)
    {
        rayRenderer.positionCount = count;
    }
    public void SetRayPosition(int index, Vector3 position)
    {
        rayRenderer.SetPosition(index, position);

    }
    public void SetRaycolor(Color color)
    {
        rayRenderer.startColor = color;
    }
    /// <summary>
    /// Method to identify the position of aim assistant bubble and to show the aiming trail
    /// </summary>
    /// <param name="hitDir"></param>
    /// <param name="pos"></param>
    /// <param name="hitPoint"></param>
    public void ShowAimAssistant(Vector2 hitDir, Vector2 pos, Vector2 hitPoint)
    {
        AimAssistantTransform aimBubbleTransform = new AimAssistantTransform(hitDir,hitPoint);
        targetPosition = aimBubbleTransform.BubblePostion(BubbleFactory.Instance.BubbleSize * 0.5f, pos);
        ShowAssistant();
    }
    public void ShowAimAssistant(Vector2 position)
    {
        targetPosition = position;
        ShowAssistant();
    }

    void ShowAssistant()
    {
        if (bubbleHelper != null)
        {
            bubbleHelper.position = targetPosition;
        }

        if (!bubbleHelper.gameObject.activeInHierarchy)
        {
            bubbleHelper.gameObject.SetActive(true);
        }
    }

    public void DisableAimAssistant()
    {
        rayRenderer.positionCount = 0;
        if (bubbleHelper.gameObject.activeInHierarchy)
        {
            bubbleHelper.gameObject.SetActive(false);
        }
    }
}

public interface IAimAssistant
{
    Vector2 TargetPosition { get; }
    void SetRayPositionCount(int count);
    void SetRayPosition(int index, Vector3 position);
    void SetRaycolor(Color color);
    void ShowAimAssistant(Vector2 position);
    void ShowAimAssistant(Vector2 hitDir, Vector2 pos, Vector2 hitPoint);
    void DisableAimAssistant();
}