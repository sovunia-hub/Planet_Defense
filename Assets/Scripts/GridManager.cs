using UnityEngine;

public class GridManager : MonoBehaviour
{
    bool _isOccupied = false;
    GameObject _prefab;

    private void Start()
    {
        //_prefab = Resources.Load<GameObject>("Prefabs/2UPD");
    }
    private void OnMouseEnter()
    {
        GetComponent<MeshRenderer>().material.color = Color.green;
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!_isOccupied)
            {
                var verts = GetComponent<MeshFilter>().mesh.vertices;
                Vector3 Center = Vector3.zero;
                foreach (var v in verts)
                {
                    Center += v;
                }
                Center /= verts.Length;
                GameObject gameObject = Instantiate(_prefab);
                gameObject.transform.parent = transform;
                gameObject.transform.localPosition = Center;
                gameObject.transform.localScale = Vector3.one * 10;
                Vector3 normal = GetComponent<MeshFilter>().mesh.normals[GetComponent<MeshFilter>().mesh.triangles[0]];
                gameObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);
                _isOccupied = true;
            }
            else
            {
                _isOccupied = false;
                Destroy(transform.GetChild(0).gameObject);
            }
            
        }
    }

    private void OnMouseExit()
    {
        GetComponent<MeshRenderer>().material.color = Color.white;
    }
}
