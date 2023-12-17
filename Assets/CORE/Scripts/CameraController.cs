using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour{
	
	// public RacerController racer;
	public GameObject MainCamera;
	public GameObject FirstCamera;

	public  float lerp        = 30.0f;
	public  float scale       =  1.0f; 
	public  float scrollSpeed =  0.1f;
	private float minScale    =  0.0f;
	private float maxScale    =  1.0f;

	private float minYOffset =  1.5f;
	private float maxYOffset =  2.0f;
	private float minZOffset =  0.1f;
	private float maxZOffset = -9.0f;

	private Vector3   offset;
	public Transform target;

	void Start(){
		// if (racer == null) return;
		// target = racer.GetComponent<Transform>();
	}

	void Update(){
		float scrollValue = Input.GetAxis("Mouse ScrollWheel");
		if (scrollValue != 0f){ // * Controll del Scroll del raton
			if ( scale < 0.6f && MainCamera.activeSelf ) {
				MainCamera.SetActive(true);
				SwitchCam();
			} else if ( scale > 0.6f && FirstCamera.activeSelf ){
				MainCamera.SetActive(false);
				SwitchCam();				
			}
			float newScale = scale + scrollValue * (scrollSpeed * -1);
			scale = Mathf.Clamp(newScale, minScale, maxScale);
		}

		// * Cambio de Camara con la barra de espacio
		if (Input.GetKeyDown(KeyCode.Space)) SwitchCam(); 
	}
	void FixedUpdate(){
		if (target == null) return;
		// * Actualiza la posicion respecto al objetivo
		offset.y = Mathf.Lerp(minYOffset, maxYOffset, scale);
		offset.z = Mathf.Lerp(minZOffset, maxZOffset, scale);
		
		Vector3 posicionDeseada = target.TransformPoint(offset);
		
		// ! Primera Camara
		if ( FirstCamera.activeSelf ){
			transform.position = posicionDeseada;
			transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, lerp * Time.deltaTime);
			return;
		}

		// ! Segunda Camara 
		// Calcular la posición y rotación deseada de la cámara
		Vector3 posicionTarget = target.position;
		// posicionTarget.y += Mathf.Lerp(0.1f, 0.0f, racer.factorReduccion);
		Quaternion rotacionDeseada = Quaternion.LookRotation(posicionTarget - posicionDeseada, target.up);
		// Mover suavemente la cámara hacia la posición y rotación deseadas
		transform.position = Vector3.Lerp(transform.position, posicionDeseada, lerp * Time.deltaTime);
		transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, lerp * Time.deltaTime);
	}

	public void SwitchCam(){
		bool state = MainCamera.activeSelf;
		scale = state ? 0.1f : 0.9f; 
		MainCamera.SetActive(!state);
		FirstCamera.SetActive(state);
	}
}
