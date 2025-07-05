using System.Collections.Generic;
using UnityEngine;

public class ObServerManager : MonoBehaviour
{
    public static ObServerManager Instance;
    private List<IObserver> observers = new List<IObserver>();

    private void Awake()
    {
        Instance = this;
    }

    public void addObsever(IObserver observer)
    {
        observers.Add(observer);
    }

    public void removeObsever(IObserver observer)
    {
        observers.Remove(observer);
    }

    public void OnCheckPoint()
    {
        for (int i = observers.Count - 1; i >= 0; i--)
        {
            observers[i].ReturnPool();
        }
    }

    
}
