using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;

[System.Serializable]
public class Hotspot
{
    [SerializeField]
    [Range(0f, 1f)]
    private float _posInSpline;

    [SerializeField]
    private Transform _transform;

    [SerializeField]
    private BezierKnot _knot;

    public Hotspot(float posInSpline, Transform transform, BezierKnot knot)
    {
        _posInSpline = posInSpline;
        _transform = transform;
        _knot = knot;
    }

    public float PosInSpline
    {
        get => _posInSpline;
        set => _posInSpline = value;
    }

    public BezierKnot Knot
    {
        get => _knot;
        set => _knot = value;
    }

    public Transform Transform
    {
        get => _transform;
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
    private float apertureSize = 0.3f;
    private float featheringEffect = 0.3f;

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
        SetHotspotParam();
    }

    private void SetHotspotParam()
    {
        foreach (Hotspot hotspot in hotspots)
        {
            hotspot.Knot = FindClosestKnot(hotspot);
        }
    }

    private BezierKnot FindClosestKnot(Hotspot hotspot)
    {
        float min = float.MaxValue;
        BezierKnot res = new BezierKnot();

        foreach (BezierKnot knot in splineAnimate.Container.Splines[0])
        {
            float dist = Vector3.Distance(hotspot.Transform.localPosition, knot.Position);
            if (dist < min)
            {
                min = dist;
                res = knot;
            }
        }
        return res;
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
            float dist = Vector3.Distance(transform.localPosition, nextHotspotTarget.Knot.Position);
            Debug.LogWarning(dist);
            if (dist < 0.05f * (splineAnimate.Container.Splines[0].Count) / 2f)
            {
                UnsetPlayerToPlatform();
            }
        }
    }

    private Hotspot FindClosestHotspot(Transform gameObject)
    {
        float min = float.MaxValue;
        Hotspot res = null;

        foreach (Hotspot hotspot in hotspots)
        {
            float dist = Vector3.Distance(gameObject.position, hotspot.Transform.position);
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
        splineAnimate.StartOffset = nextHotspotTarget.PosInSpline;
        splineAnimate.Restart(true);
        IncrementHotspot();

        player.transform.parent = transform;
        leftTeleportInteractor.SetActive(false);
        rightTeleportInteractor.SetActive(false);

        TeleportPlayerTo(spawnPoint);
        _whenOnPlatform.Invoke();
        StartCoroutine(WaitBeforePlay(0.1f));
    }

    IEnumerator WaitBeforePlay(float time)
    {
        yield return new WaitForSeconds(time);
        splineAnimate.Play();
        playerOnPlatform = true;
        SetPlatformParam();
    }

    private void UnsetPlayerToPlatform()
    {
        StopAllCoroutines();
        splineAnimate.Pause();

        player.transform.parent = null;
        leftTeleportInteractor.SetActive(true);
        rightTeleportInteractor.SetActive(true);

        Hotspot hotspot = FindClosestHotspot(player.transform);
        playerOnPlatform = false;
        TeleportPlayerTo(hotspot.Transform);
        _whenOutPlatform.Invoke();
        SetPlatformParam();
    }

    private void GetCurrentHotspot(Transform currentHotspot)
    {
        for (int i = 0; i < hotspots.Count; i++)
        {
            if (hotspots[i].Transform == currentHotspot)
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

    public void SetApertureSize(float value)
    {
        apertureSize = value;
    }
    
    public void SetFeatheringEffect(float value)
    {
        featheringEffect = value;
    }

    public float GetApertureSize()
    {
        return apertureSize;
    }

    public float GetFeatheringEffect()
    {
        return featheringEffect;
    }

    public void SetTunnelingParam()
    {
        if (playerOnPlatform && tunneling)
        {
            tunnelingManager.SetApertureSize(apertureSize);
            tunnelingManager.SetFeatheringEffect(featheringEffect);
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

    public void StartPlatform()
    {
        splineAnimate.Play();
    }

    public void StopPlatform(bool smoothStop)
    {
        if (!smoothStop)
        {
            splineAnimate.Pause();
            return;
        }
        StartCoroutine(SmoothStop(2f, true));
    }

    IEnumerator SmoothStop(float duration, bool stop)
    {
        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            splineAnimate.ElapsedTime -= Time.deltaTime / (2 * duration);
            yield return null;
        }
        if (stop)
            splineAnimate.Pause();
    }
}
