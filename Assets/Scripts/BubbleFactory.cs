using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public struct Bound
{
    public float minx;
    public float maxx;
    public float miny;
    public float maxy;
    public Bound(float minx, float maxx, float miny, float maxy)
    {
        this.minx = minx;
        this.maxx = maxx;
        this.miny = miny;
        this.maxy = maxy;
    }
}

/// <summary>
/// Used to create bubbles and get informations related to bubble
/// </summary>
public class BubbleFactory : Singleton<BubbleFactory>
{   
    [SerializeField]
    GameObject bubblePrefab;

    [SerializeField]
    Color bubblePlacerColor;

    [SerializeField]
    float bubbleSize;
    [SerializeField]
    float scaleOffset;

    [Tooltip("Add BubbleMapAsset to get the color/texture information for bubbles")]
    [SerializeField]
    BubbleMapAsset bubbleMapAsset;

    float bubblePlaceHeight;

    Bound holderBound;

    Vector2[] topRowPostions;

    Transform bubblePlacer;


    Dictionary<BubbleType, Color> colorMap;
    Dictionary<BubbleType, Sprite> textureMap;
    public float BubbleSize
    {
        get
        {
            return bubbleSize;
        }
    }
    public float ScaleOffset
    {
        get
        {
            return scaleOffset;
        }
    }
    public float BubblePlaceHeight
    {
        get
        {
            return bubblePlaceHeight;
        }
    }

    public Vector2[] TopRowPostions
    {
        get
        {
            return topRowPostions;
        }
    }

    public Bound BorderBounds
    {
        get
        {
            return holderBound;
        }
        set
        {
            holderBound = value;
        }
    }
    public Color GetColor(BubbleType type)
    {
       if(colorMap.ContainsKey(type))
        {
            return colorMap[type];
        }
        else
        {
            return Color.white;
        }
    }
    public Sprite GetSprite(BubbleType type)
    {
        if (textureMap.ContainsKey(type))
        {
            return textureMap[type];
        }
        else
        {
            return bubbleMapAsset.defaultSprite;
        }
    }
    protected override void Init()
    {
        bubblePlaceHeight = bubbleSize * 0.866f;
        colorMap = bubbleMapAsset.GetColorMap();
        textureMap = bubbleMapAsset.GetTextureMap();
    }

    public void UpdateTopRowPostions(Vector2[] topBubblePostions)
    {
        topRowPostions = topBubblePostions;
    }

    /// <summary>
    /// Instantiator which returns generic component type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    /// <param name="position"></param>
    /// <param name="destroyCallback"></param>
    /// <param name="isShooter"></param>
    /// <returns></returns>
    public T CreateBubble<T>(BubbleType type, Vector2 position, Action<IBubble> destroyCallback = null ,bool isShooter = false) where T : class
    {
        GameObject bubble = Instantiate(bubblePrefab, position, Quaternion.identity);
        bubble.GetComponent<IBubble>().Initialize(type, isShooter, destroyCallback);
        bubble.transform.localScale = Vector3.one * bubbleSize * scaleOffset;
        if (typeof(T) == typeof(GameObject))
        {
            return bubble as T;
        }
        else
        {
            return bubble.GetComponent<T>();
        }

    }

    /// <summary>
    /// Returns the transforms of the assistant bubble(Placeholder bubble to show the shooted bubble's position)
    /// </summary>
    /// <returns></returns>
    public Transform GetBubbleAssistant()
    {
        if(bubblePlacer == null)
        {
            GameObject bubble = Instantiate(bubblePrefab, Vector3.zero, Quaternion.identity);
            bubble.transform.localScale = Vector3.one * bubbleSize * scaleOffset;

            Bubble script = bubble.GetComponent<Bubble>();
            CircleCollider2D colldier = bubble.GetComponent<CircleCollider2D>();

            SpriteRenderer renderer = bubble.GetComponent<SpriteRenderer>();
            renderer.sortingLayerID = SortingLayer.NameToID("OnTop");
            Destroy(script);
            Destroy(colldier);

            foreach(Transform child in bubble.transform)
            {
                Destroy(child.gameObject);
            }

            renderer.color = bubblePlacerColor;

            bubble.SetActive(false);
            bubblePlacer = bubble.transform;
        }
        return bubblePlacer;
    }

}
