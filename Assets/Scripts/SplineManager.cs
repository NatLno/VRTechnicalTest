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

    private bool tunneling = false;
    [SerializeField]
    private TunnelingManager tunnelingManager;

    private bool smoothStartStop = false;

    [SerializeField]
    private UnityEvent _whenOnPlatform;
    [SerializeField]
    private UnityEvent _whenOutPlatform;

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
        if (playerOnPlatform)
            TeleportPlayerTo(spawnPoint);
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
        SetPlatformParam();

        splineAnimate.Play();
    }

    private void UnsetPlayerToPlatform()
    {
        splineAnimate.Pause();

        player.transform.parent = null;
        leftTeleportInteractor.SetActive(true);
        rightTeleportInteractor.SetActive(true);

        Hotspot hotspot = FindClosestHotspot(player.transform);
        playerOnPlatform = false;
        TeleportPlayerTo(hotspot.GetTransform());
        //_whenOutPlatform.Invoke();
        SetPlatformParam();
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
            audioSource.enabled = !audioSource.enabled;
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

    public void SetPlatformParam()
    {
        SetTunnelingParam();
        SetAudioParam();
    }

    public void SetTunnelingParam()
    {
        if (playerOnPlatform && tunneling)
        {
            tunnelingManager.SetApertureSize(0.3f);
            tunnelingManager.SetFeatheringEffect(0.3f);
        }
        else if (!playerOnPlatform || !tunneling)
        {
            tunnelingManager.SetApertureSize(1f);
            tunnelingManager.SetFeatheringEffect(1f);
        }
    }

    public void SetAudioParam()
    {
        if (audioSource != null && audioSource.enabled)
        {
            if (playerOnPlatform)
                audioSource.Play();
            else
                audioSource.Stop();
        }
    }
    
    public bool GetSmoothStartStop()
    {
        return smoothStartStop;
    }
    public void ToggleSmoothStartStop()
    {
        smoothStartStop = !smoothStartStop;
        if (smoothStartStop)
            splineAnimate.Easing = SplineAnimate.EasingMode.EaseInOut;
        else
            splineAnimate.Easing = SplineAnimate.EasingMode.None;
    }

}
