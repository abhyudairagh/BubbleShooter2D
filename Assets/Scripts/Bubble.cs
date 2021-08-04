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

/// <summary>
/// Holds information about a single bubble
/// </summary>
public class Bubble : MonoBehaviour,IBubble
{
    [SerializeField]
    private Text text;

    private BubbleType type;

    private float bubbleSize;

    private int targetLayer;
    private int id;

    private bool hasLink = true;
    private bool isShooter;
    private bool unlinkCheck;
    private bool isVisited = false;
    private bool isTypeChecked = false;
    private bool isBreaked;
    private bool hasSideConnection = true;
    private bool hastopConnection = true;


    private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;

    private List<IBubble> sideConnectors = new List<IBubble>();
    private List<IBubble> topConnectors = new List<IBubble>();

    private Action<IBubble> OnDestroyCallback;



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
    public bool HasLink 
    { 
        get 
        { 
            return hasLink; 
        } 
    }
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
    public GameObject GameObject
    {
        get
        {
            return gameObject;
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
        if (destroyCallback != null)
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

    /// <summary>
    /// Method to update the neighbouring side and top links/connections bubbles
    /// </summary>
    /// <param name="sideConnections"></param>
    /// <param name="topConnections"></param>
    /// <param name="hasSideEnding"></param>
    /// <param name="hasTopEnding"></param>
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
    /// <summary>
    /// Recursive check to know if it has aroot connection
    /// </summary>
    /// <returns></returns>
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

 


    /// <summary>
    /// Recursive check to find the connected bubble of same type
    /// </summary>
    /// <param name="similarBubbles"></param>
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
    /// <summary>
    /// Removes the bubble with a popping animation
    /// </summary>
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


 

   
}

public interface IBubble : IComparable
{
    BubbleType Type { get; }
    int Id { get; set; }
    float BubbleSize { get; }
    bool IsShooter { get; set; }
    bool HasLink { get; }
    bool IsBreaked { get; set; }
    bool IsVisited { get; set; }
    bool IsTypeCheked { get; set; }
    bool HasSideConnection { get; }
    bool HasTopConnection { get;  }


    GameObject GameObject { get; }
    void DestroyBubble();
    bool HasRootConnection();
    void Pop();
    void MarkAsRootless();
    void CollectSameBubble(List<IBubble> similarBubbles);
    void SetColliderEnable(bool enable);
    void Initialize(BubbleType type, bool isShooter,Action<IBubble> destroyCallback = null);
    void UpdateConnectors(List<IBubble> sideConnectors, List<IBubble> topConnectors, bool hasSideEnding, bool hasTopEnding);

}

