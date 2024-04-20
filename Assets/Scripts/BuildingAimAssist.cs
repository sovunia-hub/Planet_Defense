using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingAimAssist : MonoBehaviour
{
    [SerializeField] float m_range;
    [SerializeField] Transform m_partToRotate;
    [SerializeField] Transform m_partToShoot;
    [SerializeField] GameObject m_bullet;
    [SerializeField] float m_bulletSpeed;
    [SerializeField] float m_fireRate;

    private GameObject _target = null;
    private Transform _planet;
    private float _planetRange;

    void Start()
    {
        _planet = GameObject.FindGameObjectWithTag("Player").transform;
        _planetRange = Vector3.Distance(transform.position, _planet.transform.position);
        StartCoroutine(ShootCoroutine());
    }

    void Update()
    {
        FindTarget();
        if (_target != null)
        {
            Shoot();
        }
    }

    void FindTarget()
    { 
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject target = null;
        float minDist = Mathf.Infinity;
        float dist;

        _target = null;

        foreach (GameObject enemy in enemies)
        {
            dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                if (Vector3.Distance(_planet.transform.position, enemy.transform.position) > _planetRange)
                {
                    minDist = dist;
                    target = enemy;
                }
            }
        }

        if (minDist < m_range)
        {
            _target = target;
        }
    }

    IEnumerator ShootCoroutine()
    {
        while (true)
        {
            if (_target != null)
            {
                GameObject currBullet = Instantiate(m_bullet, m_partToShoot);
                currBullet.GetComponent<Rigidbody>().AddForce(m_bulletSpeed * Time.deltaTime * (_target.transform.position - transform.position));
                yield return new WaitForSeconds(m_fireRate);
            }
        }
    }

    void Shoot() 
    {
        if (_target == null)
            return;
        Vector3 rotation = Quaternion.LookRotation(_target.transform.position - transform.position).eulerAngles;
        m_partToRotate.rotation = Quaternion.Euler(rotation.x + 135, rotation.y, 0f);
    }
}