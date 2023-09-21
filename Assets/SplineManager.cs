using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

class Hotspot
{
    private float _posInSpline;
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

    [SerializeField]
    private List<Transform> hotspotsTransform;

    [SerializeField]
    private List<float> hotspotsSplinePos;

    private bool playerOnPlatform = false;


    private Hotspot nextTarget;
    private int currentHotspot = 0;

    public Dictionary<int, Transform> test;
    private SplineAnimate splineAnimate;

    private List<Hotspot> hotspots;

    // Start is called before the first frame update
    void Start()
    {
        splineAnimate = GetComponent<SplineAnimate>();
        splineAnimate.Pause();
        hotspots = new List<Hotspot>();
        for (int i = 0; i < hotspotsSplinePos.Count; i++)
        {
            if (hotspotsSplinePos[i] < 0)
                hotspotsSplinePos[i] = 0;
            if (hotspotsSplinePos[i] > 1)
                hotspotsSplinePos[i] = 1;

            hotspots.Add(new Hotspot(hotspotsSplinePos[i], hotspotsTransform[i]));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (playerOnPlatform && Mathf.Abs(splineAnimate.normalizedTime - nextTarget.GetPosInSpline()) < 0.01f)
        {
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

    public void TeleportPlayerTo(Transform point)
    {
        player.transform.position = point.position;
    }

    public void SetPlayerToPlatform()
    {
        IncrementHotspot();
        Hotspot hotspot = FindClosestHotspot(player.transform);
        splineAnimate.StartOffset = hotspot.GetPosInSpline();

        player.transform.parent = transform;
        leftTeleportInteractor.SetActive(false);
        rightTeleportInteractor.SetActive(false);

        TeleportPlayerTo(spawnPoint);
        playerOnPlatform = true;
        splineAnimate.Play();
    }

    void IncrementHotspot()
    {
        currentHotspot = (currentHotspot + 1) % hotspots.Count;
        nextTarget = hotspots[currentHotspot];
    }

    public void UnsetPlayerToPlatform()
    {
        splineAnimate.Pause();

        Hotspot hotspot = FindClosestHotspot(player.transform);
        player.transform.parent = null;
        leftTeleportInteractor.SetActive(true);
        rightTeleportInteractor.SetActive(true);

        TeleportPlayerTo(hotspot.GetTransform());
        playerOnPlatform = false;
    }
}
