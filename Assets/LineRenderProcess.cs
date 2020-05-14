using System;
using System.Collections;
using System.Collections.Generic;
using JoystickLab;
using TMPro;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class LineRenderProcess : MonoBehaviour
{
    public UILineRenderer lineRendererUI;
    public CaptureScreenshot captureScreenshot;
    public GameObject distanceTextButton;
    public GameObject canvas;
    public Material dottedLineMaterial;

    LineRenderer lineRenderer;

    public void Start()
    {
        lineRenderer = new LineRenderer();
        var pointList = new List<Vector2>();
        foreach (var pointStack in LineRendererDrawing.Instance.pointStack)
        {
            Debug.Log("position x " + pointStack.position.x + "position y  " + pointStack.position.y);
            var position = pointStack.position;
            pointList.Add(position);
        }
        // Vector2 pointMock1 = new Vector2(-1.126753f,0.5367763f);
        // pointList.Add(pointMock1);
        // Vector2 pointMock2 = new Vector2(-1.042109f,-0.006137069f);
        // pointList.Add(pointMock2);
        //
        // Vector2 pointMock3 = new Vector2(0.8175911f,0.4966181f);
        // pointList.Add(pointMock3);
        //
        // Vector2 pointMock4 = new Vector2(0.7839596f,1.154986f);
        // pointList.Add(pointMock4);
        //
        // Vector2 pointMock5 = new Vector2(-1.165905f,0.5661562f);
        // pointList.Add(pointMock5);

        
        lineRendererUI.Points = pointList.ToArray();
        for (var i = 1; i< pointList.Count; i++)
        {
               Debug.Log("Render line");
               GameObject point = new GameObject();
               lineRenderer = point.AddComponent<LineRenderer>();
               point.transform.position = pointList[i - 1];
               lineRenderer.startWidth = lineRenderer.endWidth = 0.009f;
           // DrawLength(lineRendererUI.Points[i],lineRendererUI.Points[i - 1], 33.3f);
                lineRenderer.material = dottedLineMaterial;
                lineRenderer.textureMode = LineTextureMode.Tile;
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, pointList[i - 1]);
                lineRenderer.SetPosition(1, pointList[i]);
                 DrawLength(pointList[i - 1], pointList[i], LineRendererDrawing.Instance.lineDistance.Pop());
        }
        captureScreenshot.Capture();
    }

    void DrawLength(Vector2 pointOne, Vector2 pointTwo, float distance)
    {
        string unit;
        Vector2 point = (pointOne + pointTwo) / 2;
        Debug.Log("Point x "+point.x + " point y "  + point.y);
        // point.y += 0.02f;
        var textObj = Instantiate(distanceTextButton.gameObject, point, Quaternion.identity);
        // textObj.transform.SetParent(lineRenderer.transform);
        var finalOutputText = LineRendererDrawing.Instance.processDistance(distance, out unit);
        textObj.GetComponentInChildren<TextMeshProUGUI>().text = finalOutputText + " <size=2>" + unit;
    }
}
