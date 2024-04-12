using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float _cameraSmoothTime;

    private Vector3 _offsetPosition;
    [SerializeField] private Transform _target;
    private float _zoom = 10f;

    [SerializeField] private LayerMask _cameraCullLayer;
    [SerializeField] private Material _transparentSpriteMaterial;
    [SerializeField] private Material _transparent3DMaterial;
    private Dictionary<GameObject, Material> _lastObstructingObjects;

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
        RaycastHit[] hits;

        if (_lastObstructingObjects != null)
        {
            if (_lastObstructingObjects.Count != 0)
            {
                foreach (var thing in _lastObstructingObjects.Keys)
                {
                    if (thing == null) continue;
                    if (thing.TryGetComponent(out Renderer renderer))
                    {
                        if (thing.tag == "Environment")
                        {
                            renderer.material = _lastObstructingObjects[thing];
                        }
                        else
                        {
                            renderer.material = _lastObstructingObjects[thing];
                        }
                    }
                    if (thing.TryGetComponent(out SpriteRenderer sRenderer))
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
                        else
                        {
                            _lastObstructingObjects.Add(hitObject, renderer.material);
                            renderer.material = _transparentSpriteMaterial;
                        }
                    }
                    if (hitObject.TryGetComponent(out SpriteRenderer sRenderer))
                    {
                        sRenderer.color = new Color(sRenderer.color.r, sRenderer.color.g, sRenderer.color.b, 0.35f);
                    }
                }
            }
        }
    }
}
