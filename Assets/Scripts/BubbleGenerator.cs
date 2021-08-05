using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used for generating starting bubbles 
/// </summary>
public class BubbleGenerator : MonoBehaviour, IManagerDependencies
{
    [SerializeField]
    SpriteRenderer bubbleHolder;
    [SerializeField]
    SpriteRenderer bubbleBase;


    [SerializeField]
    BoxCollider2D leftWall;
    [SerializeField]
    BoxCollider2D rightWall;


    int bubblecolumnCount;

    IGameManager _gameManager;
    private void OnEnable()
    {
        GameEvent.Instance.RegisterEvent(GameEvent.GAMERESET_EVENT, OnGameReset);
    }

    private void OnGameReset(IEventArgs args)
    {
        InstantiateBubbles();
    }

    private void OnDisable()
    {
        GameEvent.Instance.UnRegisterEvent(GameEvent.GAMERESET_EVENT, OnGameReset);

    }
    public void Init(IGameManager manager)
    {
        _gameManager = manager;
        if (bubbleHolder == null)
        {
            bubbleHolder = GetComponentInChildren<SpriteRenderer>();
        }
        bubblecolumnCount = _gameManager.TotalColumnCount;
        InstantiateBubbles();
    }
    /// <summary>
    /// Create a set of bubbles and place it in the game screen
    /// </summary>
    void InstantiateBubbles()
    {
        float bubbleSize = BubbleFactory.Instance.BubbleSize;
        float bubbleRadius = (bubbleSize * 0.5f);
        float platformWidth = bubbleHolder.bounds.size.x;
        float platformHeight = bubbleHolder.bounds.size.y;

        int numberOfBubblesRow = (int)(platformWidth / bubbleSize);

        float bubblesTotalWidth = numberOfBubblesRow * bubbleSize;
        float paddingX = (platformWidth - bubblesTotalWidth) * 0.5f;



        float initBubbleX = bubbleHolder.transform.position.x - (platformWidth * 0.5f) + (bubbleRadius + paddingX);
        float initBubbleY = bubbleHolder.transform.position.y - (platformHeight * 0.5f) - bubbleRadius;

        float lastBubbleX = bubbleHolder.transform.position.x + (platformWidth * 0.5f) - (bubbleRadius + paddingX);

        BubbleFactory.Instance.BorderBounds = new Bound(initBubbleX, lastBubbleX, bubbleBase.transform.position.y, bubbleHolder.transform.position.y);
        float bubbleX = initBubbleX;

        Vector2 leftwallPos = leftWall.transform.position;
        float l_wallx = initBubbleX  - (leftWall.bounds.size.x * 0.5f);
        leftwallPos.x = l_wallx;
        leftWall.transform.position = leftwallPos;

        Vector2 rightwallPos = rightWall.transform.position;
        float r_wallx = lastBubbleX  + (rightWall.bounds.size.x * 0.5f);
        rightwallPos.x = r_wallx;
        rightWall.transform.position = rightwallPos;

        Vector2[] topRowPostions = new Vector2[numberOfBubblesRow];

        for (int j = 0; j < bubblecolumnCount; j++)
        {
            float bubbleRowCount = 0;
            if (j % 2 == 0)
            {
                bubbleX = initBubbleX;
                bubbleRowCount = numberOfBubblesRow;
            }
            else
            {
                bubbleX = initBubbleX + bubbleRadius;
                bubbleRowCount = numberOfBubblesRow - 1;
            }
            for (int i = 0; i < bubbleRowCount; i++)
            {
                if (j == 0)
                {
                    topRowPostions[i] = new Vector2(bubbleX, initBubbleY);
                }

                int rand = Random.Range(0, 4);
                SpriteRenderer bubble = BubbleFactory.Instance.CreateBubble<SpriteRenderer>((BubbleType)rand, new Vector2(bubbleX, initBubbleY));
                bubble.transform.parent = transform;
                _gameManager.AddBubbleToList(bubble.GetComponent<IBubble>());
                bubbleX += bubbleSize;
            }

            initBubbleY -= BubbleFactory.Instance.BubblePlaceHeight;
        }
        BubbleFactory.Instance.UpdateTopRowPostions(topRowPostions);
    }

     



}