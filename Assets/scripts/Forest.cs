using UnityEngine;

public class Forest : MonoBehaviour
{
	public GameObject ForestPartPrefab;

	public Terrain Terrain;

	public const int Width = 5; //In number of parts
	public const int Height = 5; //In number of parts

	private readonly ForestPart[,] _parts = new ForestPart[Width, Height];

	private void Start()
	{
		PrepareForestParts();
	}

	private void PrepareForestParts()
	{
		var PartW = ForestPart.W;
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				var position = new Vector3(PartW * i, 0, PartW * j);
				var part = Instantiate(ForestPartPrefab, position, Quaternion.identity, transform);
				var partComp = part.GetComponent<ForestPart>();
				partComp.MyX = i;
				partComp.MyY = j;
				partComp.OtherParts = _parts;
				partComp.TerrainData = Terrain.terrainData;
				partComp.Wind = SceneManager.WindBaseMatrix;
				partComp.enabled = false;
				_parts[i, j] = partComp;
			}
		}
	}

	public void ClearSimulation()
	{
		foreach (var part in _parts) //foreach makes code more readable, not performance critical
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

	private ForestPart GetForestPart(Vector3 p)
	{
		var i = Mathf.FloorToInt( p.x / ForestPart.W );
		var j = Mathf.FloorToInt( p.z / ForestPart.W );
		return _parts[i, j];
	}

	public void SetSimulationActive(bool value)
	{
		foreach (var part in _parts)
		{
			part.enabled = value;
		}
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
			part.InitPart();
		}
	}

}