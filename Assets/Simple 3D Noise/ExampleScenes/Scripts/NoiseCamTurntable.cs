using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseCamTurntable : MonoBehaviour {

    public float speed = 16;
	
	// Update is called once per frame
	void Update () {
        transform.eulerAngles += new Vector3(0, speed * Time.deltaTime, 0);
	}
}
