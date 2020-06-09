using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DistanceButton : MonoBehaviour
{
    [SerializeField, Range(0, 3)] private float rotationRate = 0.2f;
    private bool dragging;
    public bool allowMove;
    [SerializeField] private Button distanceButton;
    // Start is called before the first frame update
    void OnTriggerEnter(Collider other1){
        if(other1.gameObject.CompareTag($"point"))
        {
            var image = distanceButton.image;
            var color = image.color;
            color.a = 0.2f;
            image.color = color;
            distanceButton.image = image;
        }
    }

    public void Update()
    {
        if (Input.touchCount > 0 && allowMove)
        {
            switch (Input.touchCount)
            {
                case 1:
                    if (dragging)
                    {
                        transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                    }
                    break;
                case 2:
                    var touch = Input.GetTouch(0);
                    transform.Rotate(0, 0, -touch.deltaPosition.x * rotationRate, Space.World);
                    break;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.CompareTag($"point"))
        {
            var image = distanceButton.image;
            var color = image.color;
            color.a = 1;
            image.color = color;
            distanceButton.image = image;
        }
    }
    
}
