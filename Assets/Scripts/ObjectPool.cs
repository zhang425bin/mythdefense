using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    Func<GameObject> factory;
    int capacity;
    Queue<GameObject> available = new Queue<GameObject>();
    List<GameObject> all = new List<GameObject>();

    public void Initialize(Func<GameObject> factory, int capacity)
    {
        this.factory = factory;
        this.capacity = capacity;
    }

    public GameObject Get()
    {
        if (available.Count > 0)
        {
            var go = available.Dequeue();
            go.SetActive(true);
            return go;
        }
        if (all.Count >= capacity) return null;
        var newGo = factory();
        all.Add(newGo);
        newGo.SetActive(true);
        return newGo;
    }

    public void Return(GameObject go)
    {
        if (go == null) return;
        go.SetActive(false);
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.zero;
        available.Enqueue(go);
    }
}
