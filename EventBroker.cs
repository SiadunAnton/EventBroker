using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Object = System.Object;

public class EventBroker
{
    public static EventBroker Instance => _instance ??= new EventBroker();

    private static EventBroker _instance;

    private class PackedInvokableData
    {
        public DataLifetime Lifetime;

        private readonly Dictionary<(Type, string), Object> _storage = new Dictionary<(Type, string), Object>();

        public PackedInvokableData(DataLifetime lifetime)
        {
            Lifetime = lifetime;
        }

        public object Pull(Type type)
        {
            return Pull(type, "");
        }

        public object Pull(Type type, string tag)
        {
            return _storage.GetValueOrDefault((type, tag));
        }

        public void Push(Type type, Object data)
        {
            Push(type, "", data);
        }

        public void Push(Type type, string tag, Object data)
        {
            if (_storage.ContainsKey((type, tag)))
                throw new OperationCanceledException($"Cannot push data with type: {type},tag: {tag}, because linked invokable data already exist.");

            _storage[(type, tag)] = data;
        }
    }

    public enum DataLifetime
    {
        DeleteAfterUse,
        DeleteByCommand,
        DoNotDelete
    }

    private long _id = 1;

    private readonly Dictionary<string, Action<EventArgs>> _subscribers = new Dictionary<string, Action<EventArgs>>();

    private readonly Dictionary<long, PackedInvokableData> _invokationCollector = new Dictionary<long, PackedInvokableData>();

    public void Subscribe(string eventName, Action<EventArgs> handler)
    {
        if (!_subscribers.ContainsKey(eventName))
        {
            _subscribers.Add(eventName, (x) => { });
        }

        _subscribers[eventName] += handler;

    }

    public void Unsubscribe(string eventName, Action<EventArgs> handler)
    {
        if (_subscribers.ContainsKey(eventName))
        {
            _subscribers[eventName] -= handler;
        }
    }

    public void Publish<T>(string eventName, T message, DataLifetime lifetime = DataLifetime.DeleteByCommand)
    {
        if (_subscribers.TryGetValue(eventName, out var subscriber))
        {
            _id++;
            ReserveInvokableDataForEvent(_id, "",message, lifetime);
            subscriber.Invoke(new EventArgs { Name = eventName, ID = _id, Message = message });
        }
    }

    public void Publish(string eventName, DataLifetime lifetime = DataLifetime.DeleteByCommand)
    {
        Publish(eventName,new EmptyData(), lifetime);
    }

    //USE ONLY BEFORE EVENT,THAT YOU GOING TO PUBLISH RIGHT AFTER THIS METHOD
    public void PrepareInvokableDataForNextEvent<T>(string tag, T data, DataLifetime lifetime = DataLifetime.DeleteByCommand)
    {
        long id = _id + 1;
        ReserveInvokableDataForEvent(id, tag, data, lifetime);
    }

    //USE ONLY BEFORE EVENT,THAT YOU GOING TO PUBLISH RIGHT AFTER THIS METHOD
    public void PrepareInvokableDataForNextEvent<T>(T data, DataLifetime lifetime = DataLifetime.DeleteByCommand)
    {
        long id = _id + 1;
        ReserveInvokableDataForEvent(id, "", data, lifetime);
    }

    private void ReserveInvokableDataForEvent<T>(long id, string tag, T message, DataLifetime lifetime = DataLifetime.DeleteByCommand)
    {
        if (_invokationCollector.TryGetValue(id, out var invokableData))
        {
            invokableData.Push(typeof(T), tag,(object)message);

            Debug.Log($"Append data into broker,id: {id}, tag: {tag}");
        }
        else
        {
            var packedData = new PackedInvokableData(lifetime);
            packedData.Push(typeof(T), tag, (object)message);
            _invokationCollector.Add(id, packedData);

            Debug.Log($"Add data into broker,id: {id}, tag: {tag}");
        }
    }

    public T GetInvokableData<T>(long ID)
    {
        return GetInvokableData<T>(ID, "");
    }

    public T GetInvokableData<T>(long ID, string tag)
    {
        var data = _invokationCollector.GetValueOrDefault(ID);
        object result = data.Pull(typeof(T), tag);

        if (data.Lifetime == DataLifetime.DeleteAfterUse)
        {
            _invokationCollector.Remove(ID);
        }

        Debug.LogWarning($"Try get data for broker, id: {ID}, tag: {tag},result is zero:{result == null}");

        return (T)result;
    }

    public bool RemoveInvokableData(long ID, string tag)
    {
        if (!_invokationCollector.ContainsKey(ID))
        {
            return false;
        }
        else
        {
            var data = _invokationCollector.GetValueOrDefault(ID);
            if (data.Lifetime == DataLifetime.DoNotDelete || data.Lifetime != DataLifetime.DeleteByCommand)
            {
                return false;
            }
            else
            {
                _invokationCollector.Remove(ID);
                return true;
            }
        }
    }

    public void ClarifyInvokationData<T>(long ID, string tag, T data)
    {
        if (_invokationCollector.TryGetValue(ID, out var packed))
        {
            packed.Push(typeof(T), tag, (object)data);
        }
    }
}

public struct EventArgs
{
    public string Name;
    public long ID;
    public object Message;

}


