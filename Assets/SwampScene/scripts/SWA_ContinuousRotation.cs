using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//rotates the gameobject continuously. 

public class SWA_ContinuousRotation : MonoBehaviour {


	public float X;   	// rotation values along different axes


	public float Y;

	public float Z;

	public bool local;  //use world coordinates or the local one




	// Use this for initialization
	void Update () {

	


		if (local==false)
			transform.Rotate(X*Time.deltaTime, Y*Time.deltaTime, Z*Time.deltaTime, Space.World);
		else	
			transform.Rotate(X*Time.deltaTime, Y*Time.deltaTime, Z*Time.deltaTime);



	}
	

}
