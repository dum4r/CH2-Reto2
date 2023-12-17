using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class IUInGame : MonoBehaviour {
	public CarController racer;

	public GameObject Pause;
	public GameObject Game;
	public GameObject Winn;

	public TextMeshProUGUI LblLaps;
	public TextMeshProUGUI LblTime;
	public TextMeshProUGUI BestTime;

	private Transform needle; // El transform de la aguja del velocímetro
	
	private float velocidadMaxima = 180.0f;
	[SerializeField] private float minRotation = 230.0f; // Rotación mínima de la aguja
	[SerializeField] private float maxRotation = -47.0f; // Rotación máxima de la aguja

	void Start(){
		Time.timeScale = 1f;
		needle = transform.Find("IU In Game/Meter/Needle").GetComponent<Transform>();
		if (needle == null )
			Debug.LogError("Error!!! No se encontro Needle");
	}

	// Aquí se simula un cambio de velocidad, puedes llamar a esta función con tu lógica real de velocidad
	void Update(){
		LblLaps.text = "Laps: " + racer.CurrentLap + "/3" ;

		var lapTime = TimeSpan.FromSeconds(racer.lapTime);
		LblTime.text = "Time: " +string.Format("{0:00} : {1:00}.{2:00}", lapTime.Minutes, lapTime.Seconds, lapTime.Milliseconds);

		var best = TimeSpan.FromSeconds(racer.bestLapTime);
		BestTime.text = "BestTime: "+string.Format("{0:00} : {1:00}.{2:00}", best.Minutes, best.Seconds, best.Milliseconds);

		// Establecemos la rotación de la aguja
		float fraccionVelocidad = racer.velocity / velocidadMaxima;
		float rotacionDeseada   = Mathf.Lerp(minRotation, maxRotation, fraccionVelocidad);
		needle.rotation = Quaternion.Euler(0f, 0f, rotacionDeseada);

		if ( racer.CurrentLap > 2) {
			Winn.SetActive(true);
			Pause.SetActive(false);
			Game.SetActive(false);
			return;
		}

		if (Input.GetKeyDown(KeyCode.Escape)){
			if (Time.timeScale == 0f) Renaudar(); 
			else {
				// Si el juego no está pausado, pausarlo
				Time.timeScale = 0f;
				Pause.SetActive(true);
				Game.SetActive(false);
			}
		}
	}

	public void GoMenu(){
		SceneManager.LoadScene("Menu");
	}
	public void Renaudar(){
		// Si el juego ya está pausado, reanudarlo
		Time.timeScale = 1f;
		Game.SetActive(true);
		Pause.SetActive(false);
	}
}
