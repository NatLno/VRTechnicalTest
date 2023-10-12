using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActivateObjectWithTrigger : MonoBehaviour
{
    [SerializeField]
    private UnityEvent WhenTrigger;
    [SerializeField]
    private UnityEvent ReleaseTrigger;

    private bool isTrigger = false;

    private void Start()
    {
        enabled = false;
    }

    void Update()
    {
        if (OVRInput.Get(OVRInput.Button.SecondaryHandTrigger) && OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger) && !isTrigger)
        {
            isTrigger = true;
            StartCoroutine(Trigger(1f));
        }
        else if (OVRInput.Get(OVRInput.Button.PrimaryHandTrigger) && OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) && !isTrigger)
        {
            isTrigger = true;
            StartCoroutine(Trigger(1f));
        }
    }

    IEnumerator Trigger(float duration)
    {
        while ((OVRInput.Get(OVRInput.Button.PrimaryHandTrigger) && OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger))
                || (OVRInput.Get(OVRInput.Button.SecondaryHandTrigger) && OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger)))
        {
            WhenTrigger.Invoke();
            yield return new WaitForSeconds(duration);
        }
        ReleaseTrigger.Invoke();
        isTrigger = false;
    }
}
