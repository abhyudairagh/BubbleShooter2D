using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public enum IterateDirection
{
    Left,
    Right,
    Up,
    Down
}

public class GameManager : MonoBehaviour,IGameManager
{

    [SerializeField]
    MonoBehaviour[] dependencies;
    [SerializeField]
    LayerMask bubbleLayer;
    [SerializeField]
    int scoreOfOneBubble;
    [SerializeField]
    int bubbleAmmoCount;

    int totalScore;
    int  remainingAmmo;

    int bubbleCount = 0;

    List<IBubble> generatedBubble = new List<IBubble>();

    public int BubbleCount
    {
        get
        {
             return bubbleCount;
        }
    }

    public int GameScore
    {
        get
        {
            return totalScore;
        }
    }
    public int RemainingAmmoCount
    {
        get
        {
            return remainingAmmo;
        }
    }
    public int TotalAmmo 
    { 
        get 
        {
            return bubbleAmmoCount;
        }
    }
    public bool IsShooting { get; set; }
    private void OnEnable()
    {
        GameEvent.Instance.RegisterEvent(GameEvent.GAMERESET_EVENT, OnGameReset);
    }

    public void OnDisable()
    {
        GameEvent.Instance.UnRegisterEvent(GameEvent.GAMERESET_EVENT, OnGameReset);
    }

    private void Start()
    {
        this.remainingAmmo = bubbleAmmoCount;

        foreach (MonoBehaviour behaviour in dependencies)
        {
            if (behaviour != null)
            {
                IManagerDependencies dependencies = behaviour.GetComponent<IManagerDependencies>();
                if (dependencies != null)
                {
                    dependencies.Init(this);
                }
            }
        }
        GameEvent.Instance.SendEvent(GameEvent.GAMESCORE_EVENT, new GameEventArgs(totalScore, remainingAmmo));
    }
    public void AddBubbleToList(IBubble bubble)
    {
        UpdateBubbleCount();
        bubble.Id = bubbleCount;
        generatedBubble.Add(bubble);
    }
    void UpdateBubbleCount()
    {
        bubbleCount++;
    }

    public void OnBubbleThrow(IBubble shooter, int remainingAmmo)
    {
        this.remainingAmmo = remainingAmmo;
        Bound bound = BubbleFactory.Instance.BorderBounds;
        float triggerPoint = bound.miny;
        float ballIntersectPoint = (shooter.GameObject.transform.position.y - shooter.BubbleSize * 0.5f);
        if (ballIntersectPoint< triggerPoint)
        {
            GameOver(false);
            return;
        }

        List<IBubble> sameTypeBubbles = new List<IBubble>();
        bool isSpecial = false;
        if (shooter.Type == BubbleType.RowBreaker)
        {
            isSpecial = true;
            FindItemsToBreakRow(shooter, sameTypeBubbles);
        }
        else
        {
            shooter.CollectSameBubble(sameTypeBubbles);
        }

        if (sameTypeBubbles.Count > 2 || isSpecial)
        {
            foreach (IBubble bubble in sameTypeBubbles)
            {
                bubble.IsBreaked = true;
            }
        }
        generatedBubble.ForEach(x => x.IsTypeCheked = false);
        UpdatelinkedBubbles(bound);
    }
    private void UpdatelinkedBubbles(Bound bound)
    {
        generatedBubble.RemoveAll(x => (Bubble)x == null);
        
        bool hasTopEnding =true;
        bool hasSideEnding = true;
        foreach (IBubble bubble in generatedBubble)
        {
            if (bubble != null)
            {
                if (bubble.HasLink)
                {
                    LinksOnSide(bubble, bound, out List<IBubble> connectedSideBubbles, out hasSideEnding);
                    LinksOnTop(bubble, bound, out List<IBubble> topSideBubbles, out hasTopEnding);
                    bubble.UpdateConnectors(connectedSideBubbles, topSideBubbles, hasSideEnding, hasTopEnding);
                }
            }
        }
        StartCoroutine(FindAndMarkRootless());

    }

    IEnumerator FindAndMarkRootless()
    {
        yield return null;

        foreach (IBubble bubble in generatedBubble)
        {
            UnmarkVisited();
            if (bubble != null)
            {
                if (bubble.HasLink && !bubble.IsShooter)
                {
                    if (!bubble.HasRootConnection())
                    {
                        bubble.MarkAsRootless();
                    }
                }
            }
        }
        yield return RemoveBreakedAndUnlinkedBubbles();
    }

    IEnumerator RemoveBreakedAndUnlinkedBubbles()
    {
        generatedBubble.Reverse();

        List<IBubble> breakedBubbles = generatedBubble.FindAll(x => x.IsBreaked);

        breakedBubbles.Sort();

        List<IBubble> unlinkedBubbles = generatedBubble.FindAll(x => !x.HasLink);

        unlinkedBubbles.Sort();
        int breakableBubbles = breakedBubbles.Count + unlinkedBubbles.Count;

        CalCulateScore(breakableBubbles);



        foreach (IBubble bubble in breakedBubbles)
        {
            if (bubble != null)
            {
                bubble.Pop();
                generatedBubble.Remove(bubble);
                yield return new WaitForSeconds(0.1f);
            }
        }
        foreach (IBubble bubble in unlinkedBubbles)
        {
            if (bubble != null)
            {
                bubble.Pop();
                generatedBubble.Remove(bubble);
                yield return new WaitForSeconds(0.1f);
            }
        }
        generatedBubble.Reverse();

        if(generatedBubble.Count < 1)
        {
            GameOver(true);
        }
        if(remainingAmmo < 1)
        {
            GameOver(false);
        }

        IsShooting = false;
    }

    void UnmarkVisited()
    {
        foreach (IBubble bubble in generatedBubble)
        {
            if (bubble != null)
            {
                bubble.IsVisited = false;
            }
        }
    }

    void FindItemsToBreakRow(IBubble shooter, List<IBubble> rowBubbles)
    {
        rowBubbles.Add(shooter);
        Bound bound = BubbleFactory.Instance.BorderBounds;
        LinksOnSide(shooter, bound, out List<IBubble> connectedSideBubbles, out bool hasSideEnding);
        if (connectedSideBubbles.Count > 0)
        {
            rowBubbles.AddRange(connectedSideBubbles);
        }
        else
        {
            LinksOnTop(shooter, bound, out List<IBubble> neighbourBubbles, out bool hasEnding);
            if (neighbourBubbles.Count > 0)
            {
                LinksOnSide(neighbourBubbles[0], bound, out List<IBubble> connectedtopSides, out bool hastopsideEnding);
                rowBubbles.Add(neighbourBubbles[0]);
                if (connectedtopSides.Count > 0)
                {
                    rowBubbles.AddRange(connectedtopSides);
                }

            }
        }
    }

    void LinksOnSide(IBubble bubble, Bound bound, out List<IBubble> neighbourBubbles , out bool hasEnding )
    {
        float bubbleSize = bubble.BubbleSize;
        neighbourBubbles = new List<IBubble>();
        bool leftEnding = false;
        bool rightEnding = false;
        Vector2 pos = bubble.GameObject.transform.position;
        pos.x -= bubbleSize;
        while (CheckForBubble(pos, bound, out IBubble neighbourBubble, IterateDirection.Left,ref leftEnding))
        {
            neighbourBubbles.Add(neighbourBubble);
            pos.x -= bubbleSize;
        }
       
        pos = bubble.GameObject.transform.position;
        pos.x += bubbleSize;
        while (CheckForBubble(pos, bound, out IBubble neighbourBubble, IterateDirection.Right, ref rightEnding))
         {
                neighbourBubbles.Add(neighbourBubble);
                pos.x += bubbleSize;
         }

        hasEnding = (leftEnding || rightEnding);
    }

    void LinksOnTop(IBubble bubble, Bound bound,  out List<IBubble> neighbourBubbles, out bool hasEnding)
    {
        hasEnding = false;
        neighbourBubbles = new List<IBubble>();
        float bubbleSize = bubble.BubbleSize * 0.75f;

        float topPos = bubble.GameObject.transform.position.y + bubble.BubbleSize;

        Vector2 leftpos = (Vector2)bubble.GameObject.transform.position + ((Vector2.up + Vector2.left) * bubbleSize);
        Vector2 rightpos = (Vector2)bubble.GameObject.transform.position + ((Vector2.up + Vector2.right) * bubbleSize);
        RaycastHit2D leftTopHit = Physics2D.CircleCast(leftpos, 0.001f, Vector2.zero,0,  bubbleLayer);
        RaycastHit2D rightTopHit = Physics2D.CircleCast(rightpos, 0.001f, Vector2.zero, 0,  bubbleLayer);

        Debug.DrawRay((Vector2)bubble.GameObject.transform.position, ((Vector2.up + Vector2.left) * bubbleSize),Color.red);
        Debug.DrawRay((Vector2)bubble.GameObject.transform.position, ((Vector2.up + Vector2.right) * bubbleSize), Color.red);
      //  Debug.Break();

        if (leftTopHit.collider != null)
        {
            IBubble leftTopConnection = leftTopHit.collider.GetComponent<IBubble>();

            if (leftTopConnection.HasLink)
            {
                neighbourBubbles.Add(leftTopConnection);
            }

        }
        if (rightTopHit.collider != null)
        {
            IBubble rightTopConnection = rightTopHit.collider.GetComponent<IBubble>();
            if (rightTopConnection.HasLink)
            {
                neighbourBubbles.Add(rightTopConnection);
            }
        }
        if(topPos > bound.maxy)
        {
            hasEnding = true;
        }
    }


        bool CheckForBubble(Vector2 pos,Bound bound, out IBubble neighbourBubble , IterateDirection direction, ref bool hasEnding)
        {
            neighbourBubble = null;
            RaycastHit2D hit2D =   Physics2D.CircleCast(pos, 0.1f,Vector2.zero,0, bubbleLayer);

            if(hit2D.collider != null)
            {
                IBubble bubble = hit2D.collider.GetComponent<IBubble>();

                neighbourBubble = bubble;

                return bubble.HasLink;
            }
        if ((direction == IterateDirection.Left && pos.x < bound.minx) ||
            (direction == IterateDirection.Right && pos.x > bound.maxx))
            {
                hasEnding = true;
            }
        return false;
    }


    void CalCulateScore(int totalBubblesPopped)
    {
        totalScore += (scoreOfOneBubble * totalBubblesPopped) * totalBubblesPopped;
        GameEvent.Instance.SendEvent(GameEvent.GAMESCORE_EVENT,new GameEventArgs(totalScore, remainingAmmo));
    }

    public void GameOver(bool isWin)
    {
        GameEvent.Instance.SendEvent(GameEvent.GAMEOVER_EVENT, new GameOverEventArgs(totalScore, isWin));
    }

    public void OnGameReset(IEventArgs args)
    {
        foreach (IBubble bubble in generatedBubble)
        {
            if ((Bubble)bubble != null)
            {
                bubble.DestroyBubble();
            }
        }
        generatedBubble.Clear();
        bubbleCount = 0;
        remainingAmmo = bubbleAmmoCount;
        totalScore = 0;

        GameEvent.Instance.SendEvent(GameEvent.GAMESCORE_EVENT, new GameEventArgs(totalScore, remainingAmmo));
    }

    public void OhHomeSelected()
    {
        //OnGameReset();
    }
}
public interface IGameManager
{
    int GameScore { get; }
    int RemainingAmmoCount { get; }
    bool IsShooting { get; set; }

    int TotalAmmo { get; }
    void OnBubbleThrow(IBubble shooter , int remainingAmmo);
    void AddBubbleToList(IBubble bubble);

}

public interface IManagerDependencies
{
    void Init(IGameManager manager);
}