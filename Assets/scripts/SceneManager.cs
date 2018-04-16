using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.UI;

public class SceneManager : MonoBehaviour
{

	private const int MODE_ADD = 0;
	private const int MODE_REMOVE = 1;
	private const int MODE_FIRE = 2;
	private const int MODE_EXTINGUISH = 3;
	
	private bool _isSimulationRunning;

	public Forest Forest;

	public SimulateButton SimButton;

	public Dropdown ModeDropdown;

	private Camera _camera;

	public LayerMask TerrainLayer;

	public Slider WindSpeedSlider;
	public Slider WindDirectionSlider;

	public float[,] WindMatrix; //3x3 matrix
	public static float[,] WindBaseMatrix; //how fire spreads with no wind
	private Vector2[,] WindMatrixVectors; //3x3 matrix, helps with wind calculation
	
	void Start()
	{
		_camera = Camera.main;

		WindMatrix = new float[3,3];
		WindBaseMatrix = new float[3,3];
		WindMatrixVectors = new Vector2[3,3];
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				WindMatrixVectors[i,j] = new Vector2(i-1,j-1).normalized;
				if (i == 1 || j == 1)
				{
					WindBaseMatrix[i, j] = 1f;
				}
				else
				{
					WindBaseMatrix[i, j] = 0.5f;
				}
			} 
		}
	}
	
	void Update () {
		if ( !Input.GetButton("MainAction"))
		{
			return;
		}
		if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(-1))
		{
			return;
		}
		
		Ray ray = _camera.ScreenPointToRay( Input.mousePosition );
		RaycastHit hit;
		if ( !Physics.Raycast(ray, out hit, 100, TerrainLayer) )
		{
			return;
		}

		var p = hit.point;

		var mode = ModeDropdown.value;
		switch (mode)
		{
			case MODE_ADD: Forest.AddTreeAt(p);
				break;
			case MODE_REMOVE: Forest.RemoveTreeAt(p);
				break;
			case MODE_FIRE: Forest.AddFireAt(p);
				break;
			case MODE_EXTINGUISH: Forest.ExtinguishAt(p);
				break;
		}
	}
	
	public void WindSpeedChange()
	{
		UpdateWind();
	}
	
	public void WindDirChange()
	{
		UpdateWind();
	}

	private void UpdateWind()
	{
		var speed = WindSpeedSlider.value;
		var angle = WindDirectionSlider.value;
		Vector2 v = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
		print("----------- v:"+v);
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				var a = Vector2.Dot( WindMatrixVectors[i, j],v);
				
				WindMatrix[i, j] = WindBaseMatrix[i,j] + a*speed;				

				if (WindMatrix[i, j] < 0)
				{
					WindMatrix[i, j] = 0;
				}
			}
			print( WindMatrix[i,0].ToString("0.00")+"|"+WindMatrix[i,1].ToString("0.00")+"|"+WindMatrix[i,2].ToString("0.00"));
		}
		
		Forest.WindChanged(WindMatrix);
	}

	public void StartStopSimulation()
	{
		_isSimulationRunning = !_isSimulationRunning;
		Forest.SetSimulationActive(_isSimulationRunning);
		SimButton.SetIsRunning(_isSimulationRunning);
	}

	public void Exit()
	{
		Application.Quit();
	}
	
}
