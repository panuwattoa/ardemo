using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
  
    Camera m_mainCamera;
    float m_touchesPrevPosDifference, m_touchesCurPosDifference, m_zoomModifier;
    
    Vector2 firstTouchPrevPos, secondTouchPrevPos;
    
    [SerializeField]
    float zoomModifierSpeed = 0.2f;
    private float startTime;
    public float speed = 5F; 
    private  Vector3 direction;
    private Vector3 touchStart;
    private Vector3 targetDirection;
    // Use this for initialization
    void OnEnable () {
        m_mainCamera = Camera.main;
        CalculateCentroid();
    }
	   
    // Update is called once per frame
    private void Update()
    {
        if (Input.touchCount > 0)
        {
            switch (Input.touchCount)
            {
                case 1:
                    var touch = Input.GetTouch(0);
                    switch (touch.phase)
                    {
                        case TouchPhase.Began:
                            touchStart = GetWorldPosition(0);
                            startTime = Time.time;
                            break;
                        case TouchPhase.Moved:
                        {
                            direction = touchStart - GetWorldPosition(0);
                            if (Math.Abs(direction.x) <= 0)
                            {
                                return;
                            }
                            targetDirection = m_mainCamera.transform.position + direction * 4f;
                            break;
                        }
                    }
                    break;
                case 2:
                    Touch firstTouch = Input.GetTouch (0);
                    Touch secondTouch = Input.GetTouch (1);
    
                    firstTouchPrevPos = firstTouch.position - firstTouch.deltaPosition;
                    secondTouchPrevPos = secondTouch.position - secondTouch.deltaPosition;
    
                    m_touchesPrevPosDifference = (firstTouchPrevPos - secondTouchPrevPos).magnitude;
                    m_touchesCurPosDifference = (firstTouch.position - secondTouch.position).magnitude;
    
                    m_zoomModifier = (firstTouch.deltaPosition - secondTouch.deltaPosition).magnitude * zoomModifierSpeed;
    
                    if (m_touchesPrevPosDifference > m_touchesCurPosDifference)
                        m_mainCamera.fieldOfView += m_zoomModifier;
                    if (m_touchesPrevPosDifference < m_touchesCurPosDifference)
                        m_mainCamera.fieldOfView -= m_zoomModifier;
                    break;
            }
        }
        float distCovered =  (Time.time - startTime);
        var timeTakenDuringLerp = 10 / speed;
        float fractionOfJourney = distCovered / timeTakenDuringLerp;
        if (distCovered > 2.5f) 
        {
            fractionOfJourney = 1;
            startTime = Time.time;
        }
        var position = m_mainCamera.transform.position;
        var lerp = Vector3.Lerp(position, targetDirection, fractionOfJourney);
        position = new Vector3(lerp.x, lerp.y,position.z);
        Vector3 clampMovement = position;
        clampMovement.x = Mathf.Clamp(clampMovement.x , -517f, 480f);
        clampMovement.y = Mathf.Clamp(clampMovement.y, -692, 664f);
        m_mainCamera.transform.position = clampMovement;
        m_mainCamera.fieldOfView = Mathf.Clamp (m_mainCamera.fieldOfView, 3f, 40.3f);
    }
    private Vector3 GetWorldPosition(float z)
    {
        Ray mousePos = m_mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane ground = new Plane(Vector3.forward, new Vector3(0, 0, z));
        float distance;
        ground.Raycast(mousePos, out distance);
        return mousePos.GetPoint(distance);
    }

    private void CalculateCentroid()
    {
        Vector3 centroid = Vector3.zero;
        var go = GameObject.FindWithTag("lineRender");
        foreach (Transform child  in go.transform)
        {
            centroid += child.position;
        }

        centroid /= (go.transform.childCount + 1);
        m_mainCamera.transform.position = centroid;
    }

}
