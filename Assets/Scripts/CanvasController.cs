using System;
using System.Collections;
using System.Collections.Generic;
using GOAP.Agent;
using UnityEngine;
using UnityEngine.UI;
using Debug = System.Diagnostics.Debug;

public class CanvasController : MonoBehaviour
{
    // private Canvas _canvas;
    private Transform _cameraTransform;
    [SerializeField] private Text text;
    [SerializeField] private PathPlanner planner;

    private void Awake()
    {
        text ??= GetComponentInChildren<Text>();
        planner ??= GetComponentInParent<PathPlanner>();
    }

    private void Start()
    {
        var canvas = GetComponent<Canvas>();
        Debug.Assert(Camera.main != null, "Camera.main != null");
        var main = Camera.main;
        var uiCamera = main.GetComponentInChildren<Camera>();
        _cameraTransform = uiCamera.transform; // Cache camera transform
        canvas.worldCamera = uiCamera;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(_cameraTransform);
        text.text = planner.PlanText;
    }
}
