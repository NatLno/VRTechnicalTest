using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TunnelingManager : MonoBehaviour
{
    // Start is called before the first frame update

    private Renderer rend;
    private float duration = 1f;
    void Start()
    {
        rend = GetComponent<Renderer>();
        SetApertureSize(1f);
        SetFeatheringEffect(1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetDuration(float value)
    {
        duration = value;
    }

    public void SetApertureSize(float value)
    {
        float startValue = rend.material.GetFloat("_ApertureSize");
        StartCoroutine(ChangeValue(startValue, value, duration, "_ApertureSize"));
    }
    public void SetFeatheringEffect(float value)
    {
        float startValue = rend.material.GetFloat("_FeatheringEffect");
        StartCoroutine(ChangeValue(startValue, value, duration, "_FeatheringEffect"));
    }

    IEnumerator ChangeValue(float v_start, float v_end, float duration, string propertyName)
    {
        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            rend.material.SetFloat(propertyName, Mathf.Lerp(v_start, v_end, elapsed / duration));
            elapsed += Time.deltaTime;
            yield return null;
        }
        rend.material.SetFloat(propertyName, v_end);
    }
}
