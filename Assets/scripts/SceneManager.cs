using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/**
 * Basic manager to handle user actions
 */

public class SceneManager : MonoBehaviour
{

	/* CONST */
	private const int ModeAdd = 0;
	private const int ModeRemove = 1;
	private const int ModeFire = 2;
	private const int ModeExtinguish = 3;
	
	// PUBLIC
	public float WindStrengthMultiplier = 4;
	
	public Forest Forest;
	public GameObject WindArrow;
	
	/* UI */
	public SimulateButton SimButton;
	public Dropdown ModeDropdown;
	public LayerMask TerrainLayer;
	public Slider WindSpeedSlider;
	public Slider WindDirectionSlider;
	
	/* Wind */
	public float[,] WindMatrix; //3x3 matrix
	public static float[,] WindBaseMatrix; //how fire spreads with no wind
	private Vector2[,] _windMatrixVectors; //3x3 matrix, helps with wind calculation

	// PRIVATE
	private Camera _camera;
	private bool _isSimulationRunning;
	
	void Awake()
	{
		_camera = Camera.main;

		WindMatrix = new float[3,3];
		WindBaseMatrix = new float[3,3];
		_windMatrixVectors = new Vector2[3,3];
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				_windMatrixVectors[i,j] = new Vector2(i-1,j-1).normalized;
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
		if (EventSystem.current.IsPointerOverGameObject(-1))
		{
			return;
		}
		
		Ray ray = _camera.ScreenPointToRay( Input.mousePosition );
		RaycastHit hit;
		if ( !Physics.Raycast(ray, out hit, 300, TerrainLayer) )
		{
			return;
		}

		var p = hit.point;

		var mode = ModeDropdown.value;
		switch (mode)
		{
			case ModeAdd: Forest.AddTreeAt(p);
				break;
			case ModeRemove: Forest.RemoveTreeAt(p);
				break;
			case ModeFire: Forest.AddFireAt(p);
				break;
			case ModeExtinguish: Forest.ExtinguishAt(p);
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
		/*Update wind arrow*/
		var angle = WindDirectionSlider.value * 180 / Mathf.PI;
		WindArrow.transform.localRotation = Quaternion.Euler(0,360f-angle,0);
	}

	private void UpdateWind()
	{
		var speed = WindSpeedSlider.value;
		var angle = WindDirectionSlider.value;
		Vector2 v = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
		//print("----------- v:"+v);
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				var a = WindStrengthMultiplier*Vector2.Dot( _windMatrixVectors[i, j],v); //get angle
				
				WindMatrix[i, j] = WindBaseMatrix[i,j] + a*speed;				

				if (WindMatrix[i, j] < 0)
				{
					WindMatrix[i, j] = 0;
				}
			}
			//print( WindMatrix[i,0].ToString("0.00")+"|"+WindMatrix[i,1].ToString("0.00")+"|"+WindMatrix[i,2].ToString("0.00"));
		}
		
		Forest.WindChanged(WindMatrix);
		SmokeParticles.SetWindSpeed(-WindStrengthMultiplier*speed*v);
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
