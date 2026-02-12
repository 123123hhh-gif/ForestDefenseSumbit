using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    
    public static EventManager Instance { get; private set; }// Singleton instance (global access point)

    // Stores event name and its corresponding callback list
    private Dictionary<string, Action<System.Object>> eventDic =new Dictionary<string, Action<System.Object>>();

    private void Awake()
    {
        // Ensure only one EventManager exists (Singleton pattern)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

       
        DontDestroyOnLoad(gameObject); // Prevent destruction when loading new scenes
    }

 
    public void Subscribe(string eventName, Action<System.Object> callback)   // Register a listener to a specific event
    {
        // Initialize entry if event does not exist
        if (!eventDic.ContainsKey(eventName))
        {
            eventDic[eventName] = null;
        }

        // Add callback to the event delegate chain
        eventDic[eventName] += callback;
    }

    // Remove a listener from a specific event
    public void Unsubscribe(string eventName, Action<System.Object> callback)
    {
        if (eventDic.TryGetValue(eventName, out var callbacks))
        {
            callbacks -= callback;

            // If no listeners remain, remove the event entry
            if (callbacks == null)
            {
                eventDic.Remove(eventName);
            }
            else
            {
                eventDic[eventName] = callbacks;
            }
        }
    }

    // Trigger an event and pass optional data to listeners
    public void Broadcast(string eventName, System.Object data = null)
    {
        if (eventDic.TryGetValue(eventName, out var callbacks))
        {
        
            callbacks?.Invoke(data);    // Safely invoke all registered listeners
        }
    }
}
