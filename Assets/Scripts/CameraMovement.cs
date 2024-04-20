using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform target;

    public float m_moveSpeed;
    public float m_zoomSpeed;
    public float m_zoomMin;
    public float m_zoomMax;

    private float _inputVertical, _inputHorizontal;
    private float _inputZoom;
    private Vector3 _offset;
    private bool _moved = false;
    private float _rotationVertical;
    private float _rotationHorizontal;

    private void Start()
    {
        _offset = new Vector3(0, 0, transform.position.z - target.position.z);
    }

    private void UpdateInputs()
    {
        if (Input.GetMouseButton(1))
        {
            _inputHorizontal = Input.GetAxis("Mouse X");
            _inputVertical = Input.GetAxis("Mouse Y");
        }
        else
        {
            _inputVertical = -Input.GetAxisRaw("Vertical");
            _inputHorizontal = -Input.GetAxisRaw("Horizontal");
        }
        _inputZoom = Input.GetAxisRaw("Mouse ScrollWheel");
    }

    private void Update()
    {
        UpdateInputs();

        _moved = _inputVertical != 0.0f || _inputHorizontal != 0.0f || _inputZoom != 0.0f;

        if (_moved)
        {
            Move();
        }
        
    }

    private void Move()
    {
        _offset.z += _inputZoom * Time.deltaTime * m_zoomSpeed;
        _offset.z = Mathf.Clamp(_offset.z, -Mathf.Abs(m_zoomMax), -Mathf.Abs(m_zoomMin));

        _rotationVertical += _inputVertical * Time.deltaTime * m_moveSpeed;
        _rotationVertical = Mathf.Clamp(_rotationVertical, -80, 80);

        _rotationHorizontal = _inputHorizontal + transform.localEulerAngles.y;

        transform.localEulerAngles = new Vector3(-_rotationVertical, _rotationHorizontal, 0);
        transform.position = transform.localRotation * _offset + target.position;
    }
}