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

public class BubbleFactory : Singleton<BubbleFactory>
{   
    [SerializeField]
    GameObject bubblePrefab;

    Vector2[] topRowPostions;

    [SerializeField]
    Transform bubblePlacer;
    [SerializeField]
    Color bubblePlacerColor;

    [SerializeField]
    float bubbleSize;
    [SerializeField]
    float scaleOffset;

    [SerializeField]
    BubbleMapAsset bubbleMapAsset;

    float bubblePlaceHeight;
    Bound holderBound;



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
        throw new KeyNotFoundException(type.ToString() + "Type is not found in the Dictionary");
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
        //throw new KeyNotFoundException(type.ToString() + "Type is not found in the Dictionary");
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
            renderer.color = bubblePlacerColor;

            bubble.SetActive(false);
            bubblePlacer = bubble.transform;
        }
        return bubblePlacer;
    }

}
