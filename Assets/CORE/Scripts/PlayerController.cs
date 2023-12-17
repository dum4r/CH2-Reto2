using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
	private CarController racer;
	void Start(){
		racer = GetComponent<CarController>();
	}

	void Update(){
		racer.horizontalInput = Input.GetAxis("Horizontal");
		racer.verticalInput   = Input.GetAxis("Vertical");
	}
}
