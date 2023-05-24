using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerValume : MonoBehaviour
{
    public UnityEvent Enter;
    public UnityEvent Exit;

    private void OnTriggerEnter(Collider other)
    {
        //if (!other.CompareTag("Player")) return;
        Enter.Invoke();
    }
}
