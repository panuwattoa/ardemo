using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DistanceButton : MonoBehaviour
{
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
