using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening;
using UnityEngine.EventSystems;

/// <summary>
/// Class for shooting the bubble
/// </summary>
public class BubbleShooter : MonoBehaviour,IManagerDependencies
{ 
    [SerializeField]
    Transform spawnPoint;


    [Header("Mouse Interaction")]
    [SerializeField]
    float dragAimOffset;
    [SerializeField]
    float minTiltAngle;

    float maxTiltAngle;
    float shootSpeed;

    int bubbleCount;

    bool shooterLoaded;
    bool isMousePressed;
    bool isTargetFound;

    Vector2 aimDirection;
    Vector2 bubbleTarget;
    List<Vector2> moveLocations;

    LayerMask wallLayer;
    [SerializeField]
    LayerMask bubbleLayer;
    LayerMask topLayer;

    Camera camera;
   
    IBubble _throwBubble;
    IAimAssistant _aimAssistant;
    IGameManager _gameManager;
    private void OnEnable()
    {
        GameEvent.Instance.RegisterEvent(GameEvent.GAMEOVER_EVENT, OnGameOver);
        GameEvent.Instance.RegisterEvent(GameEvent.GAMERESET_EVENT, OnGameReset);
    }

    private void OnGameOver(IEventArgs obj)
    {
        shooterLoaded = false;
        if((Bubble)_throwBubble != null)
        {
            _throwBubble.DestroyBubble();
        }
    }

    private void OnGameReset(IEventArgs args)
    {
        bubbleCount = _gameManager.TotalAmmo;
        if ((Bubble)_throwBubble != null)
        {
            _throwBubble.DestroyBubble();
        }
        CreateShooterBubble();
    }

    private void OnDisable()
    {
        GameEvent.Instance.UnRegisterEvent(GameEvent.GAMEOVER_EVENT, OnGameOver);
        GameEvent.Instance.RegisterEvent(GameEvent.GAMERESET_EVENT, OnGameReset);
    }

    public void Init(IGameManager manager)
    {
        _gameManager = manager;
        InitializeVariables();
        CreateShooterBubble();


        shootSpeed = 1f / (_gameManager.ShootingSpeed * 0.5f);


    }

    void InitializeVariables()
    {
        camera = Camera.main;
        maxTiltAngle = (180 - minTiltAngle);
        bubbleCount = _gameManager.TotalAmmo ;
        wallLayer =  1 << LayerMask.NameToLayer("Wall");
        bubbleLayer = 1 << LayerMask.NameToLayer("Bubble") | 1 << LayerMask.NameToLayer("RayBlock");
        topLayer = 1 << LayerMask.NameToLayer("Top");

    }

    public void SetAimAssistant(IAimAssistant aimAssistant)
    {
        _aimAssistant = aimAssistant;
        _aimAssistant.SetRaycolor(BubbleFactory.Instance.GetColor(_throwBubble.Type));
    }

    private void CreateShooterBubble()
    {
        if (bubbleCount > 0)
        {
            shooterLoaded = true;
            int rand = Random.Range(0, 6);
            GameObject throwBubble = BubbleFactory.Instance.CreateBubble<GameObject>((BubbleType)rand, spawnPoint.position);

            _throwBubble = throwBubble.GetComponent<IBubble>();
            _throwBubble.IsShooter = true;
            _throwBubble.SetColliderEnable(false);
            _aimAssistant?.SetRaycolor(BubbleFactory.Instance.GetColor(_throwBubble.Type));
           

            bubbleCount--;
            throwBubble.transform.localScale = Vector3.one * BubbleFactory.Instance.BubbleSize * BubbleFactory.Instance.ScaleOffset;
        }
    }
    /// <summary>
    /// Method to fire a bubble once aimed
    /// </summary>
    private void fireBubble()
    {
        if (isTargetFound)
        {
            
            isTargetFound = false;
            shooterLoaded = false;



            DisableAim();
          

             moveLocations[moveLocations.Count - 1] = bubbleTarget;
            _throwBubble.SetColliderEnable(true);

            Sequence shotSequence = DOTween.Sequence();
            for (int i = 1; i < moveLocations.Count; i++)
            {
                shotSequence.Append(_throwBubble.GameObject.transform.DOMove(moveLocations[i], shootSpeed));
            }
           
            shotSequence.Play().OnComplete(() =>
            {
               
                    _throwBubble.IsShooter = false;
                    _gameManager.AddBubbleToList(_throwBubble);
                    _gameManager.OnBubbleThrow(_throwBubble, bubbleCount);
                     CreateShooterBubble();

            });
        }
    }

    

    private void OnMouseDown()
    {
        isMousePressed = true;
    }

    private void OnMouseUp()
    {
        isMousePressed = false;
        fireBubble();
    }

    private void Update()
    {
        isTargetFound = false;
        if (!shooterLoaded)
            return;

        if (isMousePressed)
        {
            Vector2 screenPoint = camera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 transformPos = transform.position;
            Vector2 direction = transformPos - screenPoint;

            float diffInMousePoint = direction.sqrMagnitude;
            if(diffInMousePoint > (dragAimOffset * dragAimOffset))
            {
                Vector2 rotDir = direction.normalized;
                float rot_z = Mathf.Atan2(rotDir.y, rotDir.x) * Mathf.Rad2Deg;
                if (rot_z < maxTiltAngle && rot_z > minTiltAngle)
                {
                     aimDirection = direction;
                }
                FindHitTarget(aimDirection);
                return;
            }
        }
        DisableAim();

    }

    void DisableAim()
    {
        _aimAssistant?.DisableAimAssistant();
        
    }

    /// <summary>
    /// Method to find the final location of the bubble that is shot towards a direction
    /// </summary>
    /// <param name="direction"></param>
    void FindHitTarget(Vector2 direction)
    {
        
        if (PhysicsRayCaster2D.AimRayCast2D(transform.position, direction, 100f,out bool isTopHit ,out RaycastHit2D hitCollider, out List<Vector2> segments, wallLayer, bubbleLayer, topLayer ))
        {
            moveLocations = segments;
            _aimAssistant.SetRayPositionCount(segments.Count);
            for (int i= 0; i< segments.Count ; i++)
            {
                _aimAssistant.SetRayPosition(i,segments[i]);
            }
             
             isTargetFound = true;

            if (!isTopHit)
            {
                Vector2 hitDir = ((Vector2)hitCollider.collider.transform.position - segments[segments.Count - 2]).normalized;
                _aimAssistant?.ShowAimAssistant(hitDir, hitCollider.collider.transform.position, hitCollider.point);
            }
            else
            {
                _aimAssistant?.ShowAimAssistant(segments[segments.Count - 1]);
            }
            bubbleTarget = _aimAssistant.TargetPosition;
        }
        else
        {
            DisableAim();
        }

    }

   
}
