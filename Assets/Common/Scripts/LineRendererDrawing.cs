using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

namespace JoystickLab
{
    public struct LineProp
    {
        public GameObject point;
        public GameObject distanceText;
        public LineProp(GameObject point, GameObject distanceText)
        {
            this.point = point;
            this.distanceText = distanceText;
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
        public Material dottedLineMaterial;
        public GameObject cameraCapture;
        //public TextMesh distanceText;
        public GameObject distanceTextButton;
        public GameObject distanceText2DButton;

        public UnitConverter distanceUIBox;
        public GameObject exportObject;
        private List<Transform> clickPoints;
        private List<Transform> click2DPoints;
        private List<Vector3> clickPositions;
        public float lineWidth;

        GameObject textObj = null;
        LineRenderer lineRenderer;

        public Stack<LineProp> pointStack;
        public Stack<LineProp> point2DStack;
        public Stack<float> lineDistance;

        private bool isDisCrete;

        private bool IsDisCreteFromSnap;
        
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
            textObj = Instantiate(distanceTextButton.gameObject, Vector3.zero, Quaternion.identity);
            textObj.name = "DottedDistanceLabel";
            textObj.SetActive(false);
        }
        
        public float DrawLine(GameObject point, bool done,GameObject dotPoint)
        {
            float distance = 0;
            int lastIndex = clickPoints.Count - 1;
            Transform startpoint = null;
            Transform endPoint = null;

            if (!done && clickPoints.Count > 0)
            {
                lineRenderer.material = dottedLineMaterial;
                lineRenderer.textureMode = LineTextureMode.Tile;
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, clickPoints[lastIndex].position);
                lineRenderer.SetPosition(1, point.transform.position);
                startpoint = clickPoints[lastIndex];
                
                // check snap dot and close area
                if (0 != lastIndex)
                {
                    Debug.Log("Distance " + Vector3.Distance(clickPoints[0].position, point.transform.position));
                    if (Vector3.Distance(clickPoints[0].position,point.transform.position) <= 0.03)
                    {
                        Debug.Log("Snap dot");
                        // var copyPosition = clickPoints[0].position;
                        // point.transform.position = clickPoints[0].position;
                        point = Instantiate(dotPoint, clickPoints[0].position, Quaternion.identity);
                        IsDisCreteFromSnap = true;
                        done = true;
                    }
                }
                endPoint = point.transform;
            }

            if (done) // done == true when user click
            {
                //Debug.Log("Straight");
                clickPoints.Add(point.transform);
                var position1 = point.transform.position;
                var position = new Vector2(position1.x, position1.z);
                CreateLine2D(position);
                pointStack.Push(new LineProp(point, textObj));
                // point.transform.SetParent(exportObject.transform);
                point.transform.name = clickPoints.Count.ToString();
                lastIndex = clickPoints.Count - 1;
                // if (lineRenderer != null)
                // {
                //     lineRenderer.material = lineMaterial;
                // }
                if (point.GetComponent<LineRenderer>() == null)
                {
                    lineRenderer = point.AddComponent<LineRenderer>();
                }
                lineRenderer.material = lineMaterial;
                lineRenderer.startWidth = lineRenderer.endWidth = lineWidth;
                
                
                
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
                Vector3 resultant = endPoint.position - startpoint.position;
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
                textObj = Instantiate(distanceTextButton.gameObject, point, Quaternion.identity);
                textObj.name = clickPoints.Count + "th Label";
                if (pointStack.Count > 0)
                {
                    LineProp l = pointStack.Pop();
                    l.distanceText = textObj;
                    pointStack.Push(l);
                    lineDistance.Push(distance);
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
            prevAngle.y = -yAngle;
            prevAngle.x = 90;
            
            textObj.transform.eulerAngles = prevAngle;
        }

        private void ClearPoints()
        {
            clickPoints.Clear();
            click2DPoints.Clear();
        }

        public void Undo()
        {
            if (pointStack.Count > 0)
            {
                pointStack.Pop().SetEnable(false);
                lineDistance.Pop();
                textObj.SetActive(false);
            }

            if (point2DStack.Count > 0)
            {
                point2DStack.Pop().SetEnable(false);
            }
            ClearPoints();
        }

        public string ProcessDistance(float distance, out string unit)
        {
            float d = distance * 100;
            unit = "cm";
            if (d > 100)
            {
                d = distance;
                unit = "m";
            }
            return d.ToString("F1");
        }

        public void ShowDistanceBox(string output)
        {
            int leftarrowIndex = output.IndexOf('<');
            int rightarrawIndex = output.IndexOf('>');
            string ans = output.Substring(0,leftarrowIndex-1);
            print("leng"+output.Length);
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

        private void CreateLine2D(Vector2 position)
        {
            Transform startpoint = null;
            Transform endPoint = null;
            GameObject point = new GameObject();
            point.transform.position = position;
            point.transform.name = click2DPoints.Count.ToString();
            point.transform.SetParent(exportObject.transform);
            LineRenderer lineRenderer2D = new LineRenderer {};
            if (point.GetComponent<LineRenderer>() == null)
            {
                lineRenderer2D = point.AddComponent<LineRenderer>();
            }
            lineRenderer2D.material = lineMaterial;
            lineRenderer2D.textureMode = LineTextureMode.Tile;
            lineRenderer2D.startWidth = lineRenderer2D.endWidth = lineWidth;
            click2DPoints.Add(point.transform);
            int lastIndex = click2DPoints.Count - 1;

            if (click2DPoints.Count >= 2)
            {
                lineRenderer2D.positionCount = 2;
                lineRenderer2D.SetPosition(0, click2DPoints[lastIndex - 1].position);
                lineRenderer2D.SetPosition(1, point.transform.position);
                startpoint = click2DPoints[lastIndex - 1];
                endPoint = point.transform;
                if (lineDistance.Count > 0)
                {
                    DrawLength2D(startpoint.position, endPoint.position, lineDistance.Pop());
                }
            }

            // create point 2D 
            point2DStack.Push(new LineProp(point,textObj));
        }

        public void OnClearAllData()
        {
            point2DStack.Clear();
            click2DPoints.Clear();
            pointStack.Clear();
            clickPoints.Clear();

            textObj = Instantiate(distanceTextButton.gameObject, Vector3.zero, Quaternion.identity);
            textObj.name = "DottedDistanceLabel";
            textObj.SetActive(false);
            SceneManager.LoadScene("UXManagerScene");
        }
        
        void DrawLength2D(Vector2 pointOne, Vector2 pointTwo, float distance)
        {
            string unit;
            Vector2 point = (pointOne + pointTwo) / 2;
            Debug.Log("Point x "+point.x + " point y "  + point.y);
            // point.y += 0.02f;
            var textObj2D = Instantiate(distanceText2DButton.gameObject, point, Quaternion.identity);
            textObj2D.transform.SetParent(exportObject.transform);
            // textObj.transform.SetParent(lineRenderer.transform);
            var finalOutputText = ProcessDistance(distance, out unit);
            textObj2D.GetComponentInChildren<TextMeshProUGUI>().text = finalOutputText + " <size=2>" + unit;
        }
    }
}