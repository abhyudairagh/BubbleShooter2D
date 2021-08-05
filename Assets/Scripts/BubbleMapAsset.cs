using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "New BubbleMapAsset", menuName = "BubbleShooter/BubbleMapAsset")]
public class BubbleMapAsset : ScriptableObject
{
    [Serializable]
    public struct BubbleColor
    {
        public BubbleType type;
        public Color color;
    }
    [Serializable]
    public struct BubbleTexture
    {
        public BubbleType type;
        public Sprite sprite;
    }

    public BubbleColor[] bubbleColors;
    public BubbleTexture[] bubbleTextures;
    public Sprite defaultSprite;
    public Dictionary<BubbleType,Color> GetColorMap()
    {
        Dictionary<BubbleType, Color> colorMaps = new Dictionary<BubbleType, Color>();
        foreach (BubbleColor colorMap in bubbleColors)
        {
            if (!colorMaps.ContainsKey(colorMap.type))
            {
                colorMaps.Add(colorMap.type, colorMap.color);
            }
        }
        return colorMaps;
    }
    public Dictionary<BubbleType, Sprite> GetTextureMap()
    {
        Dictionary<BubbleType, Sprite> textureMaps = new Dictionary<BubbleType, Sprite>();
        foreach (BubbleTexture colorMap in bubbleTextures)
        {
            if (!textureMaps.ContainsKey(colorMap.type))
            {
                textureMaps.Add(colorMap.type, colorMap.sprite);
            }
        }
        return textureMaps;
    }

}
