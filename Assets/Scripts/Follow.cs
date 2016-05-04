using UnityEngine;
using System.Collections;

public class Follow : MonoBehaviour
{
    public Transform FollowTarget;
	// Use this for initialization
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
        transform.position = FollowTarget.position;
	}
}
