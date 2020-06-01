using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CameraZoom : MonoBehaviour
{
    const float pinchTurnRatio = Mathf.PI / 2;
    Camera m_mainCamera;
    float m_touchesPrevPosDifference, m_touchesCurPosDifference, m_zoomModifier;
    
    Vector2 firstTouchPrevPos, secondTouchPrevPos;
    
    [SerializeField]
    float zoomModifierSpeed = 0.2f;

    [SerializeField] private TextMeshProUGUI textButtonMode;
    [SerializeField] private GameObject editLabel;
    private float startTime;
    public float speed = 5F; 
    private  Vector3 direction;
    private Vector3 touchStart;
    private Vector3 targetDirection;
    private float turnAngle;
    private float turnAngleDelta;
    private bool m_isRotationMode;

    private bool m_isEditLabalModel;
    // Use this for initialization
    void OnEnable () {
        m_mainCamera = Camera.main;
        CalculateCentroid();
    }
	   
    // Update is called once per frame
    private void Update()
    {
        if (Input.touchCount > 0 && !m_isEditLabalModel)
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
                            targetDirection = m_mainCamera.transform.position + direction * 0.5f;
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
                            break;
                        }
                    }
                    break;
                case 2:
                    Touch firstTouch = Input.GetTouch (0);
                    Touch secondTouch = Input.GetTouch (1);
                    turnAngle = turnAngleDelta = 0;
                    if (firstTouch.phase == TouchPhase.Moved || secondTouch.phase == TouchPhase.Moved)
                    {
                        firstTouchPrevPos = firstTouch.position - firstTouch.deltaPosition;
                        secondTouchPrevPos = secondTouch.position - secondTouch.deltaPosition;

                        m_touchesPrevPosDifference = (firstTouchPrevPos - secondTouchPrevPos).magnitude;
                        m_touchesCurPosDifference = (firstTouch.position - secondTouch.position).magnitude;

                        m_zoomModifier = (firstTouch.deltaPosition - secondTouch.deltaPosition).magnitude *
                                         zoomModifierSpeed;
                        if (m_isRotationMode)
                        {
                            var zRotationValue = m_mainCamera.transform.rotation;
                            // ... or check the delta angle between them ...
                            turnAngle = Angle(firstTouch.position, secondTouch.position);
                            float prevTurn = Angle(firstTouch.position - firstTouch.deltaPosition,
                                secondTouch.position - secondTouch.deltaPosition);
                            turnAngleDelta = Mathf.DeltaAngle(prevTurn, turnAngle);

                            // ... if it's greater than a minimum threshold, it's a turn!
                            if (Mathf.Abs(turnAngleDelta) > 0)
                            {
                                turnAngleDelta *= pinchTurnRatio;
                            }
                            else
                            {
                                turnAngle = turnAngleDelta = 0;
                            }

                            if (Mathf.Abs(turnAngleDelta) > 0)
                            {
                                Vector3 rotationDeg = Vector3.zero;
                                rotationDeg.z = -turnAngleDelta;
                                zRotationValue *= Quaternion.Euler(rotationDeg);
                                m_mainCamera.transform.rotation = zRotationValue;
                            }

                        }
                        else
                        {
                            if (m_touchesPrevPosDifference > m_touchesCurPosDifference)
                                m_mainCamera.fieldOfView += m_zoomModifier;
                            if (m_touchesPrevPosDifference < m_touchesCurPosDifference)
                                m_mainCamera.fieldOfView -= m_zoomModifier;
                        }
                    }
                    break;
            }
        }
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
        var count = 0;
        List<GameObject> goList = new List<GameObject>();
        for (var i = 0; i < go.transform.childCount; i++)
        {
            if (go.transform.GetChild(i).gameObject.activeSelf)
            {
                if (go.transform.GetChild(i).gameObject.CompareTag("Dot"))
                {
                    centroid += go.transform.GetChild(i).position;
                    goList.Add(go.transform.GetChild(i).gameObject);
                    count++;
                }
            }
        }
        var position = FindCenterPoint(goList.ToArray());

        Debug.Log("count Line"+ count);
        centroid /= (count);
        m_mainCamera.transform.position = new Vector3(centroid.x,centroid.y,-10);
    }

    public void OnClickRotation()
    {
        m_isRotationMode = !m_isRotationMode;
        textButtonMode.text = !m_isRotationMode ? "Rotation Mode" : "Zoom Mode";
    }

    public void OnClickEditLabelMode()
    {
        m_isEditLabalModel = !m_isEditLabalModel;
        if (m_isEditLabalModel)
        {
            var image = editLabel.GetComponent<Image>();
              image.color = Color.red;
        }
        else
        {
            var image = editLabel.GetComponent<Image>();
            image.color = Color.white; 
        }
    }

     private float Angle (Vector2 pos1, Vector2 pos2) {
        Vector2 from = pos2 - pos1;
        Vector2 to = new Vector2(1, 0);
 
        float result = Vector2.Angle( from, to );
        Vector3 cross = Vector3.Cross( from, to );
 
        if (cross.z > 0) {
            result = 360f - result;
        }
 
        return result;
    }
     
     public Vector3  FindCenterPoint(GameObject[] gos) {
         Debug.Log("gos.Length "+ gos.Length);
         if (gos.Length == 0)
             return Vector3.zero;
         if (gos.Length == 1)
             return gos[0].transform.position;
         var bounds = new Bounds(gos[0].transform.position, Vector3.zero);
         for (var i = 1; i < gos.Length; i++)
             bounds.Encapsulate(gos[i].transform.position); 
         return bounds.center;
     }
}
