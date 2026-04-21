using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{

    public static EventManager Instance{get;private set;}

    private Dictionary<String,Action<System.Object>> eventDic = new Dictionary<String, Action<System.Object>>();
    private void Awake() {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        //过场景不删除
        DontDestroyOnLoad(gameObject);
    }

    public void Subscribe(String eventName , Action<System.Object> callback)
    {
        if (!eventDic.ContainsKey(eventName))
        {
            eventDic[eventName] = null;
        }
        eventDic[eventName] += callback;
    }

    public void Unsubscribe(String eventName,Action<System.Object> callback)
    {
        if(eventDic.TryGetValue(eventName,out var callbacks))
        {
            callbacks -= callback;
            if(callbacks == null)
            {
                eventDic.Remove(eventName);
            }
            else
            {
                eventDic[eventName] = callbacks;
            }
        }
    }

    public void Broadcast(String eventName,System.Object data = null)
    {
        if(eventDic.TryGetValue(eventName,out var callbacks))
        {
            callbacks?.Invoke(data);
        }
    }
}
