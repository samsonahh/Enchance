using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float _cameraSmoothTime;

    private Vector3 _offsetPosition;
    [SerializeField] private Transform _target;
    private float _zoom = 10f;

    [SerializeField] private Material _defaultSpriteMaterial;
    [SerializeField] private Material _transparentSpriteMaterial;
    private GameObject _lastObstructingObject;

    private void Start()
    {
        _offsetPosition = transform.position;
    }

    private void Update()
    {
        FollowPlayer();
        HandleZoom();
        MakeCoveringObjectsOpaque();
    }

    private void FollowPlayer()
    {
        transform.position = Vector3.Lerp(transform.position, _target.position + _offsetPosition, _cameraSmoothTime * Time.deltaTime);
    }

    private void HandleZoom()
    {
        _zoom -= Input.mouseScrollDelta.y;
        _zoom = Mathf.Clamp(_zoom, 3f, 10f);

        _offsetPosition = Vector3.Lerp(_offsetPosition, new Vector3(0, _zoom, -_zoom), 2f * _cameraSmoothTime * Time.deltaTime);
    }

    private void MakeCoveringObjectsOpaque()
    {
        Vector3 dirToPlayer = _target.position - transform.position;
        Ray ray = new Ray(transform.position, dirToPlayer);
        RaycastHit hit;

        if(_lastObstructingObject != null)
        {
            if(_lastObstructingObject.TryGetComponent(out Renderer renderer))
            {
                renderer.material = _defaultSpriteMaterial;
            }
            if (_lastObstructingObject.TryGetComponent(out SpriteRenderer sRenderer))
            {
                sRenderer.color = new Color(sRenderer.color.r, sRenderer.color.g, sRenderer.color.b, 1f);
            }
            _lastObstructingObject = null;
        }

        if(Physics.Raycast(ray, out hit))
        {
            if(hit.collider.gameObject != _target.gameObject)
            {
                GameObject hitObject = hit.collider.gameObject;
                _lastObstructingObject = hitObject;
                if(hitObject.TryGetComponent(out Renderer renderer))
                {
                    renderer.material = _transparentSpriteMaterial;
                }
                if (hitObject.TryGetComponent(out SpriteRenderer sRenderer))
                {
                    sRenderer.color = new Color(sRenderer.color.r, sRenderer.color.g, sRenderer.color.b, 0.35f);
                }
            }
        }
    }
}
