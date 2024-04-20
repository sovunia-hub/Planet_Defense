using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class SphereGrid : MonoBehaviour
{
    [SerializeField] float m_Size = 100;
    [Range (1,4)][SerializeField] int m_Subdivisions = 3;
    [SerializeField] float m_Offset = -0.01f;
    [SerializeField] Material m_Material;

    void Start()
    {
        CreateHexasphere(m_Size, m_Offset, m_Subdivisions);
    }
    void CreateHexasphere(float size, float offset, int subdivisions)
    {
        HexagonSphere hex = new((size - offset) / 2, subdivisions, offset);
        List<FinalFace> finalFaces = hex.GetFinalFaces();
        int k = 0;

        foreach (FinalFace finalFace in finalFaces)
        {
            FinalStorage storage = new();
            Mesh mesh = new();

            List<Vector3> vertices = new();
            int i = 0;
            foreach (Face f in finalFace.GetFaces())
            {
                Vector3 vert = f.OffsetCentroid(finalFace);
                vertices.Add(vert);
                storage.AddData(vert, i++);
            }

            List<int> triangles = new();
            foreach (Vector3 t in finalFace.GetTriangles())
            {
                triangles.Add(storage.GetData(t));
            }

            List<Vector3> normals = new();
            for (int j = 0; j < finalFace.GetFaces().Length; j++)
            {
                normals.Add(finalFace.GetNormal());
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.normals = normals.ToArray();

            GameObject gameObject = new();
            gameObject.AddComponent<MeshFilter>();
            gameObject.AddComponent<MeshRenderer>();
            gameObject.AddComponent<MeshCollider>();
            gameObject.AddComponent<GridManager>();

            gameObject.GetComponent<MeshFilter>().mesh = mesh;
            gameObject.GetComponent<MeshRenderer>().material = m_Material;
            gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
            gameObject.name = "Hex" + k++;
            gameObject.transform.parent = transform;
        }
    }
}
public class HexagonSphere
{
    float _size;
    List<FinalFace> _finalFaces;
    Face[] _faces;
    Vector3[] _centroidPoints;

    public List<FinalFace> GetFinalFaces()
    {
        return _finalFaces;
    }
    public HexagonSphere(float size, int subdivisions, float offset)
    {

        this._size = size;
        _finalFaces = new List<FinalFace>();
        Storage storage = new();

        FillFaces();
        SubdivideFaces(subdivisions);

        foreach (Face face in _faces)
        {
            face.FixRadius(size);
            face.StorePoints(storage);
            face.SetOffset(offset);
        }
        _finalFaces = storage.FindShapeFaces();
    }
    void SubdivideFaces(int subdivisionAmt)
    {
        for (int i = 0; i < subdivisionAmt; i++)
        {
            Face[] newFaces = new Face[(_faces.Length * 4)];
            for (int f = 0; f < _faces.Length; f++)
            {
                Face face = _faces[f];
                Face[] subdivided = face.Subdivide();
                for (int l = 0; l < subdivided.Length; l++)
                {
                    newFaces[(f * 4) + l] = subdivided[l];
                }
            }
            _faces = newFaces;
        }
        _centroidPoints = new Vector3[_faces.Length];
        for (int i = 0; i < _faces.Length; i++)
        {
            _centroidPoints[i] = _faces[i].GetCentroidPoint();
        }
    }
    public void FillFaces()
    {
        Vector3[] vertices = new Vector3[12];
        float tau = 1.61803399f;

        vertices[0] = new Vector3(_size, tau * _size, 0);
        vertices[1] = new Vector3(-_size, tau * _size, 0);
        vertices[2] = new Vector3(_size, -tau * _size, 0);
        vertices[3] = new Vector3(-_size, -tau * _size, 0);

        vertices[4] = new Vector3(0, _size, tau * _size);
        vertices[5] = new Vector3(0, -_size, tau * _size);
        vertices[6] = new Vector3(0, _size, -tau * _size);
        vertices[7] = new Vector3(0, -_size, -tau * _size);

        vertices[8] = new Vector3(tau * _size, 0, _size);
        vertices[9] = new Vector3(-tau * _size, 0, _size);
        vertices[10] = new Vector3(tau * _size, 0, -_size);
        vertices[11] = new Vector3(-tau * _size, 0, -_size);

        _faces = new Face[20];
        _faces[0] = new Face(vertices[0], vertices[1], vertices[4]);
        _faces[1] = new Face(vertices[1], vertices[9], vertices[4]);
        _faces[2] = new Face(vertices[4], vertices[9], vertices[5]);
        _faces[3] = new Face(vertices[5], vertices[9], vertices[3]);
        _faces[4] = new Face(vertices[2], vertices[3], vertices[7]);
        _faces[5] = new Face(vertices[3], vertices[2], vertices[5]);
        _faces[6] = new Face(vertices[7], vertices[10], vertices[2]);
        _faces[7] = new Face(vertices[0], vertices[8], vertices[10]);
        _faces[8] = new Face(vertices[0], vertices[4], vertices[8]);
        _faces[9] = new Face(vertices[8], vertices[2], vertices[10]);
        _faces[10] = new Face(vertices[8], vertices[4], vertices[5]);
        _faces[11] = new Face(vertices[8], vertices[5], vertices[2]);
        _faces[12] = new Face(vertices[1], vertices[0], vertices[6]);
        _faces[13] = new Face(vertices[11], vertices[1], vertices[6]);
        _faces[14] = new Face(vertices[3], vertices[9], vertices[11]);
        _faces[15] = new Face(vertices[6], vertices[10], vertices[7]);
        _faces[16] = new Face(vertices[3], vertices[11], vertices[7]);
        _faces[17] = new Face(vertices[11], vertices[6], vertices[7]);
        _faces[18] = new Face(vertices[6], vertices[0], vertices[10]);
        _faces[19] = new Face(vertices[9], vertices[1], vertices[11]);
    }
}
public class Storage
{
    private Dictionary<Vector3, List<Face>> _data = new();

    public void AddPoint(Vector3 point, Face face)
    {
        if (_data.ContainsKey(point))
        {
            _data[point].Add(face);
        }
        else
        {
            List<Face> al = new() { face };
            _data.Add(point, al);
        }
    }
    public List<FinalFace> FindShapeFaces()
    {
        List<FinalFace> finalFaces = new();
        foreach (KeyValuePair<Vector3, List<Face>> pair in _data)
        {
            List<Face> list = _data[pair.Key];
            int size = list.Count;
            if (size >= 5)
            {
                finalFaces.Add(new FinalFace(list, pair.Key));
            }
        }
        return finalFaces;
    }
}
public class FinalFace
{
    List<Face> _faces = new();
    Vector3 _normal;

    public FinalFace(List<Face> faces, Vector3 centerPoint)
    {
        this._faces = faces;
        RearangeFaces();
        _normal = new Vector3(centerPoint.x, centerPoint.y, centerPoint.z).normalized;
    }
    public Vector3 GetNormal()
    {
        return _normal;
    }
    public Face[] GetFaces()
    {
        return _faces.ToArray();
    }
    public Vector3[] GetTriangles()
    {
        List<Vector3> array = new();

        Vector3 P = _faces[2].OffsetCentroid(this);
        Vector3 Q = _faces[1].OffsetCentroid(this);
        Vector3 R = _faces[4].OffsetCentroid(this);

        Vector3 PR = R - P;
        Vector3 PQ = Q - P;

        Vector3 cross = Vector3.Cross(PR, PQ);


        bool frontFace = Vector3.Dot(cross, _normal) > 0.15f;


        if (_faces.Count == 5)
        {
            if (frontFace)
            {
                array.Add(_faces[0].OffsetCentroid(this));
                array.Add(_faces[1].OffsetCentroid(this));
                array.Add(_faces[2].OffsetCentroid(this));

                array.Add(_faces[2].OffsetCentroid(this));
                array.Add(_faces[3].OffsetCentroid(this));
                array.Add(_faces[4].OffsetCentroid(this));

                array.Add(_faces[4].OffsetCentroid(this));
                array.Add(_faces[0].OffsetCentroid(this));
                array.Add(_faces[2].OffsetCentroid(this));
            }
            else
            {
                array.Add(_faces[0].OffsetCentroid(this));
                array.Add(_faces[4].OffsetCentroid(this));
                array.Add(_faces[1].OffsetCentroid(this));

                array.Add(_faces[4].OffsetCentroid(this));
                array.Add(_faces[3].OffsetCentroid(this));
                array.Add(_faces[2].OffsetCentroid(this));

                array.Add(_faces[2].OffsetCentroid(this));
                array.Add(_faces[1].OffsetCentroid(this));
                array.Add(_faces[4].OffsetCentroid(this));
            }


        }
        else if (_faces.Count == 6)
        {
            if (frontFace)
            {
                array.Add(_faces[0].OffsetCentroid(this));
                array.Add(_faces[1].OffsetCentroid(this));
                array.Add(_faces[2].OffsetCentroid(this));

                array.Add(_faces[2].OffsetCentroid(this));
                array.Add(_faces[3].OffsetCentroid(this));
                array.Add(_faces[0].OffsetCentroid(this));

                array.Add(_faces[3].OffsetCentroid(this));
                array.Add(_faces[4].OffsetCentroid(this));
                array.Add(_faces[5].OffsetCentroid(this));

                array.Add(_faces[5].OffsetCentroid(this));
                array.Add(_faces[0].OffsetCentroid(this));
                array.Add(_faces[3].OffsetCentroid(this));
            }
            else
            {
                array.Add(_faces[0].OffsetCentroid(this));
                array.Add(_faces[5].OffsetCentroid(this));
                array.Add(_faces[1].OffsetCentroid(this));

                array.Add(_faces[5].OffsetCentroid(this));
                array.Add(_faces[4].OffsetCentroid(this));
                array.Add(_faces[3].OffsetCentroid(this));

                array.Add(_faces[3].OffsetCentroid(this));
                array.Add(_faces[2].OffsetCentroid(this));
                array.Add(_faces[1].OffsetCentroid(this));

                array.Add(_faces[1].OffsetCentroid(this));
                array.Add(_faces[5].OffsetCentroid(this));
                array.Add(_faces[3].OffsetCentroid(this));
            }
        }

        return array.ToArray();

    }
    public void RearangeFaces()
    {
        List<Face> rearanged = new (){ _faces[0] };
        Face lastFace = _faces[0];
        _faces.Remove(lastFace);
        while (_faces.Count > 0)
        {
            foreach (Face face in _faces)
            {
                Vector3[] lastFacePoints = { lastFace.p1, lastFace.p2, lastFace.p3 };
                int sharedPoints = 0;
                if (lastFacePoints.Contains(face.p1))
                    ++sharedPoints;
                if (lastFacePoints.Contains(face.p2))
                    ++sharedPoints;
                if (lastFacePoints.Contains(face.p3))
                    ++sharedPoints;
                if (sharedPoints == 2)
                {
                    rearanged.Add(face);
                    _faces.Remove(face);
                    lastFace = face;
                    break;
                }
            }
        }
        _faces = rearanged;
    }
    public Vector3 OffsetToRadius(Vector3 p, float sphereRadius)
    {
        float currentDistance = (p.x * p.x) + (p.y * p.y) + (p.z * p.z);
        float adjustment = (sphereRadius * sphereRadius) / currentDistance;
        return new Vector3(p.x * adjustment, p.y * adjustment, p.z * adjustment);
    }
}
public class Face
{
    float _offset = 5;
    public Vector3 p1;
    public Vector3 p2;
    public Vector3 p3;
    public Face[] neighbours = new Face[3];
    public void SetOffset(float offset)
    {
        this._offset = offset;
    }
    public Face(Vector3 point1, Vector3 point2, Vector3 point3)
    {
        this.p1 = point1;
        this.p2 = point2;
        this.p3 = point3;
    }

    public Face[] Subdivide()
    {
        Vector3 m1 = new((p1.x + p2.x) / 2, (p1.y + p2.y) / 2, (p1.z + p2.z) / 2);
        Vector3 m2 = new((p2.x + p3.x) / 2, (p2.y + p3.y) / 2, (p2.z + p3.z) / 2);
        Vector3 m3 = new((p3.x + p1.x) / 2, (p3.y + p1.y) / 2, (p3.z + p1.z) / 2);

        Face[] array = new Face[4];
        array[0] = new Face(m1, p1, m3);
        array[1] = new Face(m3, p3, m2);
        array[2] = new Face(m2, m1, m3);
        array[3] = new Face(p2, m1, m2);

        return array;
    }

    public Vector3 GetCentroidPoint()
    {
        return MultiplyVector(new((p1.x + p2.x + p3.x) / 3, (p1.y + p2.y + p3.y) / 3, (p1.z + p2.z + p3.z) / 3), 1.07f);
    }

    public Vector3 OffsetCentroid(FinalFace f)
    {
        return GetCentroidPoint() + (f.GetNormal() * _offset);
    }
    public float CorrectToRadius(float sphereRadius, Vector3 p)
    {
        float currentDistance = Mathf.Sqrt((p.x * p.x) + (p.y * p.y) + (p.z * p.z));
        float adjustment = sphereRadius / currentDistance;
        return adjustment;
    }
    public void FixRadius(float radius)
    {
        p1 = MultiplyVector(p1, CorrectToRadius(radius, p1));
        p2 = MultiplyVector(p2, CorrectToRadius(radius, p2));
        p3 = MultiplyVector(p3, CorrectToRadius(radius, p3));
    }
    public Vector3 MultiplyVector(Vector3 point, float multiplication)
    {
        return new Vector3(point.x * multiplication, point.y * multiplication, point.z * multiplication);
    }
    public void StorePoints(Storage storage)
    {
        storage.AddPoint(p1, this);
        storage.AddPoint(p2, this);
        storage.AddPoint(p3, this);
    }
}
public class FinalStorage
{
    Dictionary<Vector3, int> _data = new();
    public void AddData(Vector3 vec, int position)
    {
        _data.Add(vec, position);
    }
    public int GetData(Vector3 vec)
    {
        return _data[vec];
    }
}