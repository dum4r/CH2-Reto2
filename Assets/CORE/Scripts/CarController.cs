using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour{
	private Rigidbody rb;

	public float horizontalInput, verticalInput;  // Inputs de Jugador 
	public int  CurrentLap =   0  ;
	public bool ChckMidLap = false;
	public float lapTime, bestLapTime;

	public  float velocity  = 0f;                 // Acumunlacion de velocidad
	private float reduction = 0f;                 // Acumulacion de Friccion
	private float currentSteerAngle;              // Algulo de subida
	private Vector3 impulsoActual = Vector3.zero; // Acumulacion de Inercia
	private Vector3    respawnPos;                // posicion para reaparecer
	private Quaternion respawnRot;                // rotacion para reaparecer
	public bool       AutoRespawn   = true;      // reaparece automatico si esta de cabeza 
	
	private Transform objetoA; // Objeto Arriba
	private Transform objetoB; // Objeto Abajo

	[SerializeField] private float motorForce    = 15000f; // Fuerza para los Wheels
	[SerializeField] private float maxSteerAngle =  30.0f; // Algulo max de subida
	[SerializeField] private float maxVelocity   = 100.0f; // Maxima velocidad alcanzable
	[SerializeField] private float impulse       =   5.0f; // fuerza del impulso al acelerar
	[SerializeField] private float reductionMin  =   1.0f; // minimo de friccion
	[SerializeField] private float reductionMax  = 100.0f; // maximo de friccion
	[SerializeField] private float rotacionMin   =   0.3f; // minima rotacion quieto
	[SerializeField] private float rotacionMax   =  30.0f; // maxima rotacion a maxima velocidad

	private WheelCollider frontLeftWheelCollider, frontRightWheelCollider;
	private WheelCollider rearLeftWheelCollider, rearRightWheelCollider;
	private Transform frontLeftWheelTransform, frontRightWheelTransform;
	private Transform rearLeftWheelTransform, rearRightWheelTransform;
	

	void Start(){
		rb = GetComponent<Rigidbody>();
		
		objetoA = transform.Find("objetoA");
		objetoB = transform.Find("objetoB");

		// Optenemos los componentes WheelCollider de cada llanta
		frontLeftWheelCollider  = GetMyWheel("FrontLeftWheel");
		frontRightWheelCollider = GetMyWheel("FrontRightWheel");
		rearLeftWheelCollider   = GetMyWheel("RearLeftWheel");
		rearRightWheelCollider  = GetMyWheel("RearRightWheel");

		// Optenemos los componentes Transform de cada llanta
		frontLeftWheelTransform  = GetMyTransform("FrontLeftWheel");
		frontRightWheelTransform = GetMyTransform("FrontRightWheel");
		rearLeftWheelTransform   = GetMyTransform("RearLeftWheel");
		rearRightWheelTransform  = GetMyTransform("RearRightWheel");

		SetRespawn();
	}

	private void FixedUpdate() {
		lapTime += Time.deltaTime;
		if (transform.position.y < -10.0f ) Respawn();
		// ! Respawn
		if (objetoA.position.y < objetoB.position.y && AutoRespawn) Respawn();
		if (Input.GetKeyDown(KeyCode.R)) Respawn();

		// ! Velocity
		if ( velocity > 0.0f ) velocity -= 10.0f * Time.deltaTime; // minimo de velocidad cero
		if ( velocity < maxVelocity && verticalInput > 0.0f )      // limita la velocidad alcanzable
			velocity += 100.0f * Time.deltaTime;                     // aumenta si esta acelerando

		// ! Rotacion
		float rotacionY = Mathf.Lerp(rotacionMin, rotacionMax, (velocity/10.0f) * Mathf.Abs(horizontalInput));
		if ( rotacionY > 0.0f ) { // si el jugador esta rotando y su velocidad lo permite:
			Quaternion deltaRotacion = Quaternion.Euler(0, rotacionY * horizontalInput * Time.deltaTime, 0);
			Quaternion rotacion = rb.rotation * deltaRotacion;
			rb.MoveRotation(rotacion);
		}

		// ! Aceleration
		if (verticalInput < 0.3f && verticalInput > -0.3f) // Margen de inercia
			impulsoActual = Vector3.Lerp(impulsoActual, Vector3.zero, reduction * Time.deltaTime);
		else // Margen para acelerar
			impulsoActual = (transform.forward * velocity * Time.deltaTime) * impulse;
		Vector3 newPos = rb.position + impulsoActual * Time.deltaTime;
		if (velocity > 0.0f) rb.MovePosition(newPos);
		reduction = Mathf.Lerp(reductionMax, reductionMin, velocity);
    
		// ! HandleMotor
		float motorTorque = verticalInput * motorForce * Time.deltaTime;
		frontLeftWheelCollider.motorTorque  = motorTorque;
		frontRightWheelCollider.motorTorque = motorTorque;
		
		// ! HandleSteering
		currentSteerAngle = maxSteerAngle * horizontalInput;
		frontLeftWheelCollider.steerAngle  = currentSteerAngle;
		frontRightWheelCollider.steerAngle = currentSteerAngle;
		
		// ! UpdateWheels
		UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
		UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
		UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
		UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
	}

	void OnTriggerEnter(Collider other){
		if (other.CompareTag("respawn")) SetRespawn();
		if (other.CompareTag("mid")) {
			SetRespawn();
			ChckMidLap = true;
		}
		if (other.CompareTag("especial")) {
			maxVelocity = 140;
			impulse     = 13f;
			AutoRespawn = false;
			rb.useGravity  = false;
			Invoke("Nomalize", 3.5f);
		}
		
		if (other.CompareTag("finish")) {
			SetRespawn();
			if (ChckMidLap) {
				CurrentLap ++;
				ChckMidLap = false;
				if (lapTime < bestLapTime || bestLapTime == 0)
					bestLapTime = lapTime;
				lapTime = 0f;
			}
			if (CurrentLap > 2) {
				Debug.Log("WINNN!");
			}
		}
	}

	// ============== MIS METODOS ===================
	private void Nomalize(){
		rb.useGravity  = true;
		maxVelocity = 100;
		impulse     = 10f;
		AutoRespawn = true;
	}

	private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform) {
		Vector3 pos;
		Quaternion rot; 
		wheelCollider.GetWorldPose(out pos, out rot);

		wheelTransform.rotation = Quaternion.Lerp(wheelTransform.rotation, rot, Time.deltaTime * 10f);
		wheelTransform.position = Vector3.Lerp(wheelTransform.position, pos, Time.deltaTime * 10f);
	}

	public void Respawn(){
		rb.ResetInertiaTensor();
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero; 
		velocity = 0f;

		transform.position = respawnPos; // Establece la posición
		transform.rotation = respawnRot; // Establece la rotación
	}

	private void SetRespawn(){
		if ( objetoA.position.y > objetoB.position.y ){
			respawnPos = transform.position; // Establece la posición
			respawnRot = transform.rotation; // Establece la rotación
		}
	}

	private WheelCollider GetMyWheel(string str) {
		WheelCollider box  = transform.Find("Wheels/Colliders/"+str).GetComponent<WheelCollider>();
		if  (box == null )
			Debug.LogError("Error!!! No se encontro WheelCollider = " + str);
		return box;
	}

	private Transform GetMyTransform(string str) {
		Transform box  = transform.Find("Wheels/Meshes/"+str).GetComponent<Transform>();
		if  (box == null )
			Debug.LogError("Error!!! No se encontro Transform = " + str);
		return box;
	}
}
