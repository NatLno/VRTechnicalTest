using PathCreation.Examples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Teleport
{
    [SerializeField]
    private GameObject m_gameObject;
    [SerializeField]
    private float m_posInPath;

    public Teleport(GameObject gameObject, float posInPath)
    {
        m_gameObject = gameObject;
        m_posInPath = posInPath;
    }

    public GameObject GameObject
    {
        get => m_gameObject;
        set => m_gameObject = value;
    }

    public float PosInPath
    {
        get => m_posInPath;
        set => m_posInPath = value;
    }
}

public class PathFollowerManager : MonoBehaviour
{
    private PathFollower pathFollower;

    [SerializeField]
    private Transform spawnPoint;

    [SerializeField]
    private GameObject player;

    [SerializeField]
    private GameObject leftTeleportInteractor;

    [SerializeField]
    private GameObject rightTeleportInteractor;

    [SerializeField]
    private List<Teleport> teleports;
    private int currentTeleport = 0;
    private Teleport nextTeleport;

    private SphereCollider sphereCollider;

    private void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();
        pathFollower = GetComponent<PathFollower>();
        pathFollower.Stop();
        pathFollower.SetDistanceTravelled(0f);
        SetNextTarget();
    }


    private void Update()
    {
        
    }

    public void SetupPlayerOnPlatform(GameObject teleport)
    {
        //player.transform.parent = transform;
        //leftTeleportInteractor.SetActive(false);
        //rightTeleportInteractor.SetActive(false);

        Teleport t = GetTeleport(teleport);
        pathFollower.SetDistanceTravelled(t.PosInPath);
        //TeleportPlayerTo(spawnPoint);
        SetNextTarget(teleport.transform);
        SetNextTarget();
        pathFollower.Play();
    }

    public void UnsetPlayerOnPlatform(GameObject gameObject)
    {
        pathFollower.Stop();
        player.transform.parent = null;
        leftTeleportInteractor.SetActive(true);
        rightTeleportInteractor.SetActive(true);

        //TeleportPlayerTo(gameObject.transform);
    }

    private void SetNextTarget(Transform hotspot = null)
    {
        if (hotspot == null)
        {
            currentTeleport = (currentTeleport + 1) % teleports.Count;
            nextTeleport = teleports[currentTeleport];
        }
        else
        {
            int index = 0;
            foreach (Teleport hs in teleports)
            {

                if (hs.GameObject == hotspot)
                {
                    nextTeleport = hs;
                    currentTeleport = index;
                    return;
                }
                index++;
            }
        }
    }

    private Teleport GetTeleport(GameObject gameObject)
    {
        foreach (Teleport teleport in teleports)
        {
            if (gameObject.Equals(teleport.GameObject))
            {
                return teleport;
            }
        }
        return null;
    }

    public void TeleportPlayerTo(Transform point)
    {
        player.transform.position = point.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("TeleportAnchor") && IsInHotspots(other.gameObject) && other.gameObject.Equals(nextTeleport.GameObject))
        {
            UnsetPlayerOnPlatform(other.gameObject);
        }
    }

    IEnumerator WaitBeforePlay(float time)
    {
        yield return new WaitForSeconds(time);
        pathFollower.Play();
        SetNextTarget();
    }

    public bool IsInHotspots(GameObject gameObject)
    {
        foreach (Teleport teleport in teleports)
        {
            if (gameObject == teleport.GameObject)
            {
                return true;
            }
        }
        return false;
    }
}
