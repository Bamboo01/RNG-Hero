using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Bamboo.Events
{
    public class EventChannel : UnityEvent<IEventRequestInfo> { }

    public class EventManager : MonoBehaviour
    {
        // Singleton
        static public EventManager Instance
        {
            get;
            private set;
        }
        public void Awake()
        {
            if (Instance)
            {
                Debug.LogWarning("Event Manager instance already created. Deleting it and instantiating a new instance...");
                Destroy(Instance);
                Instance = this;
            }
            else
            {
                Instance = this;
            }
        }

        // Stores all the events
        Dictionary<string, EventChannel> EventDictionary = new Dictionary<string, EventChannel>();

        // Function to allow an object to listen to a channel, and call a function(s) when a request to said channel is made
        public void Listen(string channelname, UnityAction<IEventRequestInfo> action)
        {
            if (!EventDictionary.ContainsKey(channelname))
            {
                EventDictionary.Add(channelname, new EventChannel());
            }
            EventChannel channel = EventDictionary[channelname];
            channel.AddListener(action);
        }

        // Allows an object to publish info to all listeners of a channel. Can send custom datatypes over.
        public void Publish<T>(string channelname, object sender, T body)
        {
            EventChannel channel;
            if (EventDictionary.TryGetValue(channelname, out channel))
            {
                channel.Invoke(new EventRequestInfo<T>(channelname, sender, body));
            }
            else
            {
                Debug.LogWarning(
                    string.Format("Tried to publish an event to non-existent event channel \"{0}\" (Did you forget to listen to the channel, or was it a typo?)", channelname)
                );
            }
        }

        public void Publish(string channelname, object sender)
        {
            EventChannel channel;
            if (EventDictionary.TryGetValue(channelname, out channel))
            {
                channel.Invoke(new EventRequestInfo(channelname, sender));
            }
            else
            {
                Debug.LogError("Tried to publish an event to a non-existent event channel with the name " + channelname + " (Did you forget to register a channel, or was it a typo?)");
            }
        }

        // Allows an object to stop listening to a channel
        public void Close(string channelname, UnityAction<IEventRequestInfo> action)
        {
            EventChannel channel;
            if (EventDictionary.TryGetValue(channelname, out channel))
            {
                channel.RemoveListener(action);
            }
        }
    }
}