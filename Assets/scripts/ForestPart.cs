using System.Collections.Generic;
using UnityEngine;
/**
 * Forest Part
 * Can generated random trees using the Perlin noise function.
 * One mesh only. Colors are updated in one array, so 0 memory
 * allocations unless the whole mesh has to be rebuild (Add/Remove trees). 
 */
public class ForestPart : MonoBehaviour
{
	// PUBLIC
	
	//For generating random trees
	public float PerlinScale = 1 / 5f;
	public float PerlinOffsetX = 4894.98f;
	public float PerlinOffsetY = 6548.8f;
	public float TreeDensity = 10;
	public float TreeSize = 0.9f; //For values > 1, trees will overlap

	public const int Size = 55; //Size of the part, part is always square. If value > 57, the limit of 64k verts might be reached.

	public GameObject SmokePrefab;
	
	[Range(0, 1)] public float FireSpreadSpeed = 0.6f;
	[Range(0, 1)] public float BurnSpeed = 0.6f;
	
	//These values are set by Forest.cs
	[HideInInspector] public int MyX;
	[HideInInspector] public int MyY;
	[HideInInspector] public ForestPart[,] OtherParts;
	[HideInInspector] public LowPolyTerrain TerrainData;
	[HideInInspector] public float[,] Wind;
	
	// PRIVATE
	
	/* Data structure - jagged array performs about 10% faster then 2D array in this app.
	 * Bytes meaning: 0 - no tree, 1-63 - fresh tree, 64-254 burning tree, 255 burned tree
	 */
	private byte[][] _treeData; 
	private const byte Fire = 64;
	private const byte Burned = 255;

	private Mesh _areaMesh;
	private MeshFilter _meshFilter;

	private Color32[] _treeColors; //color for each vertex in the mesh  

	private static readonly Color32 AliveTreeClr = new Color32(0, 255, 0, 255);
	private static readonly Color32 AliveTreeClr1 = new Color32(0, 155, 0, 255);
	private static readonly Color32 AliveTreeClr2 = new Color32(0, 71, 0, 255);
	private static readonly Color32 BurningTreeClr = new Color32(255, 0, 0, 255);
	private static readonly Color32 DeadTreeClr = new Color32(0, 0, 0, 255);

	private byte[,] _colorNoise;
	private ParticleSystem _smoke;
	
	void Awake()
	{
		_meshFilter = GetComponent<MeshFilter>();
		_treeData = new byte[Size][];
		for (int i = 0; i < Size; i++)
		{
			_treeData[i] = new byte[Size];
		}
		ClearPart();

		//Prepare random values for the color variations
		var colorRandom = new MRandom {seed = (uint) (MyX * MyY)};
		_colorNoise = new byte[Size,Size];
		for (var x = 0; x < Size; x++)
		{
			for (var y = 0; y < Size; y++)
			{
				_colorNoise[x, y] = (byte) colorRandom.getRand(x, y);
			}
		}
	}

	void Start()
	{
		_smoke = Instantiate(SmokePrefab, transform).GetComponent<ParticleSystem>();
		
	}

	public void ClearPart()
	{
		_areaMesh = new Mesh();
		_meshFilter.mesh = _areaMesh;
		for (var x = 0; x < Size; x++)
		{
			for (var y = 0; y < Size; y++)
			{
				_treeData[x][y] = 0;
			}
		}
		_treeColors = null;
	}

	public void InitRandomTerrain()
	{
		GenerateTrees();
		MakeMesh();
		UpdateColors();
	}

	public void DoUpdate()
	{
		UpdateFire();
		UpdateColors();
	}

	private void UpdateFire()
	{
		for (var x = 0; x < Size; x++)
		{
			for (var y = 0; y < Size; y++)
			{
				var tree = _treeData[x][y];
				if (tree == 0) continue;

				if (tree < Fire && Random.value < FireSpreadSpeed)
				{
					//add up the influence of all neighbour fires
					float neigh = 0f;

					if (x > 0 && x < Size - 1 && y > 0 && y < Size - 1) //simple case
					{
						byte t;
						t = _treeData[x + 1][y];
						if (t >= Fire && t < Burned) neigh += Wind[2, 1];
						t = _treeData[x - 1][y];
						if (t >= Fire && t < Burned) neigh += Wind[0, 1];
						t = _treeData[x][y + 1];
						if (t >= Fire && t < Burned) neigh += Wind[1, 2];
						t = _treeData[x][y - 1];
						if (t >= Fire && t < Burned) neigh += Wind[1, 0];

						t = _treeData[x + 1][y + 1];
						if (t >= Fire && t < Burned) neigh += Wind[2, 2];
						t = _treeData[x - 1][y + 1];
						if (t >= Fire && t < Burned) neigh += Wind[0, 2];
						t = _treeData[x + 1][y - 1];
						if (t >= Fire && t < Burned) neigh += Wind[2, 0];
						t = _treeData[x - 1][y - 1];
						if (t >= Fire && t < Burned) neigh += Wind[0, 0];
					}
					else // we are on the border, we might have to look to neighbouring parts
					{
						neigh += Wind[0, 1] * IsFire(x - 1, y)
						         + Wind[1, 0] * IsFire(x, y - 1)
						         + Wind[1, 2] * IsFire(x, y + 1)
						         + Wind[2, 1] * IsFire(x + 1, y);

						neigh += Wind[2, 2] * IsFire(x + 1, y + 1)
						         + Wind[0, 2] * IsFire(x - 1, y + 1)
						         + Wind[2, 0] * IsFire(x + 1, y - 1)
						         + Wind[0, 0] * IsFire(x - 1, y - 1);
					}

					_treeData[x][y] += (byte) neigh;
				}

				if (tree >= Fire && tree < Burned && Random.value < BurnSpeed)
				{
					_treeData[x][y]++;
					if (_treeData[x][y] == Burned)
					{
						var h = TerrainData.GetHeight(MyX * Size + x, MyY * Size + y);
						_smoke.transform.position = new Vector3(x + Size * MyX, h + TreeSize*2, y + Size * MyY);
						_smoke.Emit(10);
					}
				}
			}
		}
	}

	public int IsFire(int x, int y)
	{
		if (x >= 0 && x < Size && y >= 0 && y < Size)
		{
			var tree = _treeData[x][y];
			return tree >= Fire && tree < Burned ? 1 : 0;
		}
		var newX = MyX;
		var newY = MyY;
		if (x < 0)
		{
			newX--;
			x += Size;
		}
		else if (x >= Size)
		{
			newX++;
			x -= Size;
		}
		if (y < 0)
		{
			newY--;
			y += Size;
		}
		else if (y >= Size)
		{
			newY++;
			y -= Size;
		}
		if (newX < 0 || newX >= Forest.Width || newY < 0 || newY >= Forest.Height)
		{
			return 0;
		}
		return OtherParts[newX, newY].IsFire(x, y);
	}

	public void AddRandomFire()
	{
		for (var i = 0; i < 150; i++)
		{
			var x = Random.Range(0, Size);
			var y = Random.Range(0, Size);
			if (_treeData[x][y] > 0 && _treeData[x][y] < Fire)
			{
				_treeData[x][y] = Fire;
				break;
			}
		}
		UpdateColors();
	}

	private void GenerateTrees()
	{
		for (var x = 0; x < Size; x++)
		{
			for (var y = 0; y < Size; y++)
			{
				_treeData[x][y] = (byte) (TerrainFormula(x, y) > 3f ? 1 : 0);
			}
		}
	}

	private float TerrainFormula(float x, float y)
	{
		return TreeDensity * Mathf.PerlinNoise(PerlinOffsetX + (x + Size * MyX) * PerlinScale,
			       PerlinOffsetY + (y + Size * MyY) * PerlinScale);
	}

	private void UpdateColors()
	{
		if (_treeColors == null) return;
		int i = 0;
		for (var x = 0; x < Size; x++)
		{
			for (var y = 0; y < Size; y++)
			{
				var tree = _treeData[x][y];
				if (tree == 0) continue;
				Color32 c = DeadTreeClr;
				if (tree < Fire)
				{
					c = AliveTreeClr;
					var k = _colorNoise[x,y];
					if (k == 1)
					{
						c = AliveTreeClr1;
					}else if (k == 2)
					{
						c = AliveTreeClr2;
					}
				}
				else if (tree < Burned)
				{
					c = BurningTreeClr;
				}
				//5 sides of the cube, each with 4 vertices
				for (var k = 0; k < 20; k++)
				{
					_treeColors[i + k] = c;
				}
				i += 20;
			}
		}

		_areaMesh.colors32 = _treeColors;
	}

	private void MakeMesh()
	{
		_areaMesh = new Mesh();

		var verts = new List<Vector3>();
		var tris = new List<int>();
		var clrs = new List<Color32>();

		var up = Vector3.up * TreeSize * 2; //trees are twice as high as wide
		var forward = Vector3.forward * TreeSize;
		var right = Vector3.right * TreeSize;

		for (var x = 0; x < Size; x++)
		{
			for (var y = 0; y < Size; y++)
			{
				var tree = _treeData[x][y];
				if (tree == 0) continue;

				var h = TerrainData.GetHeight(MyX * Size + x, MyY * Size + y);

				BuildFace(new Vector3(x, h, y), up, forward, false, verts, clrs, tris);
				BuildFace(new Vector3(x + TreeSize, h, y), up, forward, true, verts, clrs, tris);
				//	BuildFace(new Vector3(x, h, y), forward, right, false, verts, clrs, tris);// we don't ever see the bottom of the cube
				BuildFace(new Vector3(x, h + TreeSize * 2, y), forward, right, true, verts, clrs, tris);
				BuildFace(new Vector3(x, h, y), up, right, true, verts, clrs, tris);
				BuildFace(new Vector3(x, h, y + TreeSize), up, right, false, verts, clrs, tris);
			}
		}
		
		_areaMesh.vertices = verts.ToArray();
		_areaMesh.triangles = tris.ToArray();
		
		_treeColors = clrs.ToArray();
		_areaMesh.colors32 = _treeColors;
		
		_areaMesh.RecalculateBounds();
		_areaMesh.RecalculateNormals();

		_meshFilter.mesh = _areaMesh;
	}

	private void BuildFace(Vector3 corner, Vector3 up, Vector3 right, bool reversed,
		List<Vector3> verts, List<Color32> clrs, List<int> tris)
	{
		int index = verts.Count;

		//Vertices
		verts.Add(corner);
		verts.Add(corner + up);
		verts.Add(corner + up + right);
		verts.Add(corner + right);

		//Triangles
		if (reversed)
		{
			tris.Add(index + 0);
			tris.Add(index + 1);
			tris.Add(index + 2);
			tris.Add(index + 2);
			tris.Add(index + 3);
			tris.Add(index + 0);
		}
		else
		{
			tris.Add(index + 1);
			tris.Add(index + 0);
			tris.Add(index + 2);
			tris.Add(index + 3);
			tris.Add(index + 2);
			tris.Add(index + 0);
		}

		//Colors
		clrs.Add(AliveTreeClr);
		clrs.Add(AliveTreeClr);
		clrs.Add(AliveTreeClr);
		clrs.Add(AliveTreeClr);
	}

	public void AddTreeAt(Vector3 worldCoords)
	{
		var localX = Mathf.FloorToInt(worldCoords.x - Size * MyX);
		var localY = Mathf.FloorToInt(worldCoords.z - Size * MyY);
		if (_treeData[localX][localY] == 0)
		{
			_treeData[localX][localY] = 1;
		}
		MakeMesh();
		UpdateColors();
	}

	public void AddFireAt(Vector3 worldCoords)
	{
		var localX = Mathf.FloorToInt(worldCoords.x - Size * MyX);
		var localY = Mathf.FloorToInt(worldCoords.z - Size * MyY);
		if (_treeData[localX][localY] > 0 && _treeData[localX][localY] < Fire)
		{
			_treeData[localX][localY] = Fire;
		}		
		UpdateColors();
	}

	public void RemoveTreeAt(Vector3 worldCoords)
	{
		var localX = Mathf.FloorToInt(worldCoords.x - Size * MyX);
		var localY = Mathf.FloorToInt(worldCoords.z - Size * MyY);
		if (_treeData[localX][localY] > 0)
		{
			_treeData[localX][localY] = 0;
			MakeMesh();
		}
		UpdateColors();
	}

	public void ExtinguishAt(Vector3 worldCoords)
	{
		var x = Mathf.FloorToInt(worldCoords.x - Size * MyX);
		var y = Mathf.FloorToInt(worldCoords.z - Size * MyY);
		if (IsFire(x, y) == 1)
		{
			_treeData[x][y] = 1;
		}
		
		//Extinguishing just one tree is ineffective, if possible, do a group of them
		if (x > 0 && x < Size - 1 && y > 0 && y < Size - 1)
		{
			if (IsFire(x - 1, y) == 1) _treeData[x - 1][y] = 1;
			if (IsFire(x + 1, y) == 1) _treeData[x + 1][y] = 1;
			if (IsFire(x, y + 1) == 1) _treeData[x][y + 1] = 1;
			if (IsFire(x, y - 1) == 1) _treeData[x][y - 1] = 1;
			if (IsFire(x + 1, y + 1) == 1) _treeData[x + 1][y + 1] = 1;
			if (IsFire(x - 1, y - 1) == 1) _treeData[x - 1][y - 1] = 1;
			if (IsFire(x + 1, y - 1) == 1) _treeData[x + 1][y - 1] = 1;
			if (IsFire(x - 1, y + 1) == 1) _treeData[x - 1][y + 1] = 1;
		}

		UpdateColors();
	}
}