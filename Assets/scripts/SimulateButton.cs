using UnityEngine;
using UnityEngine.UI;

public class SimulateButton : MonoBehaviour
{

	public GameObject PlayImage;
	public GameObject StopImage;
	public Text Text;

	public void SetIsRunning(bool isRunning)
	{
		PlayImage.SetActive(!isRunning);
		StopImage.SetActive(isRunning);

		Text.text = isRunning ? "STOP" : "SIMULATE";
	}
}
