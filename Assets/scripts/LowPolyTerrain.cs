using System.Collections.Generic;
using UnityEngine;

internal class Tr
{
	public Vector3 V1;
	public Vector3 V2;
	public Vector3 V3;

	public Tr(Vector3 vv1, Vector3 vv2, Vector3 vv3)
	{
		V1 = vv1;
		V2 = vv2;
		V3 = vv3;
	}
}


public class LowPolyTerrain : MonoBehaviour
{
	// PUBLIC
	public Terrain Terrain;
	public float MaxTriangles = 1000;
	public Color TerrainColor = new Color(0.43f, 0.2f, 0.09f);

	// PRIVATE
	private int _heightMapResolution = 330;
	private Mesh _mesh;
	private List<Tr> _trisList;

	private TerrainData _data;

	private float[,] _heightMap;
	private float _terrainSize;

	// Use this for initialization
	void Start()
	{
		_data = Terrain.terrainData;
		_terrainSize = _data.size.x;
	
		_mesh = new Mesh();
		_trisList = new List<Tr>();

		var v1 = Vector3.zero;
		var v2 = Vector3.forward * _terrainSize;
		var v3 = new Vector3(_terrainSize, 0, _terrainSize);
		var v4 = Vector3.right * _terrainSize;
		_trisList.Add(new Tr(v1, v2, v3));
		_trisList.Add(new Tr(v1, v3, v4));

		while (_trisList.Count < MaxTriangles)
		{
			var t = new List<Tr>();
			foreach (var tr in _trisList)
			{
				BreakTriangle(tr, t);
			}
			_trisList = t;
		}

		foreach (var tr in _trisList)
		{
			tr.V1.y = GetY(tr.V1);
			tr.V2.y = GetY(tr.V2);
			tr.V3.y = GetY(tr.V3);
		}

		AddBase();
		RenderTrs();
		
		GetComponent<MeshCollider>().sharedMesh = _mesh;		
		
		CalculateHeightMap();
		
		Terrain.gameObject.SetActive(false);
	}

	private void CalculateHeightMap()
	{
		_heightMapResolution = Forest.Width * ForestPart.Size;
		_heightMap = new float[_heightMapResolution + 1, _heightMapResolution + 1];
		var scale = _heightMapResolution / _terrainSize;
		
		const int d = 8;

		for (var k = 0; k < _trisList.Count; k++)
		{
			var tr = _trisList[k];
			var x1 = tr.V1.x;
			var y1 = tr.V1.z;

			var x2 = tr.V2.x;
			var y2 = tr.V2.z;

			var x3 = tr.V3.x;
			var y3 = tr.V3.z;

			var ki = Mathf.RoundToInt((x1 + x2 + x3) / 3);
			var kj = Mathf.RoundToInt((y1 + y2 + y3) / 3);

			for (var i = -d; i < d; i++)
			{
				for (var j = -d; j < d; j++)
				{
					var x = (i + ki) * scale;
					var y = (j + kj) * scale;
					
					//Express in barycentric coordinates
					var den = (y2 - y3) * (x1 - x3) + (x3 - x2) * (y1 - y3);
					var a = ((y2 - y3) * (x - x3) + (x3 - x2) * (y - y3)) / den;
					var b = ((y3 - y1) * (x - x3) + (x1 - x3) * (y - y3)) / den;
					var c = 1 - a - b;

					if (0 <= a && a <= 1 && 0 <= b && b <= 1 && 0 <= c && c <= 1)
					{
						_heightMap[i + ki, j + kj] = (a * tr.V1 + b * tr.V2 + c * tr.V3).y;
					}
				}
			}
		}
	}

	public float GetHeight(int x, int y)
	{
		var scale = _heightMapResolution / _terrainSize;
		return _heightMap[Mathf.RoundToInt(x * scale), Mathf.RoundToInt(y * scale)];
	}


	private void AddBase()
	{
		var t = _trisList[0];
		var d1 = Mathf.Abs(t.V1.x - t.V2.x);
		d1 += Mathf.Abs(t.V1.z - t.V2.z);

		MakeWall(Vector3.zero, d1, Vector3.right, Vector3.forward * -1f);
		MakeWall(Vector3.zero, d1, Vector3.forward, Vector3.left);
		MakeWall(Vector3.forward * _data.size.x, d1, Vector3.right, Vector3.forward);
		MakeWall(new Vector3(1, 0, 1) * _data.size.x, d1, Vector3.forward * -1f, Vector3.right);
	}

	private void MakeWall(Vector3 start, float d, Vector3 dir, Vector3 n)
	{
		const float depth = 32f;  
		var last = start;
		var num = _data.size.x / d;
		for (int i = 0; i < num; i++)
		{
			var v1 = last;
			var v2 = last + dir * d;
			v2.y = GetY(v2);
			var v3 = last;
			v3.y = -depth;
			var v4 = last + dir * d;
			v4.y = -depth;

			_trisList.Add(FixTr(new Tr(v1, v2, v3), n));
			_trisList.Add(FixTr(new Tr(v2, v4, v3), n));
			last = v2;
		}
	}

	private Tr FixTr(Tr t, Vector3 n)
	{
		if (Vector3.Dot(n, Vector3.Cross(t.V2 - t.V1, t.V3 - t.V1)) < 0)
		{
			var v3 = t.V3;
			t.V3 = t.V2;
			t.V2 = v3;
		}
		return t;
	}

	private void BreakTriangle(Tr tr, List<Tr> list)
	{
		var t12 = tr.V1 * 0.5f + tr.V2 * 0.5f;
		var t23 = tr.V2 * 0.5f + tr.V3 * 0.5f;
		var t13 = tr.V1 * 0.5f + tr.V3 * 0.5f;

		list.Add(new Tr(t12, tr.V2, t23));
		list.Add(new Tr(tr.V1, t12, t13));
		list.Add(new Tr(t12, t23, t13));
		list.Add(new Tr(t13, t23, tr.V3));
	}

	private float GetY(Vector3 v)
	{
		var size = _data.size.x;
		return _data.GetInterpolatedHeight(v.x / size, v.z / size);
	}

	private void RenderTrs()
	{
		// Ensure correct order of vertices 				

		var n = Vector3.up;

		foreach (var t in _trisList)
		{
			if (Vector3.Dot(n, Vector3.Cross(t.V2 - t.V1, t.V3 - t.V1)) < 0)
			{
				var v3 = t.V3;
				t.V3 = t.V2;
				t.V2 = v3;
			}
		}

		// CONSTRUCT MESH

		var verts = new List<Vector3>();
		var tris = new List<int>();
		var colors = new List<Color>();
		var k = 0;
		foreach (var t in _trisList)
		{
			verts.Add(t.V1);
			verts.Add(t.V2);
			verts.Add(t.V3);
			tris.Add(k);
			tris.Add(k + 1);
			tris.Add(k + 2);
			colors.Add(TerrainColor);
			colors.Add(TerrainColor);
			colors.Add(TerrainColor);
			k += 3;
		}

		_mesh.vertices = verts.ToArray();
		_mesh.triangles = tris.ToArray();
		_mesh.colors = colors.ToArray();
		_mesh.RecalculateBounds();
		_mesh.RecalculateNormals();

		var meshFilter = GetComponent<MeshFilter>();
		meshFilter.mesh = _mesh;
	}
}