using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventBind
{
    public Action<object> action;
    public string eventName;

    public EventBind(string _eventName, Action<object> _action )
    {
        eventName = _eventName;
        action = _action;
    }
}
public class EventDispatcher 
{
    Dictionary<string, List<EventBind>> connections = new Dictionary<string, List<EventBind>>();

    static EventDispatcher _instance = null;
    public static EventDispatcher instance {
        get{
            if (_instance == null)
                _instance = new EventDispatcher();
            return _instance;
        }
    }

    public static EventBind Bind(string eventName, Action<object> action)
    {
        EventBind eventBind = new EventBind(eventName, action);

        if (!instance.connections.ContainsKey(eventName))
            instance.connections.Add(eventName, new List<EventBind>());

        instance.connections[eventName].Add(eventBind);

        return eventBind;
    }

    public static void Unbind(EventBind b)
    {
        if (instance.connections.ContainsKey(b.eventName))
        {
            instance.connections[b.eventName].Remove(b);
        }
    }

    public static void SendEvent(string eventName, object data = null)
    {
        if (instance.connections.ContainsKey(eventName))
        {
            foreach(EventBind eventBind in instance.connections[eventName])
            {
                eventBind.action(data);
            }
        }
    }
}
