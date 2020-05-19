using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lightSaberRenderer : MonoBehaviour
{
    private LineRenderer lineRenderer;
    [SerializeField]
    private Transform start, end;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        
        lineRenderer.SetPosition(0, start.position);
        lineRenderer.SetPosition(1, end.position);

    }
}
