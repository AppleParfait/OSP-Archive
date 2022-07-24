using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.MLAgents;
using System.Text;

public class RDWSimulationManager : MonoBehaviour
{
    public static RDWSimulationManager instance = null;

    public SimulationSetting simulationSetting; // 시뮬레이션 환경 설정을 담은 변수
    private RedirectedUnit[] redirectedUnits; //  각 unit들을 통제하는 변수
    public RedirectedUnit[] GetRedirectedUnits { get { return redirectedUnits; } }
    private GameObject[] unitObjects;

    Space2D realSpace, virtualSpace; // 실제 공간과 가상 공간에 대한 정보를 담은 변수

    private bool initializedForObstaclePosition = false;
    private List<Vector2> initialObstaclePositions;

    //input start
    private bool bStart = false;
    public bool BStart { get { return bStart; } set { bStart = value; } }

    private Dictionary<int, List<Vector2>> dic_initObstacleInfo = new Dictionary<int, List<Vector2>>();

    public float userResetTotalCount = 0;

    public float simulspeed = 30.0f;

    public void GenerateUnitObjects()
    {
        if(unitObjects == null)
        {
            unitObjects = new GameObject[redirectedUnits.Length];
            for (int i=0; i< redirectedUnits.Length; i++)
            {

                unitObjects[i] = new GameObject();
                unitObjects[i].name = "Unit_" + i;

                unitObjects[i].AddComponent<RedirectedUnitObject>();
                unitObjects[i].transform.parent = this.transform;
            }
        }
    }

    public void AssignUnitObjects()
    {
        for (int i = 0; i < redirectedUnits.Length; i++)
        {
            unitObjects[i].GetComponent<RedirectedUnitObject>().unit = redirectedUnits[i];
        }
    }

    public void DestroySpace()
    {
        if (realSpace != null) realSpace.Destroy();
        if (virtualSpace != null) virtualSpace.Destroy();
    }

    public void DestroyUnits()
    {
        if (redirectedUnits != null)
        {
            for (int i = 0; i < simulationSetting.unitSettings.Length; i++)
                if (redirectedUnits[i] != null) redirectedUnits[i].Destroy();
        }
    }

    public void DestroyAll()
    {
        DestroySpace();
        DestroyUnits();
    }

    public void GenerateSpaces()
    {
        GenerateRealSpace();
        GenerateVirtualSpace();
    }

    public void GenerateRealSpace()
    {
        realSpace = simulationSetting.realSpaceSetting.GetSpace();
        realSpace.spaceObject.transform2D.parent = this.transform;

        //InitObstacleInfo();

        if (!simulationSetting.realSpaceSetting.usePredefinedSpace)
            realSpace.GenerateSpace(simulationSetting.prefabSetting.realMaterial, simulationSetting.prefabSetting.obstacleMaterial, 3, 2);
    }
    public void UpdateObstacleVertexInfo(ref List<Vector2> vertices, Transform parent, int index)
    {
        simulationSetting.realSpaceSetting.obstacleObjectSettings[index].vertices.Clear();

        foreach (var item in vertices)
        {
            simulationSetting.realSpaceSetting.obstacleObjectSettings[index].vertices.Add(item);
        }

        realSpace.GenerateObstacle(simulationSetting.prefabSetting.obstacleMaterial, parent, index);
    }

    public void UpdateObstacleVertexInfo(ref List<Vector2> vertices, Transform parent)
    {
        for (int i = 0; i < simulationSetting.realSpaceSetting.obstacleObjectSettings.Count; i++)
        {
            simulationSetting.realSpaceSetting.obstacleObjectSettings[i].vertices.Clear();

            foreach (var item in vertices)
            {
                simulationSetting.realSpaceSetting.obstacleObjectSettings[i].vertices.Add(item);
            }

        }
        realSpace.GenerateObstacle(simulationSetting.prefabSetting.obstacleMaterial, parent);

    }

    public void Store_InitObstacleInfo()
    {
        for (int i = 0; i < simulationSetting.realSpaceSetting.obstacleObjectSettings.Count; i++)
        {
            dic_initObstacleInfo.Add(i, new List<Vector2>(simulationSetting.realSpaceSetting.obstacleObjectSettings[i].vertices));
        }
    }

    public void InitObstacleInfo(Transform parent)
    {
        for (int i = 0; i < simulationSetting.realSpaceSetting.obstacleObjectSettings.Count; i++)
        {
            simulationSetting.realSpaceSetting.obstacleObjectSettings[i].vertices.Clear();
            simulationSetting.realSpaceSetting.obstacleObjectSettings[i].vertices = new List<Vector2>(dic_initObstacleInfo[i]);
        }
        realSpace.GenerateObstacle(simulationSetting.prefabSetting.obstacleMaterial, parent);
    }

    public void GenerateVirtualSpace()
    {
        Polygon2D polygonObject = (Polygon2D) realSpace.spaceObject;

        virtualSpace = simulationSetting.virtualSpaceSetting.GetSpace();
        virtualSpace.spaceObject.transform2D.parent = this.transform;

        if (!simulationSetting.virtualSpaceSetting.usePredefinedSpace)
        {
            virtualSpace.GenerateSpace(simulationSetting.prefabSetting.virtualMaterial, simulationSetting.prefabSetting.obstacleMaterial, 3, 2);
        }

        if (!initializedForObstaclePosition)
        {
            initializedForObstaclePosition = true;
            this.initialObstaclePositions = new List<Vector2>();
            for (int i = 0; i < virtualSpace.obstacles.Count; i++)
            {
                initialObstaclePositions.Add(virtualSpace.GetObstaclePositionByIndex(i));
            }

            virtualSpace.SetInitialObstaclePositions(initialObstaclePositions);
        }

    }

    public void GenerateUnits()
    {
        redirectedUnits = new RedirectedUnit[simulationSetting.unitSettings.Length];
        //Debug.LogError("simulationSetting.unitSettings.Length " + simulationSetting.unitSettings.Length);
        List<Vector2> usedValues = new List<Vector2>();

        for (int i = 0; i < simulationSetting.unitSettings.Length; i++)
        {
            if (!simulationSetting.unitSettings[i].useRandomStartReal)
                continue;

            bool bRepeat = true;
            while (bRepeat)
            {
                Vector2 pos = realSpace.GetRandomPoint(0.6f);

                if (usedValues.Count < 1)
                {
                    simulationSetting.unitSettings[i].realStartPosition = pos;
                    usedValues.Add(pos);
                    //Debug.Log(pos.ToString());
                    break;
                }

                bool isValid = true;

                for (int j = 0; j < usedValues.Count; j++)
                {
                    if (Vector2.Distance(usedValues[j], pos) < 1.6f)
                    {
                        //Debug.Log(pos.ToString() + " / " + Vector2.Distance(usedValues[j], pos));
                        isValid = false;
                    }
                }

                if(isValid)
                {
                    simulationSetting.unitSettings[i].realStartPosition = pos;
                    usedValues.Add(pos);
                    break;
                }
            }
        }

        for (int i = 0; i < simulationSetting.unitSettings.Length; i++)
        {
            redirectedUnits[i] = simulationSetting.unitSettings[i].GetUnit(realSpace, virtualSpace, i); // 실질적으로 가져옴.
            redirectedUnits[i].GetEpisode().targetPrefab = simulationSetting.prefabSetting.targetPrefab;
            redirectedUnits[i].GetEpisode().setShowTarget(simulationSetting.showTarget);
            redirectedUnits[i].GetEpisode().SetRealAgentInitialPosition(simulationSetting.unitSettings[i].realStartPosition);
            redirectedUnits[i].GetEpisode().SetVirtualAgentInitialPosition(simulationSetting.unitSettings[i].virtualStartPosition);
            redirectedUnits[i].SetShowResetLocator(simulationSetting.showResetLocator);
            redirectedUnits[i].SetShowRealWall(simulationSetting.showRealWall);
            redirectedUnits[i].SetResetLocPrefab(simulationSetting.prefabSetting.resetLocPrefab);
            redirectedUnits[i].SetRealWallPrefab(simulationSetting.prefabSetting.realWallPrefab);
        }
        GenerateUnitObjects();
        AssignUnitObjects();
    }

    public void ReassignUnits()
    {
        for (int i = 0; i < simulationSetting.unitSettings.Length; i++)
        {
            if (simulationSetting.unitSettings[i].useRandomStartReal)
            {
                simulationSetting.unitSettings[i].realStartPosition = realSpace.GetRandomPoint(0.2f);
                simulationSetting.unitSettings[i].realStartRotation = Utility.sampleUniform(0f, 360f);
            }
            redirectedUnits[i].realUser.transform2D.localPosition = simulationSetting.unitSettings[i].realStartPosition;
            redirectedUnits[i].realUser.transform2D.localRotation = simulationSetting.unitSettings[i].realStartRotation;

            if (simulationSetting.unitSettings[i].useRandomStartVirtual)
            {
                simulationSetting.unitSettings[i].virtualStartPosition = virtualSpace.GetRandomPoint(0.2f);
                simulationSetting.unitSettings[i].virtualStartRotation = Utility.sampleUniform(0f, 360f);
            }
            redirectedUnits[i].virtualUser.transform2D.localPosition = simulationSetting.unitSettings[i].virtualStartPosition;
            redirectedUnits[i].virtualUser.transform2D.localRotation = simulationSetting.unitSettings[i].virtualStartRotation;

            redirectedUnits[i].GetEpisode().SetRealAgentInitialPosition(simulationSetting.unitSettings[i].realStartPosition);
            redirectedUnits[i].GetEpisode().SetVirtualAgentInitialPosition(simulationSetting.unitSettings[i].virtualStartPosition);
        }
        AssignUnitObjects();
    }

    public void DeleteResetLocators()
    {
        for(int i = 0; i < redirectedUnits.Length; i++)
        {
            redirectedUnits[i].DeleteResetLocObjects();
        }
    }

    public void GenerateResetLocators()
    {
        for(int i = 0; i < redirectedUnits.Length; i++)
        {
            redirectedUnits[i].GenerateResetLocObjects();
        }
    }

    public void DeleteRealWalls()
    {
        for(int i = 0; i < redirectedUnits.Length; i++)
        {
            redirectedUnits[i].DeleteRealWallObjects();
        }
    }

    public void GenerateRealWalls()
    {
        for(int i = 0; i < redirectedUnits.Length; i++)
        {
            redirectedUnits[i].GenerateRealWallObjects();
        }
    }


    public bool IsAllEpisodeEnd()
    { 
        for (int i = 0; i < redirectedUnits.Length; i++)
        {
            if (redirectedUnits[i].GetEpisode().IsNotEnd())
                return false;
        }

        return true;
    }

    public void PrintResult()
    {
        for (int i = 0; i < redirectedUnits.Length; i++)
        {
            // Debug.Log("[Space]");
            // Debug.Log("RealSpace: " + redirectedUnits[i].GetRealSpace().spaceObject.transform2D);
            // Debug.Log("VirtualSpace: " + redirectedUnits[i].GetVirtualSpace().spaceObject.transform2D);
            // Debug.Log("[User]");
            // Debug.Log("RealUser: " + redirectedUnits[i].GetRealUser().transform2D);
            // Debug.Log("VirtualUser: " + redirectedUnits[i].GetVirtualUser().transform2D);
            // Debug.Log("[Current Target]");
            // Debug.Log(redirectedUnits[i].GetEpisode().GetCurrentEpisodeIndex());
            // Debug.Log("[Target Length]");
            // Debug.Log(redirectedUnits[i].GetEpisode().GetEpisodeLength());
            // Debug.Log("[Result Data]");
            // Debug.Log(redirectedUnits[i].resultData);
            Debug.Log("[Number of Resets]");
        }
    }

    //long overTime = 10 * 1000;
    //Stopwatch sw = new Stopwatch();
    //bool checkTime = true;
    //public static float remainTime = 0;
    //public static float limitTime = 30;

    public void FastSimulationRoutine()
    {
        //int j = 0;
        //do
        //{
        //    DestroyAll();
        //    GenerateSpaces();
        //    GenerateUnits();

        //    while (!IsAllEpisodeEnd())
        //    {
        //        for (int i = 0; i < redirectedUnits.Length; i++)
        //            redirectedUnits[i].Simulation(redirectedUnits);

        //        if (simulationSetting.useDebugMode) DebugDraws();
        //    }

        //    PrintResult();
        //    j++;

        //} while (j < 3);
        Time.timeScale = 20f;//4f;
        do
        {
            DestroyAll();
            GenerateSpaces();
            GenerateUnits();

            while (!IsAllEpisodeEnd())
            {
                for (int i = 0; i < redirectedUnits.Length; i++)
                {
                    redirectedUnits[i].Simulate(redirectedUnits);
                }

            }

            PrintResult();

        } while (simulationSetting.useContinousSimulation);
    }

    public IEnumerator SlowSimulationRoutine()
    {
        //Debug.Log("aaaaaaaaaaaaaaaaaa");

        Time.timeScale = simulspeed;//4f;
        do
        {
            DestroyAll();
            GenerateSpaces();
            GenerateUnits();

            bStart = true;

            //if(!initializeRLAgent)
            //{
            //    initializeRLAgent = true;
            //    yield return new WaitForFixedUpdate();
            //}

            while (!IsAllEpisodeEnd())
            {
                _OSP.OSP_Agent.instance.RequestDecision();

                //for (int i = 0; i < redirectedUnits.Length; i++)
                //{
                //    redirectedUnits[i].Simulation(redirectedUnits);
                //}

                if (simulationSetting.useDebugMode) DebugDraws();

                yield return new WaitForFixedUpdate();
            }

            PrintResult();
            //initializeRLAgent = false;

        } while (simulationSetting.useContinousSimulation);
    }

    public void SimulateRDW()
    {
        for (int i = 0; i < redirectedUnits.Length; i++)
        {
            redirectedUnits[i].Simulate(redirectedUnits);
        }
    }

    public void DebugDraws()
    {
        realSpace.DebugDraws(Color.red, Color.blue);
        virtualSpace.DebugDraws(Color.red, Color.blue);

        foreach (RedirectedUnit unit in redirectedUnits)
            unit.DebugDraws(Color.green);
    }

    public void Start()
    {
        //if (simulationSetting.useVisualization)
        //    StartCoroutine(SlowSimulationRoutine());
        //else
        //    FastSimulationRoutine();
    }

    private void Update()
    {

    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        Store_InitObstacleInfo();
    }

    public void StartSimulation()
    {
        if (!bStart)
        {
            StopAllCoroutines();

            if (simulationSetting.useVisualization)
                StartCoroutine(SlowSimulationRoutine());
            else
                FastSimulationRoutine();
        }
    }

    Queue<DateTime> queue_userresetinfo = new Queue<DateTime>();
    public void Enqueue_UserResetFilter(DateTime date)
    {
        queue_userresetinfo.Enqueue(date);
        //Debug.Log(date.Millisecond.ToString());
    }

    public int Calc_UserResetFilter()
    {
        if (queue_userresetinfo.Count == 0)
            return 0;

        int totalresetcount = 0;
        DateTime flagDateTime;

        flagDateTime = queue_userresetinfo.Dequeue();
        totalresetcount++;

        //StringBuilder sb = new StringBuilder(); 

        while(queue_userresetinfo.Count > 0)
        {
            DateTime targetdate = queue_userresetinfo.Dequeue();
            TimeSpan aa = targetdate - flagDateTime;
            
            //sb.AppendLine(aa.TotalMilliseconds.ToString());

            if (aa.TotalMilliseconds > 150)
            {
                flagDateTime = targetdate;
                totalresetcount++;
            }
        }

        //Debug.LogWarning(sb.ToString());

        return totalresetcount;
    }

}
