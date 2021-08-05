using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "New GameConfigAsset", menuName = "BubbleShooter/GameConfigAsset")]
public class GameConfigAsset : ScriptableObject
{
    public float shootingSpeed;
    public int bubbleColumnCount;
    public float bubbleSize;
    public int scorePerBubble;
    public int bubbleAmmoCount;
}
