using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OVRSocket : MonoBehaviour
{
    [SerializeField]
    string tagName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals(tagName))
            other.gameObject.GetComponent<SnapInteractable>().enabled = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals(tagName))
            other.gameObject.GetComponent<SnapInteractable>().enabled = false;
    }
}
