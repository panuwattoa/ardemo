using System;
using System.Collections.Generic;
using JoystickLab;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
public class PlaceObjectsOnPlane : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Instantiates this prefab on a plane at the touch location.")]
    GameObject m_PlacedPrefab;

    /// <summary>
    /// The prefab to instantiate on touch.
    /// </summary>
    public GameObject placedPrefab
    {
        get { return m_PlacedPrefab; }
        set { m_PlacedPrefab = value; }
    }

    /// <summary>
    /// The object instantiated as a result of a successful raycast intersection with a plane.
    /// </summary>
    public GameObject spawnedObject { get; private set; }

    /// <summary>
    /// Invoked whenever an object is placed in on a plane.
    /// </summary>
    public static event Action onPlacedObject;
    private Pose placementPose; //Placement markers pose
    public GameObject dotPoint; // object representing the point
    public GameObject markerPoint; 
    private Camera fpsCamera;// AR Camera

    
    ARRaycastManager m_RaycastManager;

    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    void Awake()
    {
        m_RaycastManager = GetComponent<ARRaycastManager>();
    }
    
    void Start()
    {
        fpsCamera = Camera.main; // Initilize the camera in start to minimize "Camera.main" call multiple time inside code
    }


    void Update()
    {
        CalculateMarker(); // Continously calculates the placement marker Pos
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if(IsPointerOverUIObject(touch)) return;
                // If we have touched on top of a UI element, then we just return
                GameObject point = Instantiate(dotPoint, placementPose.position, Quaternion.identity);
                LineRendererDrawing.Instance.DrawLine(point,true);
                onPlacedObject?.Invoke();
            }
        }
    }

    void CalculateMarker()
    {
        TrackableType flags = TrackableType.PlaneWithinBounds | TrackableType.PlaneWithinPolygon;
        Vector2 origin = fpsCamera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f, 0));
        if (m_RaycastManager.Raycast(origin, s_Hits, flags))
        {
            Pose hitPose = s_Hits[0].pose;
            markerPoint.transform.position = hitPose.position;
            placementPose = s_Hits[0].pose;
            //Taking the center point object of the marker point. This is not important in this moment.
            //But might come handy when we do some animation like "Apple Measure" app
            GameObject markerPointObj = markerPoint.transform.GetChild(0).gameObject;               
            // The passed argument is false, because we touch here. That means it just temporary dotted line.
            LineRendererDrawing.Instance.DrawLine(markerPointObj,false);
        }
    }

    private bool IsPointerOverUIObject(Touch touch)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(touch.position.x, touch.position.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

}
