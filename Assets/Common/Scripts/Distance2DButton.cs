using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Distance2DButton : EventTrigger
{
    [SerializeField, Range(0, 3)] private float rotationRate = 0.2f;
    private bool dragging;
    [SerializeField] private Button distanceButton;
    // Start is called before the first frame update
    private float startTime;
    public float speed = 0.8F; 
    private  Vector3 direction;
    private Vector3 touchStart;
    private Vector3 targetDirection;
    public void Update()
    {
        if (Input.touchCount > 0)
        {
            switch (Input.touchCount)
            {
                case 1:
                    if (dragging)
                    {
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
                                targetDirection = transform.position - direction;
                                float distCovered =  (Time.time - startTime);
                                var timeTakenDuringLerp = 10 / speed;
                                float fractionOfJourney = distCovered / timeTakenDuringLerp;
                                if (distCovered > 2.5f) 
                                {
                                    fractionOfJourney = 1;
                                    startTime = Time.time;
                                }
                                var position = transform.position;
                                var lerp = Vector3.Lerp(position, targetDirection, fractionOfJourney);
                                position = new Vector3(lerp.x, lerp.y,position.z);
                                transform.position = position;
                                break;
                            }
                        }
                        Debug.Log("dragging");
                    }
                    break;
                case 2:
                {
                    // var touch = Input.GetTouch(0);
                    // transform.Rotate(0, 0, touch.deltaPosition.x * rotationRate, Space.World);
                    break;
                }
            }
        }
    }


    
    public override void OnPointerDown(PointerEventData eventData) {
        dragging = true;
    }

    public override void OnPointerUp(PointerEventData eventData) {
        dragging = false;
    }
    
    private Vector3 GetWorldPosition(float z)
    {
        Ray mousePos = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane ground = new Plane(Vector3.forward, new Vector3(0, 0, z));
        float distance;
        ground.Raycast(mousePos, out distance);
        return mousePos.GetPoint(distance);
    }
}
