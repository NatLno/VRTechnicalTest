using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;

[System.Serializable]
public class Hotspot
{
    [SerializeField]
    private float _posInSpline;
    [SerializeField]
    private Transform _transform;

    public Hotspot(float posInSpline, Transform transform)
    {
        _posInSpline = posInSpline;
        _transform = transform;
    }

    public float GetPosInSpline()
    {
        return _posInSpline;
    }
    
    public Transform GetTransform()
    {
        return _transform;
    }
}

public class SplineManager : MonoBehaviour
{
    [SerializeField]
    private Transform spawnPoint;
    [SerializeField]
    private GameObject player;

    [SerializeField]
    private GameObject leftTeleportInteractor;
    [SerializeField]
    private GameObject rightTeleportInteractor;

    public List<Hotspot> hotspots;

    [SerializeField]
    private bool tunneling = false;
    [SerializeField]
    private TunnelingManager tunnelingManager;

    [SerializeField]
    private UnityEvent _whenOnPlatform;
    [SerializeField]
    private UnityEvent _whenOutPlatform;

    [SerializeField]
    private bool playerOnPlatform = false;

    private Hotspot nextHotspotTarget;
    private int currentHotspotIndex = 0;

    private SplineAnimate splineAnimate;

    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        splineAnimate = GetComponent<SplineAnimate>();
        splineAnimate.Pause();
    }

    // Update is called once per frame
    void Update()
    {
        CheckToUnsetPlayerToPlatform();
        if (tunnelingManager != null)
            SetTunnelingParam();
    }

    private void CheckToUnsetPlayerToPlatform()
    {
        if (playerOnPlatform)
        {
            float dist = float.MaxValue;
            if (nextHotspotTarget.GetPosInSpline() == 0)
                dist = Mathf.Abs(splineAnimate.ElapsedTime - (splineAnimate.Duration * 2));
            else
                dist = Mathf.Abs(splineAnimate.ElapsedTime - nextHotspotTarget.GetPosInSpline());
            if (dist < 0.1f)
                UnsetPlayerToPlatform();
        }
    }

    private Hotspot FindClosestHotspot(Transform gameObject)
    {
        float min = float.MaxValue;
        Hotspot res = null;

        foreach (Hotspot hotspot in hotspots)
        {
            float dist = Vector3.Distance(gameObject.position, hotspot.GetTransform().position);
            if (dist < min)
            {
                res = hotspot;
                min = dist;
            }
        }
        return res;
    }

    private void TeleportPlayerTo(Transform point)
    {
        player.transform.position = point.position;
    }

    public void SetPlayerToPlatform(Transform currentHotspot)
    {
        GetCurrentHotspot(currentHotspot);
        IncrementHotspot();
        Hotspot hotspot = FindClosestHotspot(player.transform);
        splineAnimate.ElapsedTime = hotspot.GetPosInSpline();

        player.transform.parent = transform;
        leftTeleportInteractor.SetActive(false);
        rightTeleportInteractor.SetActive(false);

        TeleportPlayerTo(spawnPoint);
        //_whenOnPlatform.Invoke();
        playerOnPlatform = true;

        splineAnimate.Play();
    }

    private void UnsetPlayerToPlatform()
    {
        splineAnimate.Pause();

        player.transform.parent = null;
        leftTeleportInteractor.SetActive(true);
        rightTeleportInteractor.SetActive(true);

        Hotspot hotspot = FindClosestHotspot(player.transform);
        TeleportPlayerTo(hotspot.GetTransform());
        //_whenOutPlatform.Invoke();
        playerOnPlatform = false;
    }

    private void GetCurrentHotspot(Transform currentHotspot)
    {
        for (int i = 0; i < hotspots.Count; i++)
        {
            if (hotspots[i].GetTransform() == currentHotspot)
            {
                currentHotspotIndex = i;
                nextHotspotTarget = hotspots[currentHotspotIndex];
                return;
            }
        }
    }

    private void IncrementHotspot()
    {
        currentHotspotIndex = (currentHotspotIndex + 1) % hotspots.Count;
        nextHotspotTarget = hotspots[currentHotspotIndex];
    }


    public void ToggleAudioSource()
    {
        if (audioSource != null)
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
            else
                audioSource.Play();
        }
    }

    public AudioSource GetAudioSource()
    {
        return audioSource;
    }

    public bool GetTunneling()
    {
        return tunneling;
    }

    public void SetTunneling(bool value)
    {
        tunneling = value;
    }

    public void ToggleTunneling()
    {
        tunneling = !tunneling;
    }

    public void SetTunnelingParam()
    {
        if (playerOnPlatform && tunneling)
        {
            tunnelingManager.SetApertureSize(0.7f);
            tunnelingManager.SetFeatheringEffect(0.3f);
        }
        else
        {
            tunnelingManager.SetApertureSize(1f);
            tunnelingManager.SetFeatheringEffect(1f);
        }
    }
}
