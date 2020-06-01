using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleSnap : MonoBehaviour
{
        void OnTriggerEnter(Collider other1){
            if(other1.gameObject.CompareTag($"line"))
            {
                Debug.Log("OnTriggerEnter CircleSnap ");
                var o = gameObject.transform.GetChild(0);
                var position = o.transform.position;
                position = new Vector3(other1.transform.position.x, position.y,other1.transform.position.z);
                gameObject.transform.GetChild(0).position = position;
            }
        }

}
