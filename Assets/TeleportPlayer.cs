using PathCreation.Examples;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportPlayer : MonoBehaviour
{
    [SerializeField]
    private Transform spawnPoint;

    [SerializeField]
    private GameObject player;
    public GameObject leftTeleportInteractor;    
    public GameObject rightTeleportInteractor;

    public List<Transform> hotspots;
    private int currentTarget = 0;

    private Transform nextTarget;

    private PathFollower pathFollower;
    private Rigidbody rb;
    private bool playerOnPlatform = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pathFollower = GetComponent<PathFollower>();
        nextTarget = hotspots[currentTarget];
    }

    // Update is called once per frame
    void Update()
    {
        if (playerOnPlatform)
            CheckEndPath();
    }

    private void CheckEndPath()
    {
        if (Vector3.Distance(rb.position, nextTarget.position) < 1.25f)
        {
            UnsetPlayerOnPlatform();
        }
    }

    public void SetupPlayerOnPlatform(Transform hotspot)
    {
        SetNextTarget(hotspot);
        StartCoroutine(WaitPlatformToCome());
    }

    public void UnsetPlayerOnPlatform()
    {
        player.transform.parent = null;
        leftTeleportInteractor.SetActive(true);
        rightTeleportInteractor.SetActive(true);

        TeleportPlayerTo(FindClosestHotspot());

        pathFollower.speed = 0;
        playerOnPlatform = false;
    }

    private Transform FindClosestHotspot()
    {
        float min = float.MaxValue;
        Transform res = null;

        foreach (Transform hotspot in hotspots)
        {
            float dist = Vector3.Distance(rb.position, hotspot.position);
            if (dist < min)
            {
                res = hotspot;
                min = dist;
            }
        }
        return res;
    }

    private void SetNextTarget(Transform hotspot = null)
    {
        if (hotspot == null)
        {
            currentTarget = (currentTarget + 1) % hotspots.Count;
            nextTarget = hotspots[currentTarget];
        }
        else
        {
            int index = 0;
            foreach (Transform hs in hotspots)
            {

                if (hs == hotspot)
                {
                    nextTarget = hs;
                    currentTarget = index;
                    return;
                }
                index++;
            }
        }
    }

    public void TeleportPlayerTo(Transform point)
    {
        player.transform.position = point.position;
    }

    IEnumerator WaitPlatformToCome()
    {
        pathFollower.speed = 2;
        while (Vector3.Distance(rb.position, nextTarget.position) > 1.25f)
        {
            yield return new WaitForSeconds(0.1f);
        }
        SetNextTarget();

        player.transform.parent = transform;

        leftTeleportInteractor.SetActive(false);
        rightTeleportInteractor.SetActive(false);
        TeleportPlayerTo(spawnPoint);
        pathFollower.speed = 2;
        playerOnPlatform = true;
    }
}
