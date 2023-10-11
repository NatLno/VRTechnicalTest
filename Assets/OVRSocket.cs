using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Content.Interaction;

public class OVRSocket : MonoBehaviour
{
    [SerializeField]
    string tagName;

    [SerializeField]
    UfoAbductionForce ufoAbductionForce;

    bool hasObject = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals(tagName) && ufoAbductionForce.enabled && !hasObject)
        {
            other.gameObject.GetComponent<SnapInteractor>().enabled = true;
            hasObject = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals(tagName) && !ufoAbductionForce.enabled)
        {
            other.gameObject.GetComponent<SnapInteractor>().enabled = false;
            hasObject = false;
        }
    }
}
