using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScaleLightDistance : MonoBehaviour 
{
    [SerializeField] 
    private List<Light> Lights = new List<Light>();

    [SerializeField]
    private float minIntensity;

    [SerializeField]
    private float maxIntensity;

    /// <summary>
    /// Distance (between player and light) at which max intensity is reached (LOWER NUMBER)
    /// </summary>
    [SerializeField]
    private float maxIntensityDistance;

    /// <summary>
    /// Distance (between player and light) at which light starts to grow in intensity (HIGHER NUMBER)
    /// </summary>
    [SerializeField]
    private float minIntensityDistance;

    void Awake()
    {

    }

	void Start () 
    {
	
	}
	

	void Update () 
    {
	    foreach(Light light in Lights)
        {
            float distance = Vector3.Distance(transform.position, light.gameObject.transform.position);

            if(distance < minIntensityDistance && distance > maxIntensityDistance)
            {
                // scale light intensity accordingly

                float intensity = convertRangeTwoToRangeOne(minIntensity, maxIntensity, minIntensityDistance, maxIntensityDistance, distance);

                

                light.intensity = intensity;
            }

            else if(distance > minIntensityDistance)
            {
                light.intensity = minIntensity;
            }

            else if(distance < maxIntensityDistance)
            {
                light.intensity = maxIntensity;
            }
        }
	}

    private float convertRangeTwoToRangeOne(float minOne, float maxOne, float minTwo, float maxTwo, float currentTwo)
    {
        float ret = 0.0f;

        ret = (((currentTwo - minTwo) * (maxOne - minOne)) / (maxTwo - minTwo)) + minOne;

        return ret;
    }
}
