using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CameraMovement : NetworkBehaviour
{
    public static CameraMovement Instance;
    [SerializeField] private float _cameraSmoothTime;

    private Vector3 _offsetPosition;
    public Transform Target;
    private float _zoom = 10f;

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
        if (Target == null) return;

        FollowPlayer();
        HandleZoom();
        MakeCoveringObjectsOpaque();
    }

    private void FollowPlayer()
    {
        transform.position = Vector3.Lerp(transform.position, Target.position + _offsetPosition, _cameraSmoothTime * Time.deltaTime);
    }

    private void HandleZoom()
    {
        if (GameManager.Instance.State != GameState.Playing) return;
        _zoom -= Input.mouseScrollDelta.y;
        _zoom = Mathf.Clamp(_zoom, 3f, 10f);

        _offsetPosition = Vector3.Lerp(_offsetPosition, new Vector3(0, _zoom, -_zoom), 2f * _cameraSmoothTime * Time.deltaTime);
    }

    private void MakeCoveringObjectsOpaque()
    {
        Vector3 dirToPlayer = Target.position - transform.position;
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
                if (hit.collider.gameObject != Target.gameObject)
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
