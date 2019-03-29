using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Classes;



public class ProjectOnCamera2D : MonoBehaviour
{
    public Camera cam;
    public Material BoundingBoxMat;
    private Vector3[] pts = new Vector3[8];
    private double relativeX = -1;
    private double relativeY = -1;
    private double relativeXMax = -1;
    private double relativeYMax = -1;
    private Rect r;
    public bool DrawOnScreen { get; set; }
    public bool IsVisible { get; set; }

    float MapInterval(float val, float srcMin, float srcMax, float dstMin, float dstMax)
    {
        if (val >= srcMax) return dstMax;
        if (val <= srcMin) return dstMin;
        return dstMin + (val - srcMin) / (srcMax - srcMin) * (dstMax - dstMin);
    }

    private void Start()
    {
        DrawOnScreen = true;
        //CreateCube();
    }
    private void OnDrawGizmos()
    {
        Vector3[] Vertices = GetColliderVertexPositions();
        foreach(Vector3 vertex in Vertices)
        {
            //Gizmos.color = Color.yellow;
            //Gizmos.DrawWireCube(vertex, new Vector3(0.05f,0.05f,0.05f));
        }
    }

    Bounds test()
    {
        Bounds bounds = new Bounds(this.gameObject.transform.position, new Vector3(0.001f, 0.001f, 0.001f));
        Vector3[] vertices = this.gameObject.GetComponentInChildren<MeshFilter>().mesh.vertices;
        bounds = new Bounds(this.gameObject.transform.position, Vector3.zero);
        foreach (Vector3 vertex in vertices)
        {
            bounds.Encapsulate(this.transform.TransformPoint(vertex));
        }
        return bounds;
    }

    Vector3[] GetColliderVertexPositions()
    {
        Vector3[] vertices = new Vector3[8];
        Matrix4x4 thisMatrix = transform.localToWorldMatrix;
        Quaternion storedRotation = transform.rotation;
        transform.rotation = Quaternion.identity;

        Vector3 extents = GetComponent<BoxCollider>().bounds.extents * 0.098f;


        vertices[0] = thisMatrix.MultiplyPoint3x4(extents);
        vertices[1] = thisMatrix.MultiplyPoint3x4(new Vector3(-extents.x, extents.y, extents.z));
        vertices[2] = thisMatrix.MultiplyPoint3x4(new Vector3(extents.x, extents.y, -extents.z));
        vertices[3] = thisMatrix.MultiplyPoint3x4(new Vector3(-extents.x, extents.y, -extents.z));
        vertices[4] = thisMatrix.MultiplyPoint3x4(new Vector3(extents.x, -extents.y, extents.z));
        vertices[5] = thisMatrix.MultiplyPoint3x4(new Vector3(-extents.x, -extents.y, extents.z));
        vertices[6] = thisMatrix.MultiplyPoint3x4(new Vector3(extents.x, -extents.y, -extents.z));
        vertices[7] = thisMatrix.MultiplyPoint3x4(-extents);

        transform.rotation = storedRotation;
        return vertices;
    }

    void CreateCube()
    {
        // Die Eckpunkte des Collider holen
        pts = GetColliderVertexPositions();

        // cube in BoxCollider größe erzeugen mit dem Tag 'ObjectToHide'.
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.SetParent(this.transform);
        cube.transform.position = this.transform.position;
        cube.transform.rotation = this.transform.rotation;
        Vector3 cubeSize = this.gameObject.GetComponent<BoxCollider>().size;
        cube.transform.localScale = cubeSize;
        cube.tag = "ObjectToHide";

        // set transparent material
        cube.GetComponent<Renderer>().material = BoundingBoxMat;
    }
    // https://answers.unity.com/questions/8003/how-can-i-know-if-a-gameobject-is-seen-by-a-partic.html
    private bool IsInView(GameObject origin, GameObject toCheck)
    {
        Vector3 pointOnScreen = this.cam.WorldToScreenPoint(toCheck.GetComponentInChildren<Renderer>().bounds.center);

        //Is in front
        if (pointOnScreen.z < 0)
        {
            //Debug.Log("Behind: " + toCheck.name);
            return false;
        }

        //Is in FOV
        if ((pointOnScreen.x < 0) || (pointOnScreen.x > Screen.width) ||
                (pointOnScreen.y < 0) || (pointOnScreen.y > Screen.height))
        {
            //Debug.Log("OutOfBounds: " + toCheck.name);
            return false;
        }

        RaycastHit hit;
        Vector3 heading = toCheck.transform.position - origin.transform.position;
        Vector3 direction = heading.normalized;// / heading.magnitude;

        if (Physics.Linecast(cam.transform.position, toCheck.GetComponentInChildren<Renderer>().bounds.center, out hit))
        {
            if (hit.transform.name != toCheck.name)
            {
                /* -->
                Debug.DrawLine(cam.transform.position, toCheck.GetComponentInChildren<Renderer>().bounds.center, Color.red);
                Debug.LogError(toCheck.name + " occluded by " + hit.transform.name);
                */
                return false;
            }
        }
        return true;
    }


    void FixedUpdate()
    {
        Vector3 screenPos = cam.WorldToScreenPoint(this.gameObject.transform.position);
        Renderer rend = this.gameObject.transform.Find("CarBody").GetComponent<Renderer>();
        if (this.IsInView(this.cam.gameObject, this.gameObject) == false)
        {
            IsVisible = false;
            return;
        }

        pts = GetColliderVertexPositions();
        //Get them in GUI space
        for (int i = 0; i < pts.Length; i++)
        {
            pts[i] = cam.WorldToScreenPoint(pts[i]);
            pts[i].y = Screen.height - pts[i].y;
        };

        //Calculate the min and max positions
        Vector3 min = pts[0];
        Vector3 max = pts[0];
        for (int i = 1; i < pts.Length; i++)
        {
            min = Vector3.Min(min, pts[i]);
            max = Vector3.Max(max, pts[i]);
        }

        //Construct a rect of the min and max positions and apply some margin
        r = Rect.MinMaxRect(min.x, min.y, max.x, max.y);

        if (r.width < 10 || r.height < 10)
        {
            IsVisible = false;
            return;
        }
        else
        {
            IsVisible = true;
        }
        // get relative bounding box pixels
        int yHeight = Screen.height;
        int xWidth = Screen.width;

        /*
         *  (0,1)                   (1,1)
         * 
         * 
         * 
         * 
         *  (0,0)                   (1,0)    
         */
        relativeX = MapInterval(r.x, 0, xWidth, 0, 1);
        relativeY = MapInterval(r.y, 0, yHeight, 0, 1);
        relativeXMax = MapInterval(r.xMax, 0, xWidth, 0, 1);
        relativeYMax = MapInterval(r.yMax, 0, yHeight, 0, 1);
    }

    void OnGUI()
    {
        //Render the box
        if (DrawOnScreen == true && this.gameObject && IsVisible == true) { 
            GUI.Box(r, "");
        }
    }

    public List<Tuple<double, double>> getRelativeBoxCoords()
    {
        List<Tuple<double, double>> l = new List<Tuple<double, double>>();

        l.Add(new Tuple<double, double>(relativeX, relativeY));
        l.Add(new Tuple<double, double>(relativeX, relativeYMax));
        l.Add(new Tuple<double, double>(relativeXMax, relativeY));
        l.Add(new Tuple<double, double>(relativeXMax, relativeYMax));

        return l;
    }




}
