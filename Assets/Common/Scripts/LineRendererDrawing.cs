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
        public Vector2 position;
        public GameObject distanceText;
        public LineProp(GameObject point, GameObject distanceText, Vector2 position)
        {
            this.point = point;
            this.position = position;
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
        public UnitConverter distanceUIBox;
        public GameObject exportObject;
        private List<Transform> clickPoints;
        private List<Vector3> clickPositions;
        public float lineWidth;

        GameObject textObj = null;
        LineRenderer lineRenderer;

        public Stack<LineProp> pointStack;
        public Stack<float> lineDistance;

        private bool isDisCrete;
        
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
            lineRenderer = new LineRenderer();
            pointStack = new Stack<LineProp>();
            lineDistance = new Stack<float>();
            textObj = Instantiate(distanceTextButton.gameObject, Vector3.zero, Quaternion.identity);
            textObj.name = "DottedDistanceLabel";
            textObj.SetActive(false);
            DontDestroyOnLoad(this);
        }
        
        public float DrawLine(GameObject point, bool done)
        {
            float distance = 0;
            // if (clickPoints.Count == 0)
            // {
            //     return distance;
            // }
            int lastIndex = clickPoints.Count - 1;

            Transform startpoint = null;
            Transform endPoint = null;

            if (!done && clickPoints.Count > 0)
            {
                //Debug.Log("Dotted");
                lineRenderer.material = dottedLineMaterial;
                lineRenderer.textureMode = LineTextureMode.Tile;
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, clickPoints[lastIndex].position);
                lineRenderer.SetPosition(1, point.transform.position);
                startpoint = clickPoints[lastIndex];
                foreach (var otherPoint in clickPoints)
                {
                    Debug.Log("Distance " + Vector3.Distance(otherPoint.position, point.transform.position));
                    if (Vector3.Distance(otherPoint.position,point.transform.position) <= 2)
                    {
                        Debug.Log("Snap dot");
                        point.transform.position = otherPoint.position;
                        IsDisCrete = true;
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
                pointStack.Push(new LineProp(point, textObj, position));
                // point.transform.SetParent(exportObject.transform);
                point.transform.name = clickPoints.Count.ToString();
                lastIndex = clickPoints.Count - 1;
                if (lineRenderer != null)
                {
                    lineRenderer.material = lineMaterial;
                }
                lineRenderer = point.AddComponent<LineRenderer>();
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
                    lineDistance.Push(distance*100);
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

            finalOutputText = processDistance(distance, out unit);
            
            textObj.GetComponentInChildren<TextMeshProUGUI>().text = finalOutputText + " <size=2>" + unit;

            Vector3 prevAngle = textObj.transform.eulerAngles;
            prevAngle.y = -yAngle;
            prevAngle.x = 90;
            
            textObj.transform.eulerAngles = prevAngle;
        }

        public void ClearPoints()
        {
            clickPoints.Clear();
        }

        public void Undo()
        {
            if (pointStack.Count > 0)
            {
                pointStack.Pop().SetEnable(false);
                lineDistance.Pop();
                textObj.SetActive(false);
            }
            ClearPoints();
        }

        public string processDistance(float distance, out string unit)
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

       //  public void OnClickExport()
       //  {
       //    //  ExportGameObjects(new[] {(Object)exportObject});
       //   // ExportGameObjects(exportObject);
       // //   FbxExporter02.OnExport(new[] {(Object) exportObject});
       //  }
       private void CheckSnapDot()
       {
           
       }
        public void OnClickExported()
        {
            Debug.Log("OnClickExported");
            Debug.Log("clickPoints.Count  " + clickPoints.Count);
            SceneManager.LoadScene("LineRender");
            // if (clickPoints.Count < 2)
            // {
            //     return;
            // }
            //
            // for (var i =1; i < clickPoints.Count; i++)
            // {
            //     Debug.Log("clickPoint render line" );
            //     var dot1 = Instantiate(clickPoints[i - 1].gameObject, exportObject.transform);
            //     var lineRender = dot1.GetComponent<LineRenderer>();
            //     lineRender.transform.SetParent(exportObject.transform);
            //     lineRender.gameObject.layer = 8;
            //
            //     Material material;
            //         lineRender.startWidth = lineRender.endWidth = lineWidth;
            //     material = dottedLineMaterial;
            //     lineRender.material = material;
            //     lineRender.textureMode = LineTextureMode.Tile;
            //     lineRender.positionCount = 2;
            //     var pointOnePosition = new Vector2(clickPoints[i-1].position.x, clickPoints[i-1].position.z/1.5f);
            //     var pointTwoPosition = new Vector2(clickPoints[i].position.x, clickPoints[i].position.z/1.5f);
            //     lineRender.SetPosition(0, pointOnePosition);
            //     lineRender.SetPosition(1, pointTwoPosition);
            // }
            //
            // cameraCapture.SetActive(true);
            
            
            Debug.Log("OnClickExported cameraCapture");
        }
        
        // private static void ExportGameObjects(GameObject objects)
        // {
        //     // string filePath = Path.Combine(Application.dataPath, "MyGame.fbx");
        //     // Debug.Log("filePath " + filePath);
        //     // ModelExporter.ExportObjects(filePath, objects);
        //
        //     Debug.Log("Finish ");
        //
        //     // ModelExporter.ExportObject can be used instead of 
        //     // ModelExporter.ExportObjects to export a single game object
        //     //
        //     using(FbxManager fbxManager = FbxManager.Create ()){
        //         // configure IO settings.
        //         fbxManager.SetIOSettings (FbxIOSettings.Create (fbxManager, Globals.IOSROOT));
        //         // Export the scene
        //         using (FbxExporter exporter = FbxExporter.Create (fbxManager, objects.name)) {
        //             // Initialize the exporter.
        //             bool status = exporter.Initialize ("game.fbx", -1, fbxManager.GetIOSettings());
        //             // exporter.Export();
        //             // Create a new scene to export
        //             FbxObject scene = FbxObject.Create(fbxManager, objects.name);
        //             // Export the scene to the file.
        //             exporter.Export (scene.GetScene());
        //             
        //             
        //         }
        //     }
        // }
        //
        

        public void HideDistanceBox()
        {
            // distanceUIBox.gameObject.SetActive(false);
        }
    }
}