using System;
using System.Collections.Generic;
using UnityEngine;
public static class EventBus<T>
{

    // Private backing field - no one can mess with this directly
    private static Action<T> onEvent;

    // Public method to add a listener
    public static void Subscribe(Action<T> listener)
    {
        onEvent += listener;
    }

    // Public method to remove a listener
    public static void Unsubscribe(Action<T> listener)
    {
        onEvent -= listener;
    }

    // Public method to fire the event
    public static void Raise(T eventData)
    {
        onEvent?.Invoke(eventData);
    }
}

