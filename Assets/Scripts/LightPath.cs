﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class PathFollowing : MonoBehaviour
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
    private List<GameObject> waypoints = new List<GameObject>();

    void Awake()
    {
        nextWaypoint = waypoints[waypoints.Count - 1];
        waypoints.Remove(nextWaypoint);
    }
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.position, nextWaypoint.transform.position) >= 0.1f)
        {
            Vector3 toWaypoint = nextWaypoint.transform.position - transform.position;

            toWaypoint.Normalize();
            transform.position += toWaypoint * speed * Time.deltaTime;
        }

        else
        {
            if (waypoints.Count > 0)
            {
                nextWaypoint = waypoints[waypoints.Count - 1];
                waypoints.Remove(nextWaypoint);
            }
        }

    }
}
