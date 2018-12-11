using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlwaysFaceCamera : MonoBehaviour {

    private Camera myCamera;

	// Use this for initialization
	void Start ()
    {
        myCamera = Camera.main;
	}
	
	// Update is called once per frame
	void Update ()
    {
        Vector3 v = myCamera.transform.position - transform.position;
        v.x = v.z = 0.0f;

        transform.LookAt(myCamera.transform.position - v);
        transform.Rotate(0, 180, 0);
    }
}
