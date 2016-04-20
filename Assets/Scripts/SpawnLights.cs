using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnLights : MonoBehaviour 
{
    [SerializeField]
    private GameObject lightPrefab;

    [SerializeField]
    private GameObject spawnLocation;

    [SerializeField]
    private float spawnWaitTime;

    [SerializeField]
    private List<GameObject> waypoints;// = new List<GameObject>();

    void Awake()
    {

    }
	// Use this for initialization
	void Start () 
    {
        StartCoroutine(spawnLoop());
	}
	
	// Update is called once per frame
	void Update () 
    {
	    
	}

    private IEnumerator spawnLoop()
    {
        while(true)
        {
            GameObject newLight = (GameObject)Instantiate(lightPrefab, spawnLocation.transform.position, Quaternion.identity);

            newLight.GetComponent<LightPath>().Waypoints.AddRange(waypoints);

            newLight.GetComponent<LightPath>().StartPath();

            yield return new WaitForSeconds(spawnWaitTime);
        }

        yield return null;
    }

}
