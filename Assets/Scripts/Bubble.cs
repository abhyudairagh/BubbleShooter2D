using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using DG.Tweening;
public enum BubbleType
{
    Blue,
    Red,
    Green,
    yellow,
    SingleBreaker,
    RowBreaker
}


public class Bubble : MonoBehaviour,IBubble
{
    private bool isShooter;
    private BubbleType type;
    private SpriteRenderer spriteRenderer;
    [SerializeField]
    Text text;
    private bool isBreaked;
    private CircleCollider2D circleCollider;
    bool unlinkCheck;
    float bubbleSize;
    int targetLayer;
    int id;
    bool isVisited = false;
    bool isTypeChecked = false;
    private bool hasLink = true;

    private bool hasSideConnection = true;
    private bool hastopConnection = true;

    List<IBubble> sideConnectors = new List<IBubble>();
    List<IBubble> topConnectors = new List<IBubble>();

    Action<IBubble> OnDestroyCallback;
    public bool IsVisited { 
        get 
        {
            return isVisited;
        }
        set
        { 
            isVisited = value;
        } 
    }

    public bool IsTypeCheked
    {
        get
        {
            return isTypeChecked;
        }
        set
        {
            isTypeChecked = value;
        }
    }

    public int Id
    {
        get
        {
            return id;
        }

        set
        {
            id = value;
            if (text != null)
            {
                text.text = id.ToString();
            }
        }
    }

    public bool IsBreaked
    {
        get
        {
            return isBreaked;
        }
        set
        {
             isBreaked = value;
            if(isBreaked)
            {
                SetColliderEnable(false);
            }
        }
    }

    public void UpdateConnectors(List<IBubble> sideConnections,List<IBubble> topConnections,bool hasSideEnding, bool hasTopEnding)
    {
        sideConnectors.Clear();
        hasSideConnection = hasSideEnding;
        sideConnectors.AddRange(sideConnections);


        topConnectors.Clear();
        hastopConnection = hasTopEnding;
        topConnectors.AddRange(topConnections);
    }

    public void MarkAsRootless()
    {
        if (!isBreaked)
        {
            hasLink = false;
        }
    }

    public bool HasRootConnection()
    {
        isVisited = true;
        if (hastopConnection)
        {
            return true;
        }
        if(!hasLink || isBreaked)
        {
            return false;
        }


            foreach(IBubble connecter in sideConnectors)
            {
                if (!connecter.IsVisited && connecter.HasRootConnection())
                {
                    return true;
                }
            }
      
            foreach (IBubble connecter in topConnectors)
            {
                if (!connecter.IsVisited && connecter.HasRootConnection())
                {
                    return true;
                }
            }

        return false;
    }

    public void Initialize(BubbleType type, bool isShooter, Action<IBubble> destroyCallback = null)
    {
        this.type = type;
        this.isShooter = isShooter;
                targetLayer = 1 << gameObject.layer;
        spriteRenderer = GetComponent<SpriteRenderer>();
       
        if (circleCollider == null)
        {
            InitializeBoxCollider();
        }
        if(destroyCallback!=null)
        {
            OnDestroyCallback = destroyCallback;
        }

        spriteRenderer.sprite = BubbleFactory.Instance.GetSprite(type);
    }


    private void Start()
    {
        bubbleSize = spriteRenderer.bounds.size.x;

        UpdateRenderer();
    }

    void InitializeBoxCollider()
    {
        circleCollider = GetComponent<CircleCollider2D>();
    }

    public void SetColliderEnable(bool enable)
    {
        if(circleCollider == null)
        {
            InitializeBoxCollider();
        }
        circleCollider.enabled = enable;
    }

    private void BlendUpdateRenderer()
    {
        UpdateRenderer();
        spriteRenderer.sprite = BubbleFactory.Instance.GetSprite(type);
    }
    private void UpdateRenderer()
    {
        spriteRenderer.color = BubbleFactory.Instance.GetColor(type);
    }

   

    public float BubbleSize
    {
        get
        {
            return bubbleSize;
        }
    }
    public bool IsShooter
    {
        get
        {
            return isShooter;
        }
        set
        {
            isShooter = value;
        }
    }
    public bool HasSideConnection
    {
        get
        {
            return hasSideConnection;
        }
    }
    public bool HasTopConnection
    {
        get
        {
            return hastopConnection;
        }
    }
    public BubbleType Type
    {
        get
        {
            return type;
        }
    
    }
    public bool HasLink { get { return hasLink; } }


    public void CollectSameBubble(List<IBubble> similarBubbles)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, (BubbleSize * 1.1f), 1 << LayerMask.NameToLayer("Bubble"));
        foreach (Collider2D collider in colliders)
        {
            IBubble collidedBubble = collider.GetComponent<IBubble>();

            if (collidedBubble != null)
            {
                if (type == BubbleType.SingleBreaker)
                {
                    type = collidedBubble.Type;
                    BlendUpdateRenderer();
                    //similarBubbles.Add(this);
                }

                if (collidedBubble.IsTypeCheked)
                    continue;

                

                if (collidedBubble == (IBubble)this)
                {
                    continue;
                }



                

                if (collidedBubble.Type == type)
                {
                    similarBubbles.Add(collidedBubble);
                    collidedBubble.IsTypeCheked = true;
                    collidedBubble.CollectSameBubble(similarBubbles); 
                }

            }
        }
    }

    IEnumerator RemoveBubble()
    {
        yield return null;
        if (this != null)
        {
           
                OnDestroyCallback?.Invoke(this);
                Destroy(this.gameObject);
           
        }
    }
    public void Pop()
    {
        if (this != null)
        {
            Vector3 scale = transform.localScale.normalized;
            gameObject.transform.DOBlendableScaleBy(scale * 0.1f, 0.1f).OnComplete(DestroyBubble);

        }
    }
    
    public void DestroyBubble()
    {
       // gameObject.transform.DOKill();
        StartCoroutine(RemoveBubble());
    }

    public int CompareTo(object obj)
    {
        if (obj == null) return 1;

        IBubble bubble = obj as IBubble;
        Vector2 objPosition = bubble.GameObject.transform.position;
        Vector2 myPosition = transform.position;
        if (objPosition.x != myPosition.x) return myPosition.y.CompareTo(objPosition.y); 
        else  return myPosition.x.CompareTo(objPosition.x);
    }

    public GameObject GameObject
    {
        get
        {
            return gameObject;
        }
    }

 

   
}

public interface IBubble : IComparable
{
    int Id { get; set; }

    bool IsShooter { get; set; }
    bool HasLink { get; }
    bool IsBreaked { get; set; }
    bool IsVisited { get; set; }
    bool IsTypeCheked { get; set; }
    bool HasRootConnection();
    bool HasSideConnection { get; }
    bool HasTopConnection { get;  }
    BubbleType Type { get; }
    float BubbleSize { get; }
    GameObject GameObject { get; }
    void CollectSameBubble(List<IBubble> similarBubbles);
    void SetColliderEnable(bool enable);
    void Initialize(BubbleType type, bool isShooter,Action<IBubble> destroyCallback = null);
    void UpdateConnectors(List<IBubble> sideConnectors, List<IBubble> topConnectors, bool hasSideEnding, bool hasTopEnding);
    void DestroyBubble();

    void Pop();
    void MarkAsRootless();
}

