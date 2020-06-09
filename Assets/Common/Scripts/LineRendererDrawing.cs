using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

namespace JoystickLab
{
    public enum pointType
    {
        Wall = 0,
        VoidOut
    }
    public struct LineProp
    {
        public GameObject point;
        public GameObject distanceText;
        public Vector3 position;
        public pointType PointType;
        public LineProp(GameObject point, GameObject distanceText,Vector3 position,pointType pointType)
        {
            this.point = point;
            this.distanceText = distanceText;
            this.position = position;
            PointType = pointType;
        }
        
        public void SetEnable(bool enabled)
        {
            if (distanceText != null)
            {
                distanceText.SetActive(enabled);
            }            
            point.SetActive(enabled);
        }
    }
   
    public class LineRendererDrawing : SingleToneManager<LineRendererDrawing>
    {
        public Material lineMaterial;
        public Material DoorMaterial;
        public Material dottedLineMaterial;
        public GameObject cameraCapture;
        //public TextMesh distanceText;
        public GameObject distanceTextButton;
        public GameObject distanceText2DButton;
        public UnitConverter distanceUIBox;
        public GameObject exportObject;
        private List<Transform> clickPoints;
        private List<Transform> click2DPoints;
        private List<Transform> pointPosition;
        private List<Vector3> clickPositions;
        public float lineWidth;

        GameObject textObj = null;
        LineRenderer lineRenderer;

        public Stack<LineProp> pointStack;
        public Stack<LineProp> point2DStack;
        public Stack<LineProp> door2dpoint;
        public Stack<float> lineDistance;
        public ARPlaneManager PlaneManager;
        public Texture material;
        public Texture dotMaterial;
        public Material planMat;
        private bool isDisCrete;
        private bool isFromSanp;
        private bool IsDisCreteFromSnap;
        private Vector3 coppyPosition;
        private GameObject originalPoint;
        private bool m_isDrawDoor;
        private float dist = Mathf.Infinity;
        private bool isCustomMaterial;
        private float yBeginning;
        public bool IsDisCrete
        {
            private get { return isDisCrete; }
            set
            {
                isDisCrete = value;
                textObj.SetActive(false);
            }
        }
        // Use this for initialization
        void Start()
        {
            //lineRenderer = GetComponent<LineRenderer>();
            clickPoints = new List<Transform>();
            click2DPoints = new List<Transform>();
            lineRenderer = new LineRenderer();
            pointStack = new Stack<LineProp>();
            point2DStack = new Stack<LineProp>();
            lineDistance = new Stack<float>();
            pointPosition = new List<Transform>();
            textObj = Instantiate(distanceTextButton.gameObject, Vector3.zero, Quaternion.identity);
            textObj.name = "DottedDistanceLabel";
            textObj.SetActive(false);
        }
        
         
        
        public float DrawLine(GameObject point, bool done,GameObject dotPoint)
        {
            if (pointStack.Count > 0)
            {
                point.transform.position = new Vector3(point.transform.position.x,yBeginning,point.transform.position.z);
            }
            float distance = 0;
            int lastIndex = clickPoints.Count - 1;
            Transform startpoint = null;
            Transform endPoint = null;
            isFromSanp = false;
            
            if (!done && clickPoints.Count > 0)
            {
                lineRenderer.material = dottedLineMaterial;
                lineRenderer.textureMode = LineTextureMode.Tile;
                lineRenderer.positionCount = 2;
                startpoint = clickPoints[lastIndex];

                // check snap dot and close area
                if (0 != lastIndex)
                {
                    if (Vector3.Distance(clickPoints[0].position,point.transform.position) <= 0.05)
                    {
                        point.transform.position = clickPoints[0].position;
                        // IsDisCreteFromSnap = true;
                        // done = true;
                    }
                }


                
                lineRenderer.SetPosition(0, clickPoints[lastIndex].position);
                lineRenderer.SetPosition(1, point.transform.position);
                endPoint = point.transform;
            }

            if (done) // done == true when user click
            {
                if (pointStack.Count == 0)
                {
                    yBeginning = point.transform.position.y;
                }
                if (0 != lastIndex)
                {
                    if (clickPoints.Count >= 2)
                    {
                        if (Vector3.Distance(clickPoints[0].position,point.transform.position) <= 0.05)
                        {
                            point.transform.position = clickPoints[0].position;
                            IsDisCreteFromSnap = true;
                            // done = true;
                        }
                    }
                }
                var position = point.transform.position;
                var positionDraw2d = new Vector2(position.x, position.z);
                CreateLine2D(positionDraw2d,point.transform);

                // if (IsDisCreteFromSnap)
                // {
                //     point = Instantiate(dotPoint, point.transform.position, Quaternion.identity);
                // }
                //Debug.Log("Straight");
                clickPoints.Add(point.transform);
                var type = m_isDrawDoor ? pointType.VoidOut : pointType.Wall;
                pointStack.Push(new LineProp(point, textObj,point.transform.position,type));
                // point.transform.SetParent(exportObject.transform);
                point.transform.name = clickPoints.Count.ToString();
                lastIndex = clickPoints.Count - 1;
                if (lineRenderer != null)
                {
                    lineRenderer.material = m_isDrawDoor ? DoorMaterial : lineMaterial;
                }
                if (point.GetComponent<LineRenderer>() == null)
                {
                    lineRenderer = point.AddComponent<LineRenderer>();
                }

                lineRenderer.positionCount = 0;
                lineRenderer.material = m_isDrawDoor ? DoorMaterial : lineMaterial;
                // lineRenderer.material = m_isDrawDoor ? DoorMaterial : lineMaterial;
                lineRenderer.startWidth = lineRenderer.endWidth = lineWidth;
                if (lineRenderer.gameObject.GetComponent<BoxCollider>() == null)
                {
                    GameObject o;
                    (o = lineRenderer.gameObject).AddComponent<BoxCollider>().isTrigger = true;
                    lineRenderer.gameObject.AddComponent<Rigidbody>().useGravity = false;
                    o.tag = "line";
                }

                if (clickPoints.Count >= 2)
                {
                    startpoint = clickPoints[lastIndex];
                    endPoint = clickPoints[lastIndex - 1];
                    if (IsDisCrete)
                    {
                        ClearPoints();
                    }
                
                    if (IsDisCreteFromSnap)
                    {
                        IsDisCreteFromSnap = false;
                        ClearPoints();
                    }
                }                
            }
            if (startpoint != null && endPoint != null)
            {
                // Vector3 resultant = endPoint.position - startpoint.position;
                Vector3 resultant;
                if (IsGreaterOrEqual(startpoint.position,endPoint.position ))
                {
                    resultant = startpoint.position - endPoint.position;
                }
                else
                {
                    resultant = endPoint.position  - startpoint.position;
                }
                distance = Vector3.Magnitude(resultant);
                float angel = Mathf.Atan2(resultant.z, resultant.x) * Mathf.Rad2Deg;
                DrawLength(endPoint.position, startpoint.position, distance, angel, done);
            }

            return distance;
        }

        void DrawLength(Vector3 pointOne, Vector3 pointTwo, float distance, float yAngle, bool done)
        {
            string unit;
            string finalOutputText;
            
            Vector3 point = (pointOne + pointTwo) / 2;
            point.y += 0.02f;
            if (done)
            {
                //textObj.SetActive(false);
                textObj = Instantiate(distanceTextButton.gameObject, point, Quaternion.identity);
                textObj.name = clickPoints.Count + "th Label";

                if (pointStack.Count > 0)
                {               
                    LineProp l = pointStack.Pop();
                     l.distanceText = textObj;
                     pointStack.Push(l);
                }

            }      
            else if (!done)
            {
                if (distance * 100 > 5f && pointStack.Count>0)
                {
                    textObj.SetActive(true);
                }
                else
                {
                    textObj.SetActive(false);
                }
                textObj.transform.position = point;
            }

            finalOutputText = ProcessDistance(distance, out unit);
            
            textObj.GetComponentInChildren<TextMeshProUGUI>().text = finalOutputText + " <size=2>" + unit;
           Vector3 prevAngle = textObj.transform.eulerAngles;
            Debug.Log("yAngle " + yAngle);
            if (Mathf.Abs(yAngle) >= 150)
            {
                yAngle = 0;
            }
            else if (Mathf.Abs(yAngle) >= 60)
            {
                yAngle = 90;
            }
            else if (Mathf.Abs(yAngle) <= 40)
            {
                yAngle = 0;
            }
            prevAngle.y = -yAngle;
            prevAngle.x = 90;
            
            textObj.transform.eulerAngles = prevAngle;
        }

        private void ClearPoints()
        {
            clickPoints.Clear();
            click2DPoints.Clear();
            pointPosition.Clear();
        }

        public void Undo()
        {
            if (pointStack.Count > 0)
            {
                pointStack.Pop().SetEnable(false);
                textObj.SetActive(false);
            }

            if (point2DStack.Count > 0)
            {
                point2DStack.Pop().SetEnable(false);
            }
            ClearPoints();
        }

        private string ProcessDistance(float distance, out string unit)
        {
            // float d = distance * 100;
            // unit = "cm";
            // if (d > 100)
            // {
                float d = distance;
                unit = "m";
            // }
            return d.ToString("F");
        }

        public void ShowDistanceBox(string output)
        {
            int leftarrowIndex = output.IndexOf('<');
            int rightarrawIndex = output.IndexOf('>');
            string ans = output.Substring(0,leftarrowIndex-1);
            string unit = output.Substring(rightarrawIndex+1, output.Length - rightarrawIndex-1);
            // distanceUIBox.output = ans+ " <size=80>" +unit;
            //
            // distanceUIBox.gameObject.SetActive(true);
        }
        public void OnClickExported()
        {
            Debug.Log("OnClickExported");
            Debug.Log("clickPoints.Count  " + clickPoints.Count);
            DontDestroyOnLoad(exportObject);
            exportObject.SetActive(true);
            SceneManager.LoadScene("LineRender");
            Debug.Log("OnClickExported cameraCapture");
        }

        public void HideDistanceBox()
        {
            // distanceUIBox.gameObject.SetActive(false);
        }

        private void CreateLine2D(Vector2 position,Transform pointVector3)
        {
            Transform startpoint = null;
            Transform endPoint = null;
            GameObject point = new GameObject();
            // point.transform.position = new Vector2(position.x,position.z);
            point.transform.position = position;
            point.transform.name = click2DPoints.Count.ToString();
            point.transform.SetParent(exportObject.transform);
            LineRenderer lineRenderer2D = new LineRenderer {};
            
            if (point.GetComponent<LineRenderer>() == null)
            {
                 lineRenderer2D = point.AddComponent<LineRenderer>();
                 point.SetActive(false);
                 lineRenderer2D.positionCount = 0;
            }

            lineRenderer2D.material = m_isDrawDoor ? DoorMaterial : lineMaterial;
            lineRenderer2D.textureMode = LineTextureMode.Tile;
            lineRenderer2D.startWidth = lineRenderer2D.endWidth = lineWidth;
            click2DPoints.Add(point.transform);
            pointPosition.Add(pointVector3);
            int lastIndex = click2DPoints.Count - 1;

            if (click2DPoints.Count >= 2)
            {
                click2DPoints[lastIndex - 1].gameObject.SetActive(true);
                point.SetActive(true);
                lineRenderer2D.positionCount = 2;
                lineRenderer2D.gameObject.tag = "Dot";
                lineRenderer2D.SetPosition(0, click2DPoints[lastIndex - 1].position);
                lineRenderer2D.SetPosition(1, point.transform.position);
                startpoint = pointVector3;
                endPoint = pointPosition[lastIndex - 1];
                if (startpoint != null && endPoint != null)
                {
                    var position1 = startpoint.position;
                    var position2 = endPoint.position;
                    Vector3 resultant;
                    if (IsGreaterOrEqual(position1,position2))
                    {
                         resultant = position1 - position2;
                    }
                    else
                    {
                         resultant = position2 - position1;
                    }
                    var distance = Vector3.Magnitude(resultant);
                    var resultant2d = point.transform.position - click2DPoints[lastIndex - 1].position;
                    float angel = Mathf.Atan2(resultant.z, resultant2d.x) * Mathf.Rad2Deg;
                    DrawLength2D(point.transform.position, click2DPoints[lastIndex - 1].position, distance,angel);
                }
            }

            // create point 2D 
            var type = m_isDrawDoor ? pointType.VoidOut : pointType.Wall;
            point2DStack.Push(new LineProp(point,textObj,pointVector3.position,type));
        }

        public void OnClickDrawDoor()
        {
            ClearPoints();
            m_isDrawDoor = true;
            //IsDisCrete = false;
        }

        public void OnClickDrawPlan()
        {
            ClearPoints();
            m_isDrawDoor = false;
            IsDisCrete = false;
        }
        
        void DrawLength2D(Vector2 pointOne, Vector2 pointTwo, float distance,float yAngle)
        {
            string unit;
            Vector2 point = (pointOne + pointTwo) / 2;
            Debug.Log("Point x "+point.x + " point y "  + point.y);
            point.y += 0.06f;
            point.x += 0.04f;
            var textObj2D = Instantiate(distanceText2DButton.gameObject, point, Quaternion.identity);
            textObj2D.transform.SetParent(exportObject.transform);
            if (m_isDrawDoor)
            {
                if (textObj2D.GetComponentInChildren<Button>() != null)
                {
                   var image = textObj2D.GetComponentInChildren<Button>().image;
                    var colorImg = image.color;
                    colorImg.a = 0.2f;
                    image.color = colorImg;
                    textObj2D.GetComponentInChildren<Button>().image = image;
                }
            }
            if (point2DStack.Count > 0)
            {
                LineProp l = point2DStack.Pop();
                l.distanceText = textObj2D;
                point2DStack.Push(l);
            }
            // textObj.transform.SetParent(lineRenderer.transform);
            var finalOutputText = ProcessDistance(distance, out unit);
            textObj2D.GetComponentInChildren<TextMeshProUGUI>().text = finalOutputText + " <size=2>" + unit;
            var color = m_isDrawDoor ? new Color(81, 81, 255, 255) : Color.white;
            textObj2D.GetComponentInChildren<TextMeshProUGUI>().color = color;
            Debug.Log("2D yAngle " + yAngle);
            Vector3 prevAngle = textObj2D.transform.eulerAngles;
            // if (Mathf.Abs(yAngle) >= 150)
            // {
            //     yAngle = 0;
            // }
            // else if (Mathf.Abs(yAngle) >= 60)
            // {
            //     yAngle = 90;
            // }
            // else if (Mathf.Abs(yAngle) <= 40)
            // {
            //     yAngle = 0;
            // }
            prevAngle.z = Mathf.Abs(yAngle);
            prevAngle.y = 0;
            prevAngle.x = 0;
            textObj2D.transform.eulerAngles = prevAngle;
        }


        
        private static bool IsGreaterOrEqual(Vector3 local, Vector3 other)
        {
            if(local.x >= other.x && local.y >= other.y && local.z >= other.z)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool IsLesserOrEqual(Vector3 local, Vector3 other)
        {
            if(local.x <= other.x && local.y <= other.y && local.z <= other.z)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}