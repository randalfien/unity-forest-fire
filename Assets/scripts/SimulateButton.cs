using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimulateButton : MonoBehaviour
{

	public GameObject PlayImage;
	public GameObject StopImage;
	public Text Text;
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetIsRunning(bool isRunning)
	{
		PlayImage.SetActive(!isRunning);
		StopImage.SetActive(isRunning);

		Text.text = isRunning ? "STOP" : "SIMULATE";
	}
}
