using UnityEngine;

/**
 *  Forest consists of a number of smaller parts with their own mesh
 *  and assigns correct properties and updates them. 
 */
public class Forest : MonoBehaviour
{
	// PUBLIC
	public GameObject ForestPartPrefab;
	public LowPolyTerrain Terrain;

	public const int Width = 6; //how many forest parts along the X axis
	public const int Height = 6; //how many forest parts along the Z axis

	// PRIVATE
	private readonly ForestPart[,] _parts = new ForestPart[Width, Height]; //list of all the forest parts. Parts may access other parts' data through this.

	private bool _isRunning; //Should we keep updating the parts or not
	private int _currentRow; //Which row should we start updating next frame
	
	private void Start()
	{
		PrepareForestParts();
	}

	private void Update()
	{
		if (!_isRunning) return;
		
		//Update just two rows at a time, in effect updating at only a third of the framerate
		//Slows down the speed of simulation, but keeps framerate at 60fps
		
		for (int j = 0; j < Height; j++)  
		{
			_parts[_currentRow, j].DoUpdate();
			_parts[(_currentRow + 1) % Width, j].DoUpdate();
		}
		
		_currentRow = (_currentRow + 2) % Width;
	}

	private void PrepareForestParts()
	{
		const int partWidth = ForestPart.Size;
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				var position = new Vector3(partWidth * i, 0, partWidth * j);
				var part = Instantiate(ForestPartPrefab, position, Quaternion.identity, transform);
				var partComp = part.GetComponent<ForestPart>();
				partComp.MyX = i;
				partComp.MyY = j;
				partComp.OtherParts = _parts;
				partComp.TerrainData = Terrain;
				partComp.Wind = SceneManager.WindBaseMatrix;				
				_parts[i, j] = partComp;
			}
		}
	}

	public void ClearSimulation()
	{
		foreach (var part in _parts) //foreach makes code more readable, here it's not performance critical
		{
			part.ClearPart();
		}
	}

	public void AddRandomFire()
	{
		foreach (var part in _parts)
		{
			part.AddRandomFire();
		}
	}

	public void AddTreeAt(Vector3 p)
	{
		GetForestPart(p).AddTreeAt(p);
	}
	
	public void AddFireAt(Vector3 p)
	{
		GetForestPart(p).AddFireAt(p);
	}
	
	public void RemoveTreeAt(Vector3 p)
	{
		GetForestPart(p).RemoveTreeAt(p);
	}
	
	public void ExtinguishAt(Vector3 p)
	{
		GetForestPart(p).ExtinguishAt(p);
	}
	
	/**
	 * Select correct part for a global position 
	 */
	private ForestPart GetForestPart(Vector3 p)
	{
		var i = Mathf.FloorToInt( p.x / ForestPart.Size );
		var j = Mathf.FloorToInt( p.z / ForestPart.Size );
		return _parts[i, j];
	}

	public void SetSimulationActive(bool value)
	{
		_isRunning = value;
	}
	
	public void WindChanged(float[,] windMatrix)
	{
		foreach (var part in _parts)
		{
			part.Wind = windMatrix;
		}
	}

	public void GenerateRandomForest()
	{
		var randX = Random.value * 10000;
		var randY = Random.value * 10000;
		foreach (var part in _parts)
		{
			part.PerlinOffsetX = randX;
			part.PerlinOffsetY = randY;
			part.InitRandomTerrain();
		}
	}

}