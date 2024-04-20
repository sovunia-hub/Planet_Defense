using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] float m_moveSpeed;
    private Vector3 _target;
    void Start()
    {
        _target = FindObjectOfType<SphereGrid>().transform.position;
        transform.rotation = Quaternion.FromToRotation(Vector3.left, _target - transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<Rigidbody>().AddForce((_target - transform.position) * Time.deltaTime * m_moveSpeed);
    }
}
