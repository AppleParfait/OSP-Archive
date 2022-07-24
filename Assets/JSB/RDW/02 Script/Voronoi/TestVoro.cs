using UnityEngine;
using System.Collections.Generic;
using csDelaunay;

public class TestVoro : MonoBehaviour
{

    public static TestVoro instance = null;

    // The number of polygons/sites we want
    public int polygonNumber = 200;

    // This is where we will store the resulting data
    private Dictionary<Vector2f, Site> sites;
    private List<Edge> edges;
    List<Vector2f> points;
    List<Vector2f> Defaultpoints;

    public int width = 512;
    public int height = 512;

    public GameObject prefab01;
    public GameObject prefab02;
    public GameObject prefab03;

    List<GameObject> list_UserMarker = new List<GameObject>();
    List<GameObject> list_VoronoiVertexMarker = new List<GameObject>();
    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // Create your sites (lets call that the center of your polygons)
        points = CreateRandomPoint();

        //Defaultpoints = new List<Vector2f>(points);

        //// Create the bounds of the voronoi diagram
        //// Use Rectf instead of Rect; it's a struct just like Rect and does pretty much the same,
        //// but like that it allows you to run the delaunay library outside of unity (which mean also in another tread)
        //Rectf bounds = new Rectf(0, 0, width, height);

        //// There is a two ways you can create the voronoi diagram: with or without the lloyd relaxation
        //// Here I used it with 2 iterations of the lloyd relaxation
        ////Voronoi voronoi = new Voronoi(points, bounds, 5);

        //// But you could also create it without lloyd relaxtion and call that function later if you want
        //Voronoi voronoi = new Voronoi(points,bounds);
        ////voronoi.LloydRelaxation(5);

        //// Now retreive the edges from it, and the new sites position if you used lloyd relaxtion
        //sites = voronoi.SitesIndexedByLocation;
        //edges = voronoi.Edges;

        ////DisplayVoronoiDiagram();
    }

    private List<Vector2f> CreateRandomPoint()
    {
        // Use Vector2f, instead of Vector2
        // Vector2f is pretty much the same than Vector2, but like you could run Voronoi in another thread
        List<Vector2f> points = new List<Vector2f>();
        for (int i = 0; i < polygonNumber; i++)
        {
            //points.Add(new Vector2f(Random.Range(0, width), Random.Range(0, height)));
            points.Add(new Vector2f(Random.Range(-3.0f, 3.0f), Random.Range(-3.0f, 3.0f)));
            list_UserMarker.Add(Instantiate(prefab01, new Vector3(points[i].x, points[i].y, 0.0f), Quaternion.identity));

            list_VoronoiVertexMarker.Add(Instantiate(prefab03, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity));
            list_VoronoiVertexMarker.Add(Instantiate(prefab03, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity));
            list_VoronoiVertexMarker.Add(Instantiate(prefab03, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity));
            list_VoronoiVertexMarker.Add(Instantiate(prefab03, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity));
        }



        return points;
    }

    private bool flag1 = true;

    private void Update()
    {
        for (int i = 0; i < polygonNumber; i++)
        {
            //points[i] = new Vector2f(Defaultpoints[i].x + Random.Range(-10, 10), Defaultpoints[i].y + Random.Range(-10, 10));
        }

        //Rectf bounds = new Rectf(0, 0, width, height);
        Rectf bounds = new Rectf(-3, -3, 6, 6);

        Voronoi voronoi;
        if (flag1)
        {
            voronoi = new Voronoi(points, bounds);
        }
        else
        {
            voronoi = new Voronoi(points, bounds, 50);
        }

        sites = voronoi.SitesIndexedByLocation;
        edges = voronoi.Edges;

        DisplayVoronoiDiagram();

        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (flag1)
            {
                flag1 = false;
            }
            else
            {
                flag1 = true;
            }
        }

    }

    // Here is a very simple way to display the result using a simple bresenham line algorithm
    // Just attach this script to a quad
    public void DisplayVoronoiDiagram()
    {

        //foreach (GameObject item in list_UserMarker)
        //{
        //    DestroyImmediate(item);
        //}
        //list_UserMarker.Clear();

        //Texture2D tx = new Texture2D(width, height);
        //tx.SetPixel(0, 0, Color.green);
        //tx.SetPixel(100, 100, Color.green);
        int count1 = 0;
        foreach (KeyValuePair<Vector2f, Site> kv in sites)
        {
            //tx.SetPixel((int)kv.Key.x, (int)kv.Key.y, Color.red);
            Debug.Log(kv.Key.x + " " + kv.Key.y);
            //list_UserMarker.Add(Instantiate(prefab01, new Vector3(kv.Key.x, kv.Key.y, 0.0f), Quaternion.identity));
            list_UserMarker[count1++].transform.position = new Vector3(kv.Key.x, kv.Key.y, 0.0f);
        }

        int count = 0;
        foreach (Edge edge in edges)
        {
            // if the edge doesn't have clippedEnds, if was not within the bounds, dont draw it
            if (edge.ClippedEnds == null) continue;

            DrawLine(edge.ClippedEnds[LR.LEFT], edge.ClippedEnds[LR.RIGHT]);
            list_VoronoiVertexMarker[count++].transform.position = new Vector3(edge.ClippedEnds[LR.LEFT].x, edge.ClippedEnds[LR.LEFT].y, 0.0f);
            list_VoronoiVertexMarker[count++].transform.position = new Vector3(edge.ClippedEnds[LR.RIGHT].x, edge.ClippedEnds[LR.RIGHT].y, 0.0f);
        }
        //tx.Apply();

        //GetComponent<Renderer>().material.mainTexture = tx;
        //GetComponent<SpriteRenderer>().sprite = Sprite.Create(tx, new Rect(0, 0, width, height), Vector2.one * 0.5f);
    }

    // Bresenham line algorithm
    private void DrawLine(Vector2f p0, Vector2f p1)
    {
        //int x0 = (int)p0.x;
        //int y0 = (int)p0.y;
        //int x1 = (int)p1.x;
        //int y1 = (int)p1.y;

        float x0 = p0.x;
        float y0 = p0.y;
        float x1 = p1.x;
        float y1 = p1.y;



        //int dx = Mathf.Abs(x1 - x0);
        //int dy = Mathf.Abs(y1 - y0);
        //int sx = x0 < x1 ? 1 : -1;
        //int sy = y0 < y1 ? 1 : -1;
        //int err = dx - dy;

        float dx = Mathf.Abs(x1 - x0);
        float dy = Mathf.Abs(y1 - y0);
        float sx = x0 < x1 ? 1 : -1;
        float sy = y0 < y1 ? 1 : -1;
        float err = dx - dy;

        //list_UserMarker.Add(Instantiate(prefab02, new Vector3((x0 + offset), (y0 + offset), 0.0f), Quaternion.identity));
        //while (true)
        //{
        //    //tx.SetPixel(x0 + offset, y0 + offset, c);


        //    if (x0 == x1 && y0 == y1) break;
        //    //int e2 = 2 * err;
        //    float e2 = 2 * err;
        //    if (e2 > -dy)
        //    {
        //        err -= dy;
        //        x0 += sx;
        //    }
        //    if (e2 < dx)
        //    {
        //        err += dx;
        //        y0 += sy;
        //    }
        //}

        //»öÄ¥¾²
        //tx.SetPixel(x0, y0, Color.red);
        //tx.SetPixel(x1, y1, Color.red);
        //list_UserMarker.Add(Instantiate(prefab03, new Vector3(x0, y0, 0.0f), Quaternion.identity));
        //list_UserMarker.Add(Instantiate(prefab03, new Vector3(x1, y1, 0.0f), Quaternion.identity));

        Debug.DrawLine(new Vector3(x0, y0, 0.0f), new Vector3(x1, y1, 0.0f), Color.green, 200, false);
    }
}
