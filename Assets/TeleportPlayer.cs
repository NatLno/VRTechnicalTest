using PathCreation.Examples;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TeleportPlayer : MonoBehaviour
{
    [SerializeField]
    private Transform spawnPoint;

    [SerializeField]
    private GameObject player;

    [SerializeField]
    private float platformSpeed;

    public float maxSpeed = 2f;

    [SerializeField]
    private GameObject leftTeleportInteractor;
    
    [SerializeField]
    private GameObject rightTeleportInteractor;

    [SerializeField]
    private List<Transform> hotspots;

    private int currentTarget = 0;

    private Transform nextTarget;

    private PathFollower pathFollower;
    private Rigidbody rb;
    private bool playerOnPlatform = false;

    [SerializeField]
    private bool toggleTunneling = true;

    [SerializeField]
    private bool smoothStartStop = true;

    [SerializeField]
    private UnityEvent _whenOnPlatform;
    
    [SerializeField]
    private UnityEvent _whenOutPlatform;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pathFollower = GetComponent<PathFollower>();
        platformSpeed = 0;
        nextTarget = hotspots[currentTarget];
    }

    // Update is called once per frame
    void Update()
    {
        pathFollower.speed = platformSpeed;
        if (playerOnPlatform)
            CheckEndPath();
    }

    private void CheckEndPath()
    {
        if (Vector3.Distance(rb.position, nextTarget.position) < 1.25f)
        {
            UnsetPlayerOnPlatform();
        }
        else if (Vector3.Distance(rb.position, nextTarget.position) < 3.5f && smoothStartStop)
        {
            StartCoroutine(ChangeSpeed(platformSpeed, 0f, 2.5f));
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
        if (toggleTunneling)
            _whenOutPlatform.Invoke();

        platformSpeed = 0;
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
        platformSpeed = 2;
        while (Vector3.Distance(rb.position, nextTarget.position) > 1.25f)
        {
            yield return new WaitForSeconds(0.1f);
        }
        SetNextTarget();

        player.transform.parent = transform;
        leftTeleportInteractor.SetActive(false);
        rightTeleportInteractor.SetActive(false);

        TeleportPlayerTo(spawnPoint);
        if (toggleTunneling)
            _whenOnPlatform.Invoke();

        if (smoothStartStop)
            StartCoroutine(ChangeSpeed(0f, maxSpeed, 1.5f));
        else
            platformSpeed = maxSpeed;
        playerOnPlatform = true;
    }

    IEnumerator ChangeSpeed(float v_start, float v_end, float duration)
    {
        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            platformSpeed = Mathf.Lerp(v_start, v_end, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        platformSpeed = v_end;
    }

    public void ToggleTunneling()
    {
        toggleTunneling = !toggleTunneling;
    }

    public bool GetTunneling()
    {
        return toggleTunneling;
    }

    public void ToggleSmoothStartStop()
    {
        smoothStartStop = !smoothStartStop;
    }

    public bool GetSmoothStartStop()
    {
        return smoothStartStop;
    }
}
