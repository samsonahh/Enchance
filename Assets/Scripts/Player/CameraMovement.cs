using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public static CameraMovement Instance;

    [SerializeField] private float _cameraSmoothTime;

    private Vector3 _offsetPosition;
    [SerializeField] private Transform _target;
    private float _zoom = 10f;
    private float _maxZoom = 10f;
    private float _minZoom = 3f;

    [SerializeField] private LayerMask _cameraCullLayer;
    [SerializeField] private Material _transparentSpriteMaterial;
    [SerializeField] private Material _transparent3DMaterial;
    private Dictionary<GameObject, Material> _lastObstructingObjects;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _offsetPosition = transform.position;
        _lastObstructingObjects = new Dictionary<GameObject, Material>();
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

    public void InstantlyFixPosition()
    {
        transform.position = _target.position + _offsetPosition;
    }

    private void HandleZoom()
    {
        if (GameManager.Instance.State != GameState.Playing) return;
        _zoom -= Input.mouseScrollDelta.y;
        _zoom = Mathf.Clamp(_zoom, _minZoom, _maxZoom);

        _offsetPosition = Vector3.Lerp(_offsetPosition, new Vector3(0, _zoom, -_zoom), 2f * _cameraSmoothTime * Time.deltaTime);
    }

    public void SetZoom(float zoom)
    {
        if (zoom > _maxZoom) _maxZoom = zoom;

        _zoom = zoom;
    }

    public void ResetMaxZoom()
    {
        _maxZoom = 10f;

        if(_zoom > _maxZoom) _zoom = _maxZoom;
    }

    private void MakeCoveringObjectsOpaque()
    {
        Vector3 dirToPlayer = _target.position - transform.position;
        Ray ray = new Ray(transform.position, dirToPlayer);
        RaycastHit[] hits;

        if (_lastObstructingObjects != null)
        {
            if (_lastObstructingObjects.Count != 0)
            {
                foreach (var o in _lastObstructingObjects.Keys)
                {
                    if (o == null) continue;
                    if (o.TryGetComponent(out Renderer renderer))
                    {
                        renderer.material = _lastObstructingObjects[o];
                    }
                    if (o.TryGetComponent(out SpriteRenderer sRenderer))
                    {
                        sRenderer.color = new Color(sRenderer.color.r, sRenderer.color.g, sRenderer.color.b, 1f);
                    }
                }

                _lastObstructingObjects.Clear();
            }
        }

        hits = Physics.RaycastAll(ray, dirToPlayer.magnitude, _cameraCullLayer);
        if(hits != null)
        {
            foreach(var hit in hits)
            {
                if (hit.collider.gameObject == null) continue;
                if (hit.collider.gameObject != _target.gameObject)
                {
                    GameObject hitObject = hit.collider.gameObject;

                    if (hitObject.TryGetComponent(out Renderer renderer))
                    {
                        if (hitObject.tag == "Environment")
                        {
                            _lastObstructingObjects.Add(hitObject, renderer.material);
                            renderer.material = _transparent3DMaterial;
                        }
                    }
                    if (hitObject.TryGetComponent(out SpriteRenderer sRenderer))
                    {
                        _lastObstructingObjects.Add(hitObject, renderer.material);
                        renderer.material = _transparentSpriteMaterial;
                        sRenderer.color = new Color(sRenderer.color.r, sRenderer.color.g, sRenderer.color.b, 0.35f);
                    }
                }
            }
        }
    }
}
