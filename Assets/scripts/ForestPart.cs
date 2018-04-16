using System.Collections.Generic;
using UnityEngine;

public class ForestPart : MonoBehaviour
{
	public float PerlinScale = 1 / 5f;
	public float PerlinOffsetX = 4894.98f;
	public float PerlinOffsetY = 6548.8f;
	public float TreeDensity = 10;
	public float TreeSize = 0.9f;

	public const int W = 60;
	private const byte F = 64;
	private const byte OUT = 255;
	private byte[][] _treeData; // 0 - no tree, 1 - fresh tree, 2-63 - hot tree, 64-254 burning tree, 255 burned tree

	private Mesh _areaMesh;
	private MeshFilter _meshFilter;

	private Color32[] _treeColors;

	private static readonly Color32 AliveTreeClr = new Color32(0, 255, 0, 255);
	private static readonly Color32 BurningTreeClr = new Color32(255, 0, 0, 255);
	private static readonly Color32 DeadTreeClr = new Color32(0, 0, 0, 255);

	[Range(0, 1)] public float FireSpreadSpeed = 0.6f;
	[Range(0, 1)] public float BurnSpeed = 0.6f;

	//Set by Forest.cs
	[HideInInspector] public int MyX;

	[HideInInspector] public int MyY;
	[HideInInspector] public ForestPart[,] OtherParts;
	[HideInInspector] public TerrainData TerrainData;

	// Use this for initialization
	void Awake()
	{
		_meshFilter = GetComponent<MeshFilter>();
		_treeData = new byte[W][];
		for (int i = 0; i < W; i++)
		{
			_treeData[i] = new byte[W];
		}
	}

	public void ClearPart()
	{
		_areaMesh = new Mesh();
		_meshFilter.mesh = _areaMesh;
	}

	public void InitPart()
	{
		GenerateTrees();
		MakeMesh();
	}

	private void Update()
	{
		UpdateFire();
		UpdateColors();
	}

	private void UpdateFire()
	{
		for (var x = 0; x < W; x++)
		{
			for (var y = 0; y < W; y++)
			{
				var tree = _treeData[x][y];
				if (tree == 0) continue;

				if (tree < F && Random.value < FireSpreadSpeed)
				{
					//add all neighbour fires
					float neigh = 0f;

					if (x > 0 && x < W - 1 && y > 0 && y < W - 1)
					{
						byte t; 
						t = _treeData[x + 1][y];
						if (t >= F && t < OUT) neigh++;
						t = _treeData[x - 1][y];
						if (t >= F && t < OUT) neigh++;
						t = _treeData[x][y + 1];
						if (t >= F && t < OUT) neigh++;
						t = _treeData[x][y - 1];
						if (t >= F && t < OUT) neigh++;

						t = _treeData[x + 1][y + 1];
						if (t >= F && t < OUT) neigh += 0.5f;
						t = _treeData[x - 1][y + 1];
						if (t >= F && t < OUT) neigh += 0.5f;
						t = _treeData[x + 1][y - 1];
						if (t >= F && t < OUT) neigh += 0.5f;
						t = _treeData[x - 1][y - 1];
						if (t >= F && t < OUT) neigh += 0.5f;
					}
					else
					{
						neigh += IsFire(x - 1, y) + IsFire(x, y - 1) + IsFire(x, y + 1) + IsFire(x + 1, y);
						neigh += 0.5f * (IsFire(x - 1, y + 1) + IsFire(x - 1, y + 1) + IsFire(x + 1, y + 1) + IsFire(x + 1, y + 1));
					}

					_treeData[x][y] += (byte) neigh;
				}

				if (tree >= F && tree < OUT && Random.value < BurnSpeed)
				{
					_treeData[x][y]++;
				}
			}
		}
	}

	public int IsFire(int x, int y)
	{
		if (x >= 0 && x < W && y >= 0 && y < W)
		{
			var tree = _treeData[x][y];
			return tree >= F && tree < OUT ? 1 : 0;
		}
		var newX = MyX;
		var newY = MyY;
		if (x < 0)
		{
			newX--;
			x += W;
		}
		else if (x >= W)
		{
			newX++;
			x -= W;
		}
		if (y < 0)
		{
			newY--;
			y += W;
		}
		else if (y >= W)
		{
			newY++;
			y -= W;
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
			var x = Random.Range(0, W);
			var y = Random.Range(0, W);
			if (_treeData[x][y] > 0 && _treeData[x][y] < F)
			{
				_treeData[x][y] = F;
				break;
			}
		}
		UpdateColors();
	}

	private void GenerateTrees()
	{
		for (var x = 0; x < W; x++)
		{
			for (var y = 0; y < W; y++)
			{
				_treeData[x][y] = (byte) (TerrainFormula(x, y) > 3f ? 1 : 0);
			}
		}
	}

	private float TerrainFormula(float x, float y)
	{
		return TreeDensity * Mathf.PerlinNoise(PerlinOffsetX + (x + W * MyX) * PerlinScale,
			       PerlinOffsetY + (y + W * MyY) * PerlinScale);
	}

	private void UpdateColors()
	{
		int i = 0;
		for (var x = 0; x < W; x++)
		{
			for (var y = 0; y < W; y++)
			{
				var tree = _treeData[x][y];
				if (tree == 0) continue;
				Color32 c = DeadTreeClr;
				if (tree < F)
				{
					c = AliveTreeClr;
				}
				else if (tree < OUT)
				{
					c = BurningTreeClr;
				}
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

		var up = Vector3.up * TreeSize * 2;
		var forward = Vector3.forward * TreeSize;
		var right = Vector3.right * TreeSize;

		var terrainSize = TerrainData.size.x;

		for (var x = 0; x < W; x++)
		{
			for (var y = 0; y < W; y++)
			{
				var tree = _treeData[x][y];
				if (tree == 0) continue;

				var h = TerrainData.GetInterpolatedHeight((MyX * W + x) / terrainSize, (MyY * W + y) / terrainSize);

				BuildFace(new Vector3(x, h, y), up, forward, false, verts, clrs, tris);
				BuildFace(new Vector3(x + TreeSize, h, y), up, forward, true, verts, clrs, tris);
				//	BuildFace(new Vector3(x, h, y), forward, right, false, verts, clrs, tris);// we don't ever see the bottom of the cube
				BuildFace(new Vector3(x, h + TreeSize * 2, y), forward, right, true, verts, clrs, tris);
				BuildFace(new Vector3(x, h, y), up, right, true, verts, clrs, tris);
				BuildFace(new Vector3(x, h, y + TreeSize), up, right, false, verts, clrs, tris);
			}
		}
		_treeColors = clrs.ToArray();
		_areaMesh.vertices = verts.ToArray();
		_areaMesh.triangles = tris.ToArray();
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
		var localX = Mathf.FloorToInt(worldCoords.x - W * MyX);
		var localY = Mathf.FloorToInt(worldCoords.z - W * MyY);
		if (_treeData[localX][localY] == 0)
		{
			_treeData[localX][localY] = 1;
		}
		MakeMesh();
		UpdateColors();
	}

	public void AddFireAt(Vector3 worldCoords)
	{
		var localX = Mathf.FloorToInt(worldCoords.x - W * MyX);
		var localY = Mathf.FloorToInt(worldCoords.z - W * MyY);
		if (_treeData[localX][localY] > 0 && _treeData[localX][localY] < F)
		{
			_treeData[localX][localY] = F;
		}
		MakeMesh();
		UpdateColors();
	}

	public void RemoveTreeAt(Vector3 worldCoords)
	{
		var localX = Mathf.FloorToInt(worldCoords.x - W * MyX);
		var localY = Mathf.FloorToInt(worldCoords.z - W * MyY);
		if (_treeData[localX][localY] > 0)
		{
			_treeData[localX][localY] = 0;
		}
		MakeMesh();
		UpdateColors();
	}

	public void ExtinguishAt(Vector3 worldCoords)
	{
		var localX = Mathf.FloorToInt(worldCoords.x - W * MyX);
		var localY = Mathf.FloorToInt(worldCoords.z - W * MyY);
		if (IsFire(localX, localY) == 1)
		{
			_treeData[localX][localY] = 1;
		}
		MakeMesh();
		UpdateColors();
	}
}