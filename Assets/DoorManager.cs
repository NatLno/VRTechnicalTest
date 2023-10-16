using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DoorManager : MonoBehaviour
{
    [SerializeField]
    bool m_DoorOpen = false;

    [SerializeField]
    UnityEvent m_OpenDoor;
    
    [SerializeField]
    UnityEvent m_CloseDoor;

    [SerializeField]
    OVRWheel m_DoorTransform;

    [SerializeField]
    OVRWheel m_DoorHandle;

    [SerializeField]
    OVRKnob m_KeyTransform;

    private void Update()
    {
        if (m_DoorTransform.value > 0 || (m_DoorHandle.isActivated && m_KeyTransform.isActivated && !m_DoorOpen))
        {
            m_DoorOpen = true;
            m_OpenDoor.Invoke();
        }
        else if (m_DoorTransform.value == 0 && (!m_DoorHandle.isActivated || !m_KeyTransform.isActivated) && m_DoorOpen)
        {
            m_DoorOpen = false;
            m_CloseDoor.Invoke();
        }
    }
}
