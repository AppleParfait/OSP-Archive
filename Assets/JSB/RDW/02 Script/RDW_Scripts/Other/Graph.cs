using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph
{
    Dictionary<Vector2, List<Vector2>> adjList;
    
    public Graph()
    {
        adjList = new Dictionary<Vector2, List<Vector2>>();
        
    }

    public Graph(Graph otherGraph)
    {
        adjList = new Dictionary<Vector2, List<Vector2>>(otherGraph.adjList);
    }

    public void AddVertex(Vector2 vertex)
    {
        if(!HasVertex(vertex))
            adjList.Add(vertex, new List<Vector2>());
    }


    public void AddEdge(Vector2 sourceVertex, Vector2 destinationVertex, bool isBidirection)
    {
        if (!adjList.ContainsKey(sourceVertex))
            AddVertex(sourceVertex);
        if (!adjList.ContainsKey(destinationVertex))
            AddVertex(destinationVertex);

        if(!HasEdge(sourceVertex, destinationVertex))
        {
            adjList[sourceVertex].Add(destinationVertex);

            if (isBidirection)
                adjList[destinationVertex].Add(sourceVertex);
        }
    }

    public void RemoveVertex(Vector2 vertex, bool isBidirection)
    {
        if (HasVertex(vertex))
        {
            if(isBidirection)
            {
                foreach (var v in adjList[vertex])
                {
                    adjList[v].Remove(vertex);
                }
            }
            adjList[vertex].Clear();

            adjList.Remove(vertex);
        }
    }

    public void RemoveEdge(Vector2 sourceVertex, Vector2 destinationVertex, bool isBidirection)
    {
        if(HasEdge(sourceVertex, destinationVertex))
        {
            adjList[sourceVertex].Remove(destinationVertex);

            if (isBidirection)
                adjList[destinationVertex].Remove(sourceVertex);
        }
    }

    public void RemoveAllEdgeInVertex(Vector2 vertex, bool isBidirection)
    {
        if (HasVertex(vertex))
        {
            if (isBidirection)
            {
                foreach (var v in adjList[vertex])
                {
                    adjList[v].Remove(vertex);
                }
            }
            adjList[vertex].Clear();
        }
    }

    public int GetVertexCount()
    {
        return adjList.Count;
    }

    public int GetEdgesCount(bool isBidirection)
    {
        int count = 0;
        foreach(Vector2 vertex in adjList.Keys)
        {
            count += adjList[vertex].Count;
        }

        if (isBidirection)
        {
            count = count / 2;
        }

        return count;
    }

    public int GetDegree(Vector2 vertex, bool isBidirection)
    {
        if (HasVertex(vertex))
        {
            int result = adjList[vertex].Count;

            if (isBidirection)
            {
                foreach (Vector2 otherVertex in adjList.Keys)
                {
                    if (HasEdge(otherVertex, vertex))
                        result += 1;
                }
            }

            return result;
        }
        else
            return -1;
    }

    public bool HasVertex(Vector2 vertex)
    {
        return adjList.ContainsKey(vertex);
    }

    public bool HasEdge(Vector2 sourceVertex, Vector2 destinationVertex)
    {
        return adjList[sourceVertex].Contains(destinationVertex);
    }

    public override string ToString()
    {
        string result = null;

        foreach(Vector2 vertex in adjList.Keys)
        {
            result += string.Format("{0}-> ", vertex);

            foreach(Vector2 connectedVertex in adjList[vertex])
            {
                result += string.Format("{0}, ", connectedVertex);
            }

            result += '\n';
        }
        return result;
    }

    public List<Vector2> FindOutline(bool isBidirection)
    {
        List<Vector2> answer = new List<Vector2>();
        Stack<Vector2> stack = new Stack<Vector2>();
        Graph copiedGraph = new Graph(this);

        Vector2 startVertex = Vector2.negativeInfinity;
        foreach(var v in adjList.Keys)
        {
            if (v.x > startVertex.x)
                startVertex = v;
        }

        //Debug.Log(startVertex);

        stack.Push(startVertex);
        Vector2 currentDirection = Vector2.right;

        while (stack.Count != 0)
        {
            Vector2 currentVetex = stack.Peek();

            if (GetDegree(currentVetex, isBidirection) == 0)
            {
                answer.Add(currentVetex);
                stack.Pop();
            }
            else
            {
                float minAngle = 361;
                Vector2 nextVertex = Vector2.zero;

                foreach (var neighborVertex in adjList[currentVetex])
                {
                    Vector2 selectedEdge = neighborVertex - currentVetex;
                    float ccwAngle = Utility.GetCCWAngle(currentDirection, selectedEdge);

                    if (ccwAngle < minAngle)
                    {
                        minAngle = ccwAngle;
                        nextVertex = neighborVertex;
                    }
                }

                currentDirection = (currentVetex - nextVertex).normalized;

                RemoveAllEdgeInVertex(currentVetex, isBidirection);
                stack.Push(nextVertex);
            }
        }

        return answer;
    }

    //public List<Vector2> FindEulerPath(Vector2 startVertex, bool isBidirection) // Depreciated
    //{
    //    List<Vector2> answer = new List<Vector2>();
    //    Stack<Vector2> stack = new Stack<Vector2>();
    //    Graph copiedGraph = new Graph(this);
    //    stack.Push(startVertex);

    //    while(stack.Count != 0)
    //    {
    //        Vector2 currentVetex = stack.Peek();

    //        if(GetDegree(currentVetex, isBidirection) == 0)
    //        {
    //            answer.Add(currentVetex);
    //            stack.Pop();
    //        }
    //        else
    //        {
    //            Vector2 otherVertex = adjList[currentVetex][0];
    //            RemoveEdge(currentVetex, otherVertex, isBidirection);
    //            stack.Push(otherVertex);
    //        }
    //    }

    //    answer.RemoveAt(answer.Count - 1);
    //    return answer;
    //}
}
