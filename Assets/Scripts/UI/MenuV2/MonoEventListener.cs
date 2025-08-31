using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MonoEventListener : MonoBehaviour
{
    public UnityEvent awakeEvent, startEvent, enableEvent, disableEvent, destroyEvent;

    private void Awake()
    {
        awakeEvent?.Invoke();
    }
    private void Start()
    {
        startEvent?.Invoke();
    }
    private void OnEnable()
    {
        enableEvent?.Invoke();
    }
    private void OnDisable()
    {
        disableEvent?.Invoke();
    }
    private void OnDestroy()
    {
        destroyEvent?.Invoke();
    }
}
