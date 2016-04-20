using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class LightPath : MonoBehaviour
{
    private Vector3 targetPosition;

    private GameObject nextWaypoint;

    private Vector3 moveVector;

    private float distance;
    private float curTime;

    private float distanceCovered;
    private float distaceRatio;

    [SerializeField]
    private float speed;

    [SerializeField]
    public List<GameObject> Waypoints = new List<GameObject>();

    [SerializeField]
    private bool ignoreY;

    void Awake()
    {
        
    }
    // Use this for initialization
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if(nextWaypoint != null)
        //{
            if (Vector3.Distance(transform.position, nextWaypoint.transform.position) >= 0.2f)
            {
                Vector3 toWaypoint = nextWaypoint.transform.position - transform.position;

                toWaypoint.Normalize();

                Vector3 deltaPos = toWaypoint * speed * Time.deltaTime;

                if (ignoreY == true)
                {
                    deltaPos.y = 0.0f;

                }

                transform.position += deltaPos;

            }

            else if(ignoreY == true && Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(nextWaypoint.transform.position.x, nextWaypoint.transform.position.z)) < 0.2f)
            {
                if (Waypoints.Count > 0)
                {
                    nextWaypoint = Waypoints[0];
                    Waypoints.Remove(nextWaypoint);
                }
            }

            else
            {
                if (Waypoints.Count > 0)
                {
                    nextWaypoint = Waypoints[0];
                    Waypoints.Remove(nextWaypoint);
                }
            }
        //}
    }

    public void StartPath()
    {
        nextWaypoint = Waypoints[0];
        Waypoints.Remove(nextWaypoint);

        Debug.Log(nextWaypoint.gameObject.name);
    }
}
