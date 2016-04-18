using UnityEngine;
using System.Collections;

public class LightIndicators : MonoBehaviour 
{
    [SerializeField]
    private float sinOffset;

    [SerializeField]
    private float segments;

    [SerializeField]
    private float speed;

    [SerializeField]
    private Light light;

    private float sinAmount;

	// Use this for initialization
	void Start () 
    {
        float startOffset = (360.0f * (sinOffset / segments));

        sinAmount = Mathf.Sin(startOffset + (Time.time * speed));
	}
	
	// Update is called once per frame
	void Update () 
    {
        float startOffset = (360.0f * (sinOffset / segments));

        sinAmount = Mathf.Sin(startOffset + (Time.time * speed));

        //Debug.Log(sinAmount);

        float lightIntensity = convertToLightIntensity(0.0f, 1.0f, -1.0f, 1.0f, sinAmount);

        //Debug.Log(lightIntensity);

        light.intensity = lightIntensity;

	}

    private float convertToLightIntensity(float minLight, float maxLight, float minSin, float maxSin, float currentSin)
    {
        float ret = 0.0f;

        ret = (((currentSin - minSin) * (maxLight - minLight)) / (maxSin - minSin)) + minLight;

        return ret;
    }
}
