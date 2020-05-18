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
    
    // Use this for initialization
    void OnEnable () {
        m_mainCamera = Camera.main;
    }
	   
    // Update is called once per frame
    private void Update()
    {
        if (Input.touchCount > 0)
        {
            switch (Input.touchCount)
            {
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
        m_mainCamera.fieldOfView = Mathf.Clamp (m_mainCamera.fieldOfView, 3f, 40.3f);
    }


}
