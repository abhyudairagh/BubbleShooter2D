using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class GameEvent : Singleton<GameEvent>
{
    public const string GAMESCORE_EVENT = "*****=GAME_SCORE_EVENT=******";
    public const string GAMEOVER_EVENT = "*****=GAME_OVER_EVENT=******";
    public const string GAMERESET_EVENT = "*****=GAME_RESET_EVENT=******";
    public const string HOMEPRESSED_EVENT = "*****=GAME_HOMEPRESSED_EVENT=******";

    Dictionary<string, List<Action<IEventArgs>>> eventActionList = new Dictionary<string, List<Action<IEventArgs>>>();



    public  void RegisterEvent(string eventId , Action<IEventArgs> callback)
    {
        if(eventActionList.ContainsKey(eventId))
        {
            if (!eventActionList[eventId].Contains(callback))
            {
                eventActionList[eventId].Add(callback);
            }
        }
        else
        {
            List<Action<IEventArgs>> actionQueue = new List<Action<IEventArgs>>();
            actionQueue.Add(callback);
            eventActionList.Add(eventId, actionQueue);
        }
    }
    public  void UnRegisterEvent(string eventId, Action<IEventArgs> callback)
    {
        if (eventActionList.ContainsKey(eventId))
        {
            if (eventActionList[eventId].Contains(callback))
            {
                eventActionList[eventId].Remove(callback);
            }
        }
    }

    public  void SendEvent(string eventId, IEventArgs args)
    {
        if (eventActionList.ContainsKey(eventId))
        {
           foreach(Action<IEventArgs> action in eventActionList[eventId])
            {
                action?.Invoke(args);
            }
        }
    }

    protected override void Init()
    {
        //throw new NotImplementedException();
    }
}


public interface IEventArgs
{

}
public class EventArgs : IEventArgs
{
    public EventArgs()
    {

    }
}

public interface IGameEventArgs:IEventArgs
{
    public int Score { get; }
    public int RemainingBubbles { get; }
}
public class GameEventArgs : IGameEventArgs
{
    public int score;
    public int remainingBubbles;
    public GameEventArgs(int score, int remainingBubbles)
    {
        this.score = score;
        this.remainingBubbles = remainingBubbles;
    }

    public int Score { get { return score; } }

    public int RemainingBubbles { get { return remainingBubbles; } }
}


public interface IGameOverEventArgs : IEventArgs
{
    public int Score { get; }
    public bool IsWin  { get; }
}
public class GameOverEventArgs : IGameOverEventArgs
{
    public int score;
    public bool isWin;
    public GameOverEventArgs(int score, bool isWin)
    {
        this.score = score;
        this.isWin = isWin;
    }

    public int Score { get { return score; } }

    public bool IsWin { get { return isWin; } }
}
