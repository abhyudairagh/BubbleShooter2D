using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class Singleton<T> : MonoBehaviour where T : class
{
    private static T _instance;



    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
             Init();
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    protected abstract void Init();

    public static T Instance
    {
        get
        {
            return _instance;
        }
    }
}


