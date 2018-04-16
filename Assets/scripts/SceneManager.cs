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

	void Start()
	{
		_camera = Camera.main;
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

		print(p);
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

	public void OnModeChange()
	{
		print(ModeDropdown.value);
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
