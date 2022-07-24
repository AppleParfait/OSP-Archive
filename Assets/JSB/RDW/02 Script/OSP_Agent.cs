using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using System.Text;
using System.Linq;
using csDelaunay;

namespace _OSP
{
    public enum Enum_CurriculumState
    {
        _1stQuater, _2ndQuater, _3rdQuater, _4thQuater
    };

    public class OSP_Agent : Agent
    {
        /// <summary>
        /// for singleton pattern
        /// </summary>
        public static OSP_Agent instance = null;

        /// <summary>
        /// enable to input state information
        /// </summary>
        public bool bUseVecOberv;

        /// <summary>
        /// mlagent Academic envParams
        /// </summary>
        EnvironmentParameters m_ResetParams;

        /// <summary>
        /// List for pointing each actual user object
        /// </summary>        
        private List<GameObject> list_physical_simulatedUsers = new List<GameObject>();

        /// <summary>
        /// List for pointing each virtual user object
        /// </summary>        
        private List<GameObject> list_virtual_simulatedUsers = new List<GameObject>();
 
        private float prev_wallReset_mean = 0;
        private float prev_userReset_mean = 0;
        private float prev_shutterReset_mean = 0;

        /// <summary>
        /// Text for checking the current episode count
        /// </summary>        
        [SerializeField]
        private Text text_currentEpi;
        /// <summary>
        /// Text for checking step count of current episode 
        /// </summary>    
        [SerializeField]
        private Text text_currentStep;
        /// <summary>
        /// Text for checking cumulative reward of current episode 
        /// </summary>    
        [SerializeField]
        private Text text_currentReward;

        /// <summary>
        /// Dictionary for pointing a queue of the position X of each physical user
        /// (Divided 2D vector separately to use X values as a window)
        /// </summary>
        Dictionary<int, Queue<float>> dic_physicalUsers_pos_X = new Dictionary<int, Queue<float>>();
        /// <summary>
        /// Dictionary for pointing a queue of the position Z of each physical user
        /// (Divided 2D vector separately to use Z values as a window)
        /// </summary>
        Dictionary<int, Queue<float>> dic_physicalUsers_pos_Z = new Dictionary<int, Queue<float>>();
        /// <summary>
        /// Dictionary for pointing a queue of the 1D orientation of each physical user
        /// </summary>
        Dictionary<int, Queue<float>> dic_physicalUsers_orient_Y = new Dictionary<int, Queue<float>>();
        /// <summary>
        /// Dictionary for pointing a queue of the position X of each virtual user
        /// (Divided 2D vector separately to use X values as a window)
        /// </summary>
        Dictionary<int, Queue<float>> dic_virtualUsers_pos_X = new Dictionary<int, Queue<float>>();
        /// <summary>
        /// Dictionary for pointing a queue of the position Z of each virtual user
        /// (Divided 2D vector separately to use Z values as a window)
        /// </summary>
        Dictionary<int, Queue<float>> dic_VirtualUsers_Position_Z = new Dictionary<int, Queue<float>>();
        /// <summary>
        /// Dictionary for pointing a queue of the 1D orientation of each virtual user
        /// </summary>
        Dictionary<int, Queue<float>> dic_VirtualUsers_Orient_Y = new Dictionary<int, Queue<float>>();

        /// <summary>
        /// Dictionary for pointing a queue of the 1D orientation of each physical user
        /// (Divided 8D vector separately to use values of each direction as a window)
        /// </summary>
        private Dictionary<int, Dictionary<int, Queue<float>>> doubleDic_physicalUsers_8wayWallDist = new Dictionary<int, Dictionary<int, Queue<float>>>();
        /// <summary>
        /// Dictionary for pointing a queue of the 1D orientation of each virtual user
        /// (Divided 8D vector separately to use values of each direction as a window)
        /// </summary>
        private Dictionary<int, Dictionary<int, Queue<float>>> doubleDic_virtualUsers_8wayWallDist = new Dictionary<int, Dictionary<int, Queue<float>>>();

        /// <summary>
        /// Dictionary for pointing a queue of area value of sub-space for each physical user
        /// </summary>
        Dictionary<int, Queue<float>> dic_physicalRoomSize_users = new Dictionary<int, Queue<float>>();


        /// <summary>
        /// 
        /// </summary>
        private float physicalRoom_width_half = 0;
        /// <summary>
        /// 
        /// </summary>
        private float physicalRoom_height_half = 0;
        /// <summary>
        /// 
        /// </summary>
        private float virtualRoom_width_half = 0;
        /// <summary>
        /// 
        /// </summary>
        private float virtualRoom_height_Half = 0;
        /// <summary>
        /// 
        /// </summary>
        private float actual_halfRoomsize_default = 36;
        /// <summary>
        /// 
        /// </summary>
        private float actual_roomhypotenuse = 0.0f;

        /// <summary>
        /// 
        /// </summary>
        private int currentSimulationCount = 0;
        /// <summary>
        /// 
        /// </summary>
        public int SimulationCount_max = 100;

        /// <summary>
        /// 
        /// </summary>
        List<int> list_usersTotalReset_perEpisode = new List<int>();
        /// <summary>
        /// 
        /// </summary>
        List<int> list_userbetReset_perEpisode = new List<int>();
        /// <summary>
        /// 
        /// </summary>
        List<int> list_usersShutterReset_perEpisode = new List<int>();

        /// <summary>
        /// 
        /// </summary>
        private RaycastHit rayCastHit;

        /// <summary>
        /// 
        /// </summary>
        private Dictionary<Vector2f, Site> sites;
        /// <summary>
        /// 
        /// </summary>
        private List<Edge> edges;
        /// <summary>
        /// 
        /// </summary>
        List<Vector2f> list_VoronoiSeedPoint = new List<Vector2f>();
        /// <summary>
        /// 
        /// </summary>
        List<Vector2f> list_VoronoiSeedPoint_Fixed = new List<Vector2f>();
        /// <summary>
        /// 
        /// </summary>
        List<GameObject> list_VoronoiVertexMarker = new List<GameObject>();
        /// <summary>
        /// 
        /// </summary>
        List<Vector2> list_WayPoint_vertices = new List<Vector2>();
        /// <summary>
        /// 
        /// </summary>
        List<Vector2> list_voronoiCentroid = new List<Vector2>();
        /// <summary>
        /// 
        /// </summary>
        List<float> list_voronoiArea = new List<float>();

        /// <summary>
        /// for Uniform Partitioning Simulation (with Lloyd Relaxation)
        /// </summary>
        public bool bEnable_InitPhyUserPosUni = false;

        /// <summary>
        /// Preserve initial Voronoi Diagram 
        /// </summary>
        private bool bLock_VoronoiDiagram = false;

        /// <summary>
        /// voronoi vertex
        /// </summary>
        [SerializeField]
        private GameObject prefab_VoronoiVertex;

        /// <summary>
        /// seed
        /// </summary>
        [SerializeField]
        private GameObject prefab_VoronoiSeedPoint;

        /// <summary>
        /// seed
        /// </summary>
        List<GameObject> list_seedPointVisual = new List<GameObject>();

        /// <summary>
        /// 
        /// </summary>
        private List<GameObject> list_VirtualSutterDelegate = new List<GameObject>();

        /// <summary>
        /// 
        /// </summary>
        private Voronoi voronoi;

        /// <summary>
        /// 
        /// </summary>
        public int totalUserCount = 2;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        private float userRadius = 0.5f;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        private float shutterWidth = 0.5f;

        /// <summary>
        /// 
        /// </summary>
        public bool bMixedExploration = true;

        /// <summary>
        /// 
        /// </summary>
        public Enum_CurriculumState currnet_CurriculumState = Enum_CurriculumState._1stQuater;

        /// <summary>
        /// 
        /// </summary>
        private List<Vector2> list_UsersPrePhysicalPos = new List<Vector2>();
        /// <summary>
        /// 
        /// </summary>
        private List<Vector2> list_UsersCurrentPhysicalPos = new List<Vector2>();
        /// <summary>
        /// 
        /// </summary>
        private List<float> list_UsersCumulativeDist = new List<float>();
        /// <summary>
        /// 
        /// </summary>
        private List<float> users_wallReset_pre = new List<float>();
        /// <summary>
        /// 
        /// </summary>
        private List<float> users_userReset_pre = new List<float>();
        /// <summary>
        /// 
        /// </summary>
        private List<float> users_shutterReset_pre = new List<float>();
        /// <summary>
        /// 
        /// </summary>
        private List<float> UsersCumulative_MDbR_sqeuence = new List<float>();
        /// <summary>
        /// 
        /// </summary>
        private List<float> UsersCumulative_MDbR_SimulationCount_max = new List<float>();

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        private List<float> list_rewardWeight = new List<float>();

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        private GameObject S2C_CenterPointerObject_Prefab;
        /// <summary>
        /// 
        /// </summary>
        List<GameObject> list_S2C_CenterPointer = new List<GameObject>();

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<int, List<Vector2>> dic_AreaSegmentsVertex = new Dictionary<int, List<Vector2>>();

        /// <summary>
        /// 
        /// </summary>
        public List<Material> PartitionedSpaceMaterials = new List<Material>();

        /// <summary>
        /// 
        /// </summary>
        public GameObject prefab_VoronoiEdgesCollider;
        /// <summary>
        /// 
        /// </summary>
        public List<GameObject> list_VoronoiEdgesCollider = new List<GameObject>();

        /// <summary>
        /// 
        /// </summary>
        private int edgeMaxcount = 45;

        /// <summary>
        /// 
        /// </summary>
        private int sptialInfo_windowSize = 450;

        /// <summary>
        /// epsilon for floating point error
        /// </summary>
        private float eps = 0.001f;

        private bool bOneframetimerblockVoronoi = false;


        //------------------------

        private void Awake()
        {
            instance = this;

            //Academy.Instance.AutomaticSteppingEnabled = false;
        }

        private void Start()
        {
            InitializeInfoDics();
            
            InitializeInfoLists(Vector2.zero);

            /// init object pool for visualization center-points of S2C
            for (int i = 0; i < totalUserCount; i++)
            {
                list_S2C_CenterPointer.Add(Instantiate(S2C_CenterPointerObject_Prefab));

                list_S2C_CenterPointer[i].SetActive(false);
            }

            if(totalUserCount < 3 && bEnable_InitPhyUserPosUni)
            {
                bEnable_InitPhyUserPosUni = false;
                bOneframetimerblockVoronoi = true;
            }
        }

        /// <summary>
        /// ML-agent Framework API 
        /// </summary>
        public override void Initialize()
        {
            m_ResetParams = Academy.Instance.EnvironmentParameters;
            //Academy.Instance.AutomaticSteppingEnabled = false;
        }

        /// <summary>
        /// ML-agent Framework API 
        /// </summary>
        public override void OnEpisodeBegin()
        {
            SetResetParameters();
        }

        /// <summary>
        /// ML-agent Framework API 
        /// </summary>
        public override void CollectObservations(VectorSensor sensor)
        {
            if (!RDWSimulationManager.instance.BStart)
                return;

            if (bUseVecOberv)
            {
                /// dummy contatiners
                Queue<float> queue_data = new Queue<float>();
                Dictionary<int, Queue<float>> dic_data = new Dictionary<int, Queue<float>>();

                for (int i = 0; i < totalUserCount; i++)
                {
                    /// physical user pos X
                    dic_physicalUsers_pos_X.TryGetValue(i, out queue_data);
                    float[] array_physicalUser_pos_X = queue_data.ToArray();
                    sensor.AddObservation(array_physicalUser_pos_X);

                    /// physical user pos Z
                    dic_physicalUsers_pos_Z.TryGetValue(i, out queue_data);
                    float[] array_physicalUser_pos_Z = queue_data.ToArray();
                    sensor.AddObservation(array_physicalUser_pos_Z);

                    /// physical user orient Y
                    dic_physicalUsers_orient_Y.TryGetValue(i, out queue_data);
                    float[] array_physicalUser_orient_Y = queue_data.ToArray();
                    sensor.AddObservation(array_physicalUser_orient_Y);

                    /// 8-way distances from physical walls/obstacles
                    doubleDic_physicalUsers_8wayWallDist.TryGetValue(i, out dic_data);
                    for (int j = 0; j < 8; j++)
                    {
                        dic_data.TryGetValue(j, out queue_data);

                        float[] array_oneWayWallDist = queue_data.ToArray();
                        sensor.AddObservation(array_oneWayWallDist);
                    }

                    /// physical sub-space room size
                    dic_physicalRoomSize_users.TryGetValue(i, out queue_data);
                    float[] array_RoomSize = queue_data.ToArray();
                    sensor.AddObservation(array_RoomSize);

                    /// virtual user pos X
                    dic_virtualUsers_pos_X.TryGetValue(i, out queue_data);
                    float[] array_virtualUser_pos_X = queue_data.ToArray();
                    sensor.AddObservation(array_virtualUser_pos_X);

                    /// virtual user pos Z
                    dic_VirtualUsers_Position_Z.TryGetValue(i, out queue_data);
                    float[] array_virtualUser_pos_Z = queue_data.ToArray();
                    sensor.AddObservation(array_virtualUser_pos_Z);

                    /// virtual user orient Y
                    dic_VirtualUsers_Orient_Y.TryGetValue(i, out queue_data);
                    float[] array_virtualUser_orient_Y = queue_data.ToArray();
                    sensor.AddObservation(array_virtualUser_orient_Y);

                    /// 8-way distances from virtual walls/obstacles
                    doubleDic_virtualUsers_8wayWallDist.TryGetValue(i, out dic_data);
                    for (int j = 0; j < 8; j++)
                    {
                        dic_data.TryGetValue(j, out queue_data);

                        float[] array_oneWayWallDist = queue_data.ToArray();
                        sensor.AddObservation(array_oneWayWallDist);
                    }
                }


                //Queue<float> queue_data1 = new Queue<float>();
                //dic_ActualUser00_WallDistance.TryGetValue(0, out queue_data1);

                //float[] array_fromQueue1 = queue_data1.ToArray();

                //StringBuilder sb = new StringBuilder();

                //for (int i = 0; i < queue_data1.Count; i++)
                //{

                //    sb.Append(queue_data1.ToArray().GetValue(i)).Append("/");
                //}

                //Debug.Log("Count : " + queue_data1.Count.ToString() + " || " + sb.ToString());
                //Debug.Log(array_fromQueue1[array_fromQueue1.Length - 1]);
            }

            /// Seed point Init
            for (int i = 0; i < totalUserCount; i++)
            {
                list_VoronoiSeedPoint[i] = new Vector2f(list_physical_simulatedUsers[i].transform.position.x, list_physical_simulatedUsers[i].transform.position.z);
            }

        }

        /// <summary>
        /// ML-agent Framework API 
        /// </summary>
        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            if (!RDWSimulationManager.instance.BStart)
                return;


            /// Init Obstacle mesh info for next episode
            if (StepCount == MaxStep)
            {
                for (int i = 0; i < edgeMaxcount; i++)
                {
                    RDWSimulationManager.instance.InitObstacleInfo(list_VirtualSutterDelegate[i].transform);
                }

                CalcResultPerEpisode();

                return;
            }

            if (!bLock_VoronoiDiagram)
            {
                ///calc maximum available radius
                float allowedRange = CalcStableAreaRadius() - eps;

                /// action values based on the number of users
                int actionValueCount = 0;

                /// decide the action
                for (int i = 0; i < totalUserCount; i++)
                {
                    ///calc actions value
                    var action1 = Mathf.Clamp(actionBuffers.ContinuousActions[actionValueCount++], 0.0f, 1.0f) * allowedRange;
                    var action2 = Mathf.Clamp(actionBuffers.ContinuousActions[actionValueCount++], 0.0f, 1.0f) * 360.0f;

                    ///calc new seed points pos
                    Vector2 pos = new Vector2(list_physical_simulatedUsers[i].transform.position.x, list_physical_simulatedUsers[i].transform.position.z);
                    pos += rotateVec2D(Vector2.right, action2) * action1;
                    list_VoronoiSeedPoint[i] = new Vector2f(pos.x, pos.y);
                }


                /// display seed points
                for (int i = 0; i < totalUserCount; i++)
                {
                    list_seedPointVisual[i].transform.position = new Vector3(list_VoronoiSeedPoint[i].x, 0.0f, list_VoronoiSeedPoint[i].y);
                }


                /// Update Voronoi Diagram for current state
                UpdateVoronoiDiagram();
            }

            if (bEnable_InitPhyUserPosUni && !bLock_VoronoiDiagram)
            {
                bLock_VoronoiDiagram = true;
            }

            if (bOneframetimerblockVoronoi)
            {
                //for (int i = 0; i < totalUserCount; i++)
                //{
                //    Vector2 ranVec = Random.insideUnitCircle * ((physicalRoom_width_half / 2.0f) - 0.26f);

                //    list_physical_simulatedUsers[i].transform.position = new Vector3(list_S2C_CenterPointer[i].transform.position.x + ranVec.x, 0.0f, list_S2C_CenterPointer[i].transform.position.z + ranVec.y);
                //}

                bLock_VoronoiDiagram = true;
            }


            /// Simulate the designated redirection controller
            RDWSimulationManager.instance.SimulateRDW();

            /// Add rewards for current action
            AddRewards();

            /// Update sptial Information
            UpdateSptialInfo();
        }

        /// <summary>
        /// Update physical and virtual sptial Information
        /// </summary>
        private void UpdateSptialInfo()
        {
            Queue<float> queue_data = new Queue<float>();
            Dictionary<int, Queue<float>> dic_data = new Dictionary<int, Queue<float>>();
            Vector3 orient;

            for (int i = 0; i < totalUserCount; i++)
            {
                /// physical user pos X
                dic_physicalUsers_pos_X.TryGetValue(i, out queue_data);
                queue_data.Enqueue(list_physical_simulatedUsers[i].transform.position.x / physicalRoom_width_half);
                queue_data.Dequeue();

                /// physical user pos Z
                dic_physicalUsers_pos_Z.TryGetValue(i, out queue_data);
                queue_data.Enqueue(list_physical_simulatedUsers[i].transform.position.z / physicalRoom_height_half);
                queue_data.Dequeue();

                /// physical user orientation Y
                orient = list_physical_simulatedUsers[i].transform.rotation.eulerAngles;
                dic_physicalUsers_orient_Y.TryGetValue(i, out queue_data);
                queue_data.Enqueue(orient.y);
                queue_data.Dequeue();

                /// 8-way distances from physical walls/obstacles
                doubleDic_physicalUsers_8wayWallDist.TryGetValue(i, out dic_data);
                Calc_8Way_Distances(list_physical_simulatedUsers[i].transform, ref dic_data, true);

                /// physical sub-space room size
                dic_physicalRoomSize_users.TryGetValue(i, out queue_data);
                Enqueue_RoomSize(list_voronoiArea[i], ref queue_data);

                /// virtual user pos X
                dic_virtualUsers_pos_X.TryGetValue(i, out queue_data);
                queue_data.Enqueue(list_virtual_simulatedUsers[i].transform.position.x / virtualRoom_width_half);
                queue_data.Dequeue();

                /// virtual user pos Z
                dic_VirtualUsers_Position_Z.TryGetValue(i, out queue_data);
                queue_data.Enqueue(list_virtual_simulatedUsers[i].transform.position.z / virtualRoom_height_Half);
                queue_data.Dequeue();

                /// virtual user orientation Y
                orient = list_virtual_simulatedUsers[i].transform.rotation.eulerAngles;
                dic_VirtualUsers_Orient_Y.TryGetValue(i, out queue_data);
                queue_data.Enqueue(orient.y);
                queue_data.Dequeue();

                /// 8-way distances from virtual walls/obstacles
                doubleDic_virtualUsers_8wayWallDist.TryGetValue(i, out dic_data);
                Calc_8Way_Distances(list_virtual_simulatedUsers[i].transform, ref dic_data, false);
            }
        }

        /// <summary>
        /// Add rewards for current action
        /// </summary>
        private void AddRewards()
        {
            /// basic reward (positive reward)
            AddReward(10.0f);

            /// wall reset count
            List<float> list_currentTotalWallReset = new List<float>();
            for (int i = 0; i < totalUserCount; i++)
            {
                list_currentTotalWallReset.Add(RDWSimulationManager.instance.GetRedirectedUnits[i].resultData.getWallReset());
            }
            float current_wallReset_mean = list_currentTotalWallReset.Average();
            float gap_wallReset = (current_wallReset_mean - prev_wallReset_mean) * totalUserCount;
            prev_wallReset_mean = current_wallReset_mean;

            /// user reset count
            List<float> list_currentTotalUserReset = new List<float>();
            for (int i = 0; i < totalUserCount; i++)
            {
                list_currentTotalUserReset.Add(RDWSimulationManager.instance.GetRedirectedUnits[i].resultData.getUserReset());
            }
            float current_userReset_mean = list_currentTotalUserReset.Average();
            float gap_userReset = (current_userReset_mean - prev_userReset_mean) * totalUserCount;
            prev_userReset_mean = current_userReset_mean;

            /// shutter reset count
            List<float> list_currentTotalSutterReset = new List<float>();
            for (int i = 0; i < totalUserCount; i++)
            {
                list_currentTotalSutterReset.Add(RDWSimulationManager.instance.GetRedirectedUnits[i].resultData.getShutterReset());
            }
            float current_shutterReset_mean = list_currentTotalSutterReset.Average();
            float gap_shutterReset = (current_shutterReset_mean - prev_shutterReset_mean) * totalUserCount;
            prev_shutterReset_mean = current_shutterReset_mean;

            /// one reset occurs = panelty
            if (gap_wallReset > 0)
            {
                AddReward(list_rewardWeight[1] * gap_wallReset);
            }

            /// one reset occurs = panelty
            if (gap_shutterReset > 0)
            {
                AddReward(list_rewardWeight[2] * gap_shutterReset);
            }

            /// positive deviation occurs = panelty
            if (gap_wallReset > 0 || gap_shutterReset > 0)
            {
                // check all users
                for (int i = 0; i < totalUserCount; i++)
                {
                    float totalReset_deviation = (list_currentTotalWallReset[i] + list_currentTotalSutterReset[i]) - (current_wallReset_mean + current_shutterReset_mean);

                    if (totalReset_deviation > 0)
                    {
                        AddReward(list_rewardWeight[3] * totalReset_deviation);
                    }
                }
            }

            /// acculmulate distance
            for (int i = 0; i < totalUserCount; i++)
            {
                list_UsersCurrentPhysicalPos[i] = new Vector2(RDWSimulationManager.instance.GetRedirectedUnits[i].realUser.transform2D.position.x, RDWSimulationManager.instance.GetRedirectedUnits[i].realUser.transform2D.position.y);

                // In simulation environment, the displacement will be zero when the simulated user resets, but not in user study.
                if (!RDWSimulationManager.instance.GetRedirectedUnits[i].IsResetting)
                {
                    float smalldist = (list_UsersCurrentPhysicalPos[i] - list_UsersPrePhysicalPos[i]).magnitude;
                    list_UsersCumulativeDist[i] += smalldist;

                    // per one step , positive Rewards for physical distnace of simulated users 
                    AddReward(list_rewardWeight[0] * smalldist);
                }

                list_UsersPrePhysicalPos[i] = new Vector2(list_UsersCurrentPhysicalPos[i].x, list_UsersCurrentPhysicalPos[i].y);
            }

            /// acculmulate Mean distance between resets (for wall reset)
            for (int i = 0; i < totalUserCount; i++)
            {
                if (list_currentTotalWallReset[i] != users_wallReset_pre[i])
                {
                    float MDbR_wall = list_UsersCumulativeDist[i];
                    //Debug.Log("MDbR_wall " + MDbR_wall);
                    UsersCumulative_MDbR_sqeuence.Add(MDbR_wall);
                    list_UsersCumulativeDist[i] = 0;
                }
            }

            /// acculmulate Mean distance between resets (for user reset) for Non-OSP Algorithms and Non-UP (e.g. original APF-SC, ...)
            for (int i = 0; i < totalUserCount; i++)
            {
                if (list_currentTotalUserReset[i] != users_userReset_pre[i])
                {
                    float MDbR_user = list_UsersCumulativeDist[i];

                    UsersCumulative_MDbR_sqeuence.Add(MDbR_user);
                    list_UsersCumulativeDist[i] = 0;
                }
            }

            ///  acculmulate Mean distance between resets (for shutter reset)
            for (int i = 0; i < totalUserCount; i++)
            {
                if (list_currentTotalSutterReset[i] != users_shutterReset_pre[i])
                {
                    float MDbR_shutter = list_UsersCumulativeDist[i];
                    //Debug.Log("MDbR_shutter " + MDbR_shutter);
                    UsersCumulative_MDbR_sqeuence.Add(MDbR_shutter);
                    list_UsersCumulativeDist[i] = 0;
                }
            }

            users_wallReset_pre = new List<float>(list_currentTotalWallReset);
            users_userReset_pre = new List<float>(list_currentTotalUserReset);
            users_shutterReset_pre = new List<float>(list_currentTotalSutterReset);

            text_currentStep.text = "Current Step : " + StepCount;
            text_currentReward.text = "Current Reward : " + GetCumulativeReward();
        }

        /// <summary>
        /// Update Vornoi Diagram for current frame
        /// </summary>
        private void UpdateVoronoiDiagram()
        {
            /// define physcial boundary for Voronoi Diagram 
            Rectf bounds = new Rectf(-physicalRoom_width_half, -physicalRoom_height_half, physicalRoom_width_half * 2, physicalRoom_height_half * 2);

            /// Calculate Voronoi Diagram                                                                                                              
            voronoi = new Voronoi(list_VoronoiSeedPoint, bounds);
            sites = voronoi.SitesIndexedByLocation;
            edges = voronoi.Edges;

            /// make Voronoi vertices invisible
            foreach (var item in list_VoronoiVertexMarker)
            {
                item.GetComponent<MeshRenderer>().enabled = false;
            }

            int tempCount = 0;
            int VoronoiEdgeCount = 0;

            ///Update Voronoi edge point marker
            foreach (Edge edge in edges)
            {
                if (edge.ClippedEnds == null)
                    continue;

                list_VoronoiVertexMarker[tempCount].transform.position = new Vector3(edge.ClippedEnds[LR.LEFT].x, 0.0f, edge.ClippedEnds[LR.LEFT].y);
                list_VoronoiVertexMarker[tempCount++].GetComponent<MeshRenderer>().enabled = true;
                list_VoronoiVertexMarker[tempCount].transform.position = new Vector3(edge.ClippedEnds[LR.RIGHT].x, 0.0f, edge.ClippedEnds[LR.RIGHT].y);
                list_VoronoiVertexMarker[tempCount++].GetComponent<MeshRenderer>().enabled = true;
                VoronoiEdgeCount++;
            }


            #region for totalUserCount > 2
            /// Update Voronoi Centronoid and assign centronoi redirection target each user
            if (totalUserCount > 2)
            {
                list_voronoiCentroid.Clear();
                list_voronoiArea.Clear();
                List<Vector2f> centroids = new List<Vector2f>();

                SiteList sites_forcentronoid = new SiteList();
                centroids = voronoi.GetCentroid_site(totalUserCount, ref list_voronoiArea, ref sites_forcentronoid);

                for (int i = 0; i < totalUserCount; i++)
                {
                    list_voronoiCentroid.Add(new Vector2(centroids[i].x, centroids[i].y));
                }

                if (list_voronoiCentroid.Count == totalUserCount && !float.IsNaN(list_voronoiCentroid[0].x) && !float.IsNaN(list_voronoiCentroid[0].y))
                {
                    for (int i = 0; i < totalUserCount; i++)
                    {
                        int targetIndex = 0;
                        for (int j = 0; j < totalUserCount; j++)
                        {
                            if (Mathf.Abs(list_VoronoiSeedPoint[i].x - sites_forcentronoid.GetSite_byIndex(j).x) < eps
                                && Mathf.Abs(list_VoronoiSeedPoint[i].y - sites_forcentronoid.GetSite_byIndex(j).y) < eps)
                            {
                                targetIndex = j;
                            }
                            else
                            {
                                continue;
                            }
                        }

                        if (bUseVecOberv)
                        {
                            list_S2C_CenterPointer[i].transform.position = new Vector3(list_voronoiCentroid[targetIndex].x, 0.0f, list_voronoiCentroid[targetIndex].y);
                        }

                        list_S2C_CenterPointer[i].name = "centroid for user" + targetIndex;

                        List<Vector2> list_AreaSegmentsVertex = new List<Vector2>();
                        dic_AreaSegmentsVertex.TryGetValue(i, out list_AreaSegmentsVertex);
                        list_AreaSegmentsVertex.Clear();

                        List<Vector2f> region = sites_forcentronoid.GetSite_byIndex(targetIndex).Region(bounds);

                        for (int k = region.Count - 1; k >= 0; k--)
                        {
                            list_AreaSegmentsVertex.Add(new Vector2(region[k].x, region[k].y));
                        }

                        //just a moment shut down code (for APF)
                        if (RDWSimulationManager.instance.GetRedirectedUnits[i].GetRedirector() is S2CRedirector)
                        {
                            ((S2CRedirector)RDWSimulationManager.instance.GetRedirectedUnits[i].GetRedirector()).SetCenterPoint(list_S2C_CenterPointer[i].transform.position);
                            list_S2C_CenterPointer[i].gameObject.SetActive(true);
                        }

                    }
                }
                else
                {
                    Debug.LogError("IsNaN");
                }
            }
            #endregion

            #region for totalUserCount == 2
            ///Update voronoi Centronoid and assign centronoi redirection target each user
            if (totalUserCount == 2)
            {
                list_voronoiCentroid.Clear();
                list_voronoiArea.Clear();
                List<Vector2f> centroids = new List<Vector2f>();
                List<float> areas = new List<float>();
                float areavalue = 0.0f;
                (Vector2f, List<Vector2f>) item = voronoi.GetCentroidTwin_region(0, true, ref areavalue);
                (Vector2f, List<Vector2f>) item2 = voronoi.GetCentroidTwin_region(1, false, ref areavalue);
                centroids.Add(item.Item1);
                areas.Add(areavalue);
                centroids.Add(item2.Item1);
                areas.Add(areavalue);

                Vector3 vec1 = list_VoronoiVertexMarker[1].transform.position - list_VoronoiVertexMarker[0].transform.position;
                Vector3 cntr1 = new Vector3(centroids[0].x, 0.0f, centroids[0].y) - list_VoronoiVertexMarker[0].transform.position;
                Vector3 vec2 = list_physical_simulatedUsers[0].transform.position - list_VoronoiVertexMarker[0].transform.position;

                List<Vector2> list_AreaSegmentsVertex0 = new List<Vector2>();
                dic_AreaSegmentsVertex.TryGetValue(0, out list_AreaSegmentsVertex0);
                list_AreaSegmentsVertex0.Clear();

                List<Vector2> list_AreaSegmentsVertex1 = new List<Vector2>();
                dic_AreaSegmentsVertex.TryGetValue(1, out list_AreaSegmentsVertex1);
                list_AreaSegmentsVertex1.Clear();

                List<Vector2f> aa = new List<Vector2f>();

                if (Mathf.Sign(Vector3.Cross(vec1, cntr1).y) == Mathf.Sign(Vector3.Cross(vec1, vec2).y))
                {
                    //Debug.Log("true");
                    list_voronoiCentroid.Add(new Vector2(centroids[0].x, centroids[0].y));
                    list_voronoiArea.Add(areas[0]);
                    for (int j = item.Item2.Count - 1; j >= 0; j--)
                    {
                        list_AreaSegmentsVertex0.Add(new Vector2(item.Item2[j].x, item.Item2[j].y));
                    }

                    list_voronoiCentroid.Add(new Vector2(centroids[1].x, centroids[1].y));
                    list_voronoiArea.Add(areas[1]);
                    for (int j = item2.Item2.Count - 1; j >= 0; j--)
                    {
                        list_AreaSegmentsVertex1.Add(new Vector2(item2.Item2[j].x, item2.Item2[j].y));
                    }
                }
                else
                {
                    //Debug.Log("false");
                    list_voronoiArea.Add(areas[1]);
                    list_voronoiCentroid.Add(new Vector2(centroids[1].x, centroids[1].y));
                    for (int j = item2.Item2.Count - 1; j >= 0; j--)
                    {
                        list_AreaSegmentsVertex0.Add(new Vector2(item2.Item2[j].x, item2.Item2[j].y));
                    }

                    list_voronoiArea.Add(areas[0]);
                    list_voronoiCentroid.Add(new Vector2(centroids[0].x, centroids[0].y));
                    for (int j = item.Item2.Count - 1; j >= 0; j--)
                    {
                        list_AreaSegmentsVertex1.Add(new Vector2(item.Item2[j].x, item.Item2[j].y));
                    }
                }


                if (list_voronoiCentroid.Count == totalUserCount && !float.IsNaN(list_voronoiCentroid[0].x) && !float.IsNaN(list_voronoiCentroid[0].y))
                {
                    for (int i = 0; i < totalUserCount; i++)
                    {
                        if (bUseVecOberv)
                        {
                            list_S2C_CenterPointer[i].transform.position = new Vector3(list_voronoiCentroid[i].x, 0.0f, list_voronoiCentroid[i].y);
                        }

                        //just a moment shut down code (for APF)
                        if (RDWSimulationManager.instance.GetRedirectedUnits[i].GetRedirector() is S2CRedirector)
                        {
                            ((S2CRedirector)RDWSimulationManager.instance.GetRedirectedUnits[i].GetRedirector()).SetCenterPoint(list_S2C_CenterPointer[i].transform.position);
                            list_S2C_CenterPointer[i].gameObject.SetActive(true);
                        }


                    }

                }
            }
            #endregion

            int VoronoiVertexCount = 0;
            int count2 = (totalUserCount * (totalUserCount - 1));
            int count3 = 0;

            while (VoronoiVertexCount + 2 <= count2)
            {
                list_WayPoint_vertices.Clear();

                if (VoronoiVertexCount >= VoronoiEdgeCount * 2)
                {
                    Vector2 dir = new Vector2(list_VoronoiVertexMarker[1].transform.position.x, list_VoronoiVertexMarker[1].transform.position.z) - new Vector2(list_VoronoiVertexMarker[0].transform.position.x, list_VoronoiVertexMarker[0].transform.position.z);
                    Vector2 dir_perpendicular = Vector2.Perpendicular(dir);
                    Vector2 dir_perpendicular_unit = dir_perpendicular.normalized * (shutterWidth / 2);

                    list_WayPoint_vertices.Add(new Vector2(-51, 1));
                    list_WayPoint_vertices.Add(new Vector2(-49, 1));
                    list_WayPoint_vertices.Add(new Vector2(-49, -1));
                    list_WayPoint_vertices.Add(new Vector2(-51, -1));

                    list_VoronoiEdgesCollider[count3].transform.forward = new Vector3(dir.x, 0.0f, dir.y);
                    float x = (list_VoronoiVertexMarker[1].transform.position.x + list_VoronoiVertexMarker[0].transform.position.x) / 2;
                    float z = (list_VoronoiVertexMarker[1].transform.position.z + list_VoronoiVertexMarker[0].transform.position.z) / 2;
                    list_VoronoiEdgesCollider[count3].transform.position = new Vector3(x, 0.0f, z);
                    list_VoronoiEdgesCollider[count3].transform.localScale = new Vector3(shutterWidth, 2.0f, dir.magnitude);
                }
                else
                {
                    Vector2 dir = new Vector2(list_VoronoiVertexMarker[VoronoiVertexCount + 1].transform.position.x, list_VoronoiVertexMarker[VoronoiVertexCount + 1].transform.position.z) - new Vector2(list_VoronoiVertexMarker[VoronoiVertexCount].transform.position.x, list_VoronoiVertexMarker[VoronoiVertexCount].transform.position.z);
                    Vector2 dir_perpendicular = Vector2.Perpendicular(dir);
                    Vector2 dir_perpendicular_unit = dir_perpendicular.normalized * (shutterWidth / 2);

                    if ((list_VoronoiVertexMarker[VoronoiVertexCount + 1].transform.position - list_VoronoiVertexMarker[VoronoiVertexCount].transform.position).magnitude < 0.1f)
                    {
                        list_WayPoint_vertices.Add(new Vector2(-51, 1));
                        list_WayPoint_vertices.Add(new Vector2(-49, 1));
                        list_WayPoint_vertices.Add(new Vector2(-49, -1));
                        list_WayPoint_vertices.Add(new Vector2(-51, -1));
                    }
                    else
                    {
                        list_WayPoint_vertices.Add(new Vector2(list_VoronoiVertexMarker[VoronoiVertexCount].transform.position.x - dir_perpendicular_unit.x, list_VoronoiVertexMarker[VoronoiVertexCount].transform.position.z - dir_perpendicular_unit.y));
                        list_WayPoint_vertices.Add(new Vector2(list_VoronoiVertexMarker[VoronoiVertexCount].transform.position.x + dir_perpendicular_unit.x, list_VoronoiVertexMarker[VoronoiVertexCount].transform.position.z + dir_perpendicular_unit.y));
                        list_WayPoint_vertices.Add(new Vector2(list_VoronoiVertexMarker[VoronoiVertexCount + 1].transform.position.x + dir_perpendicular_unit.x, list_VoronoiVertexMarker[VoronoiVertexCount + 1].transform.position.z + dir_perpendicular_unit.y));
                        list_WayPoint_vertices.Add(new Vector2(list_VoronoiVertexMarker[VoronoiVertexCount + 1].transform.position.x - dir_perpendicular_unit.x, list_VoronoiVertexMarker[VoronoiVertexCount + 1].transform.position.z - dir_perpendicular_unit.y));

                        list_VoronoiEdgesCollider[count3].transform.forward = new Vector3(dir.x, 0.0f, dir.y);

                        float x = (list_VoronoiVertexMarker[VoronoiVertexCount + 1].transform.position.x + list_VoronoiVertexMarker[VoronoiVertexCount].transform.position.x) / 2;
                        float z = (list_VoronoiVertexMarker[VoronoiVertexCount + 1].transform.position.z + list_VoronoiVertexMarker[VoronoiVertexCount].transform.position.z) / 2;
                        list_VoronoiEdgesCollider[count3].transform.position = new Vector3(x, 0.0f, z);
                        list_VoronoiEdgesCollider[count3].transform.localScale = new Vector3(shutterWidth, 2.0f, dir.magnitude);
                    }
                }

                /// just for display
                list_VirtualSutterDelegate[count3].transform.position = Vector3.zero + Vector3.up * 0.02f;


                ///Update obstacle mesh info
                RDWSimulationManager.instance.UpdateObstacleVertexInfo(ref list_WayPoint_vertices, list_VirtualSutterDelegate[count3].transform, count3);

                VoronoiVertexCount += 2;
                count3++;
            }
        }

        /// Calculate the radius of a circular region where Voronoi Seed Points can be located
        private float CalcStableAreaRadius()
        {
            /// min distance of bet. users = calculate stable radius for new seed points
            Dictionary<string, float> dist_dic = new Dictionary<string, float>();
            for (int i = 0; i < totalUserCount; i++)
            {
                Vector3 me = new Vector3(list_VoronoiSeedPoint[i].x, 0.0f, list_VoronoiSeedPoint[i].y);
                Vector3 target = Vector3.zero;

                for (int j = i; j < totalUserCount; j++)
                {
                    if (i == j)
                        continue;

                    target = new Vector3(list_VoronoiSeedPoint[j].x, 0.0f, list_VoronoiSeedPoint[j].y);
                    Vector3 distVec_users00_01 = me - target;

                    dist_dic.Add(i.ToString()+j.ToString(), distVec_users00_01.magnitude);
                }
            }

            ///calc min dist
            var keyAndValue = dist_dic.OrderBy(kvp => kvp.Value).First();
            float minDistValue = keyAndValue.Value;

            ///calc maximum available radius
            return ((minDistValue - shutterWidth) / 2) - userRadius;
        }

        /// <summary>
        /// Calculate resaults per episode
        /// </summary>
        private void CalcResultPerEpisode()
        {
            if (StepCount == 16200)
            {
                ///wall reset count
                List<int> list_userWallReset = new List<int>();
                for (int i = 0; i < totalUserCount; i++)
                {
                    list_userWallReset.Add((int)(RDWSimulationManager.instance.GetRedirectedUnits[i].resultData.getWallReset()));
                }
                int wallResetSum = list_userWallReset.Sum();

                ///user reset count
                //List<int> list_useruserReset = new List<int>();
                //for (int i = 0; i < totalUserCount; i++)
                //{
                //    list_useruserReset.Add((int)(RDWSimulationManager.instance.GetRedirectedUnits[i].resultData.getUserReset()));
                //}
                //int userResetSum = list_useruserReset.Sum();
                //int userResetSum = list_useruserReset.Sum();

                ///shutter reset count
                List<int> list_userShutterReset = new List<int>();
                for (int i = 0; i < totalUserCount; i++)
                {
                    list_userShutterReset.Add((int)(RDWSimulationManager.instance.GetRedirectedUnits[i].resultData.getShutterReset()));
                }
                int ShutterResetSum = list_userShutterReset.Sum();

                /// user reset zero concept
                //int userbet = 0;
                //int userbet = userResetSum;
                int userbet = RDWSimulationManager.instance.Calc_UserResetFilter();

                list_usersTotalReset_perEpisode.Add(wallResetSum + userbet + ShutterResetSum);
                list_usersShutterReset_perEpisode.Add(ShutterResetSum);
                list_userbetReset_perEpisode.Add(userbet);

                float MDbR_AVG = 0.0f;
                if (UsersCumulative_MDbR_sqeuence.Count > 0)
                {
                    MDbR_AVG = UsersCumulative_MDbR_sqeuence.Average();
                }
                else
                {
                    MDbR_AVG = list_UsersCumulativeDist.Average();
                }


                Debug.LogWarning(string.Format("walllreset {0} / userreset {1} /shutterreset {2} / MDbR AVg. {3}", wallResetSum, userbet, ShutterResetSum, MDbR_AVG));

                UsersCumulative_MDbR_SimulationCount_max.Add(MDbR_AVG);

                StringBuilder sb = new StringBuilder();

                sb.Append(',');
                sb.Append(',');
                sb.Append(wallResetSum).Append(',');
                sb.Append(wallResetSum + ShutterResetSum + userbet).Append(',');
                sb.Append(userbet).Append(',');
                sb.Append(ShutterResetSum).Append(',');
                sb.Append(MDbR_AVG).Append(',');

                //sb.AppendFormat("{0:F4}", GetCumulativeReward()).Append(',');

                if (sb.Length > 0 && sb[sb.Length - 1] == ',')
                {
                    sb.Remove(sb.Length - 1, 1);
                }

                GM_DataRecord.instance.Enequeue_Data(sb.ToString());

                currentSimulationCount++;

                text_currentEpi.text = "Current Episode : " + (currentSimulationCount + 1);

                if(currentSimulationCount + 1  == 2500)
                {
                    currnet_CurriculumState = Enum_CurriculumState._2ndQuater;
                }
                else if (currentSimulationCount + 1 == 5000)
                {
                    currnet_CurriculumState = Enum_CurriculumState._3rdQuater;
                }
                else if (currentSimulationCount + 1 == 7500)
                {
                    currnet_CurriculumState = Enum_CurriculumState._4thQuater;
                }


                if (currentSimulationCount == SimulationCount_max)
                {
                    currentSimulationCount = 0;
                    GM_DataRecord.instance.Write_Warning(GM_DataRecord.Warning_Type.TotalResetMean, list_usersTotalReset_perEpisode.Average().ToString("F3"));
                    GM_DataRecord.instance.Write_Warning(GM_DataRecord.Warning_Type.TotalResetMean, getStandardDeviation(list_usersTotalReset_perEpisode).ToString("F3"));
                    GM_DataRecord.instance.Write_Warning(GM_DataRecord.Warning_Type.UserbetResetMean, list_userbetReset_perEpisode.Average().ToString("F3"));
                    GM_DataRecord.instance.Write_Warning(GM_DataRecord.Warning_Type.UserbetResetMean, getStandardDeviation(list_userbetReset_perEpisode).ToString("F3"));
                    GM_DataRecord.instance.Write_Warning(GM_DataRecord.Warning_Type.UsershutterResetMean, list_usersShutterReset_perEpisode.Average().ToString("F3"));
                    GM_DataRecord.instance.Write_Warning(GM_DataRecord.Warning_Type.UsershutterResetMean, getStandardDeviation(list_usersShutterReset_perEpisode).ToString("F3"));
                    GM_DataRecord.instance.Write_Warning(GM_DataRecord.Warning_Type.MeanDistBetResets, UsersCumulative_MDbR_SimulationCount_max.Average().ToString("F3"));
                    GM_DataRecord.instance.Write_Warning(GM_DataRecord.Warning_Type.MeanDistBetResets, getStandardDeviation(UsersCumulative_MDbR_SimulationCount_max).ToString("F3"));
                    GM_DataRecord.instance.Save_SteamingData_Batch();

                    UsersCumulative_MDbR_SimulationCount_max.Clear();
                }
            }
        }

        private void FixedUpdate()
        {

        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {

        }

        /// <summary>
        /// Initialize Dictionary for spatial information
        /// </summary>
        private void InitializeInfoDics()
        {
            edgeMaxcount = (totalUserCount * (totalUserCount - 1)) / 2;

            for (int i = 0; i < edgeMaxcount; i++)
            {
                list_VoronoiEdgesCollider.Add(Instantiate(prefab_VoronoiEdgesCollider));
            }

            for (int i = 0; i < 100; i++)
            {
                list_VoronoiVertexMarker.Add(Instantiate(prefab_VoronoiVertex, new Vector3(-50.0f, 0.0f, 0.0f), Quaternion.identity));
                list_VoronoiVertexMarker[i].SetActive(false);
                list_VoronoiVertexMarker[i].name = "VoronoiVertexMarker " + i;
            }

            for (int i = 0; i < 10; i++)
            {
                list_seedPointVisual.Add(Instantiate(prefab_VoronoiSeedPoint, new Vector3(-50.0f, 0.0f, 0.0f), Quaternion.identity));
                list_seedPointVisual[i].name = "seedpointView " + i;
                //list_seedpointView[i].SetActive(false);
            }

            for (int i = 0; i < edgeMaxcount; i++)
            {
                GameObject go = new GameObject();
                list_VirtualSutterDelegate.Add(go);
                list_VirtualSutterDelegate[i].transform.position = Vector3.zero + Vector3.up * 0.02f;
                list_VirtualSutterDelegate[i].name = "VirtualSutterDelegate " + i;
            }

            for (int i = 0; i < totalUserCount; i++)
            {
                Queue<float> queue = new Queue<float>();
                dic_physicalUsers_pos_X.Add(i, queue);
            }

            for (int i = 0; i < totalUserCount; i++)
            {
                Queue<float> queue = new Queue<float>();
                dic_physicalUsers_pos_Z.Add(i, queue);
            }

            for (int i = 0; i < totalUserCount; i++)
            {
                Queue<float> queue = new Queue<float>();
                dic_physicalUsers_orient_Y.Add(i, queue);
            }

            for (int i = 0; i < totalUserCount; i++)
            {
                Queue<float> queue = new Queue<float>();
                dic_physicalRoomSize_users.Add(i, queue);
            }

            for (int i = 0; i < totalUserCount; i++)
            {
                Queue<float> queue = new Queue<float>();
                dic_virtualUsers_pos_X.Add(i, queue);
            }

            for (int i = 0; i < totalUserCount; i++)
            {
                Queue<float> queue = new Queue<float>();
                dic_VirtualUsers_Position_Z.Add(i, queue);
            }

            for (int i = 0; i < totalUserCount; i++)
            {
                Queue<float> queue = new Queue<float>();
                dic_VirtualUsers_Orient_Y.Add(i, queue);
            }

            for (int i = 0; i < totalUserCount; i++)
            {
                Dictionary<int, Queue<float>> dic_data = new Dictionary<int, Queue<float>>();
                for (int j = 0; j < 8; j++)
                {
                    Queue<float> queue = new Queue<float>();
                    dic_data.Add(j, queue);

                    //Debug.Log("dic_data : " + dic_data.Count);
                }
                doubleDic_physicalUsers_8wayWallDist.Add(i, dic_data);
            }

            //Debug.Log("doubleDic_ActualUsers_8wayWallDist : " + doubleDic_ActualUsers_8wayWallDist.Count);

            for (int i = 0; i < totalUserCount; i++)
            {
                Dictionary<int, Queue<float>> dic_data = new Dictionary<int, Queue<float>>();
                for (int j = 0; j < 8; j++)
                {
                    Queue<float> queue = new Queue<float>();
                    dic_data.Add(j, queue);
                }
                doubleDic_virtualUsers_8wayWallDist.Add(i, dic_data);
            }
            for (int i = 0; i < totalUserCount; i++)
            {
                List<Vector2> list = new List<Vector2>();
                dic_AreaSegmentsVertex.Add(i, list);
            }
        }

        /// <summary>
        /// Initialize list for spatial information
        /// </summary>
        /// <param name="_initUserPhyiscalPos"></param>
        private void InitializeInfoLists(Vector2 _initUserPhyiscalPos)
        {
            list_UsersCumulativeDist.Clear();
            list_UsersCurrentPhysicalPos.Clear();
            list_UsersPrePhysicalPos.Clear();
            users_wallReset_pre.Clear();
            users_userReset_pre.Clear();
            users_shutterReset_pre.Clear();
            for (int i = 0; i < totalUserCount; i++)
            {
                list_UsersCumulativeDist.Add(0.0f);
                list_UsersCurrentPhysicalPos.Add(_initUserPhyiscalPos);
                list_UsersPrePhysicalPos.Add(_initUserPhyiscalPos);

                users_wallReset_pre.Add(0);
                users_userReset_pre.Add(0);
                users_shutterReset_pre.Add(0);
            }
        }




        /// <summary>
        /// Reset the parameters
        /// </summary>
        public void SetResetParameters()
        {
            RDWSimulationManager.instance.BStart = false;
            RDWSimulationManager.instance.StartSimulation();

            physicalRoom_width_half = Mathf.Abs(RDWSimulationManager.instance.simulationSetting.realSpaceSetting.spaceObjectSetting.vertices[0].x);
            physicalRoom_height_half = Mathf.Abs(RDWSimulationManager.instance.simulationSetting.realSpaceSetting.spaceObjectSetting.vertices[0].y);

            virtualRoom_width_half = Mathf.Abs(RDWSimulationManager.instance.simulationSetting.virtualSpaceSetting.spaceObjectSetting.vertices[0].x);
            virtualRoom_height_Half = Mathf.Abs(RDWSimulationManager.instance.simulationSetting.virtualSpaceSetting.spaceObjectSetting.vertices[0].y);

            actual_halfRoomsize_default = physicalRoom_height_half * 2 * physicalRoom_width_half;
            actual_roomhypotenuse = Mathf.Sqrt(Mathf.Pow(physicalRoom_width_half, 2) + Mathf.Pow(physicalRoom_height_half, 2));

            /// refresh pointers for physical user 
            foreach (var item in list_physical_simulatedUsers)
            {
                DestroyImmediate(item);
            }

            list_physical_simulatedUsers.Clear();

            for (int i = 0; i < totalUserCount; i++)
            {
                GameObject actuser = GameObject.FindWithTag("RealUser" + i);
                list_physical_simulatedUsers.Add(actuser);
            }

            /// refresh pointers for virtual user 
            foreach (var item in list_virtual_simulatedUsers)
            {
                DestroyImmediate(item);
            }

            list_virtual_simulatedUsers.Clear();

            for (int i = 0; i < totalUserCount; i++)
            {
                GameObject viruser = GameObject.FindWithTag("VirtualUser" + i);
                viruser.GetComponent<CapsuleCollider>().enabled = false;
                list_virtual_simulatedUsers.Add(viruser);
            }

            //Debug.Log("A");

            if (bEnable_InitPhyUserPosUni && currentSimulationCount == 0)
            {
                list_VoronoiSeedPoint_Fixed.Clear();

                for (int i = 0; i < totalUserCount; i++)
                {
                    list_VoronoiSeedPoint_Fixed.Add(new Vector2f(Random.Range(-physicalRoom_width_half, physicalRoom_width_half), Random.Range(-physicalRoom_height_half, physicalRoom_height_half)));
                }
                Rectf bounds = new Rectf(-physicalRoom_width_half, -physicalRoom_height_half, physicalRoom_width_half * 2, physicalRoom_height_half * 2);

                //Debug.Log("B:" + list_VoronoiSeedPoint_Fixed.Count);

                voronoi = new Voronoi(list_VoronoiSeedPoint_Fixed, bounds, 5000);
                sites = voronoi.SitesIndexedByLocation;
                int count = 0;
                foreach (KeyValuePair<Vector2f, Site> kv in sites)
                {
                    list_VoronoiSeedPoint_Fixed[count++] = new Vector2f(kv.Key.x, kv.Key.y);
                }
                //Debug.Log("C:" + list_VoronoiSeedPoint_Fixed.Count);

                List<Vector2f> centroids = new List<Vector2f>();

                SiteList sites_forcentronoid = new SiteList();
                centroids = voronoi.GetCentroid_site(totalUserCount, ref list_voronoiArea, ref sites_forcentronoid);
                int targetIndex = 0;
                List<Vector2f> list_VoronoiSeedPoint_Fixed_temp = new List<Vector2f>();
                //Debug.Log("D:" + list_VoronoiSeedPoint_Fixed.Count);

                for (int i = 0; i < totalUserCount; i++)
                {
                    for (int j = 0; j < totalUserCount; j++)
                    {
                        if (Mathf.Abs(list_VoronoiSeedPoint_Fixed[i].x - sites_forcentronoid.GetSite_byIndex(j).x) < eps
                            && Mathf.Abs(list_VoronoiSeedPoint_Fixed[i].y - sites_forcentronoid.GetSite_byIndex(j).y) < eps)
                        {
                            targetIndex = j;
                        }
                        else
                        {
                            continue;
                        }

                        list_VoronoiSeedPoint_Fixed_temp.Add(list_VoronoiSeedPoint_Fixed[targetIndex]);

                    }
                }
                //Debug.Log("E:" + list_VoronoiSeedPoint_Fixed.Count);
                list_VoronoiSeedPoint_Fixed = list_VoronoiSeedPoint_Fixed_temp;
            }

            //Debug.Log("F:" + list_VoronoiSeedPoint_Fixed.Count);

            if (bEnable_InitPhyUserPosUni)
            {
                //Debug.Log("G:" + list_physical_simulatedUsers.Count);

                for (int i = 0; i < totalUserCount; i++)
                {
                    list_physical_simulatedUsers[i].transform.position = new Vector3(list_VoronoiSeedPoint_Fixed[i].x, 0.0f, list_VoronoiSeedPoint_Fixed[i].y);
                }

                if(totalUserCount == 4)
                {
                    for (int i = 0; i < totalUserCount; i++)
                    {
                        list_physical_simulatedUsers[i].transform.Translate(Vector3.forward * Random.Range(-0.001f, 0.001f));
                    }
                }
            }

            bLock_VoronoiDiagram = false;




            prev_wallReset_mean = 0;

            InitializeInfoLists(Vector2.zero);

            UsersCumulative_MDbR_sqeuence.Clear();

            InitializeInfoQueues();
        }

        /// <summary>
        /// Initialize queue for spatial information
        /// </summary>
        public void InitializeInfoQueues()
        {
            Queue<float> queue_data = new Queue<float>();
            Dictionary<int, Queue<float>> dic_data = new Dictionary<int, Queue<float>>();

            for (int i = 0; i < totalUserCount; i++)
            {
                dic_physicalUsers_pos_X.TryGetValue(i, out queue_data);
                queue_data.Clear();

                dic_physicalUsers_pos_Z.TryGetValue(i, out queue_data);
                queue_data.Clear();

                dic_physicalUsers_orient_Y.TryGetValue(i, out queue_data);
                queue_data.Clear();

                doubleDic_physicalUsers_8wayWallDist.TryGetValue(i, out dic_data);
                for (int j = 0; j < 8; j++)
                {
                    dic_data.TryGetValue(j, out queue_data);
                    queue_data.Clear();
                }

                dic_physicalRoomSize_users.TryGetValue(i, out queue_data);
                queue_data.Clear();

                dic_virtualUsers_pos_X.TryGetValue(i, out queue_data);
                queue_data.Clear();

                dic_VirtualUsers_Position_Z.TryGetValue(i, out queue_data);
                queue_data.Clear();

                dic_VirtualUsers_Orient_Y.TryGetValue(i, out queue_data);
                queue_data.Clear();

                doubleDic_virtualUsers_8wayWallDist.TryGetValue(i, out dic_data);
                for (int j = 0; j < 8; j++)
                {
                    dic_data.TryGetValue(j, out queue_data);
                    queue_data.Clear();
                }
            }


            list_VoronoiSeedPoint.Clear();
            for (int i = 0; i < totalUserCount; i++)
            {
                list_VoronoiSeedPoint.Add(new Vector2f(0.0f, 0.0f));
            }

            for (int i = 0; i < totalUserCount; i++)
            {
                dic_physicalUsers_pos_X.TryGetValue(i, out queue_data);
                for (int j = 0; j < sptialInfo_windowSize; j++)
                {
                    queue_data.Enqueue(RDWSimulationManager.instance.simulationSetting.unitSettings[i].realStartPosition.x / physicalRoom_width_half);
                }

                dic_physicalUsers_pos_Z.TryGetValue(i, out queue_data);
                for (int j = 0; j < sptialInfo_windowSize; j++)
                {
                    queue_data.Enqueue(RDWSimulationManager.instance.simulationSetting.unitSettings[i].realStartPosition.x / physicalRoom_width_half);
                }

                dic_physicalUsers_orient_Y.TryGetValue(i, out queue_data);
                Vector3 orient = list_physical_simulatedUsers[i].transform.rotation.eulerAngles;
                for (int j = 0; j < sptialInfo_windowSize; j++)
                {
                    queue_data.Enqueue(orient.y);
                }

                doubleDic_physicalUsers_8wayWallDist.TryGetValue(i, out dic_data);
                for (int j = 0; j < 8; j++)
                {
                    dic_data.TryGetValue(j, out queue_data);

                    for (int k = 0; k < sptialInfo_windowSize; k++)
                    {
                        queue_data.Enqueue(physicalRoom_height_half / 2);
                    }
                }

                dic_physicalRoomSize_users.TryGetValue(i, out queue_data);
                for (int j = 0; j < sptialInfo_windowSize; j++)
                {
                    queue_data.Enqueue((actual_halfRoomsize_default * 2) / totalUserCount);
                }

                dic_virtualUsers_pos_X.TryGetValue(i, out queue_data);
                for (int j = 0; j < sptialInfo_windowSize; j++)
                {
                    queue_data.Enqueue(RDWSimulationManager.instance.simulationSetting.unitSettings[i].virtualStartPosition.x / virtualRoom_width_half);
                }

                dic_VirtualUsers_Position_Z.TryGetValue(i, out queue_data);
                for (int j = 0; j < sptialInfo_windowSize; j++)
                {
                    queue_data.Enqueue(RDWSimulationManager.instance.simulationSetting.unitSettings[i].virtualStartPosition.y / virtualRoom_height_Half);
                }

                dic_VirtualUsers_Orient_Y.TryGetValue(i, out queue_data);
                orient = list_virtual_simulatedUsers[i].transform.rotation.eulerAngles;
                for (int j = 0; j < sptialInfo_windowSize; j++)
                {
                    queue_data.Enqueue(orient.y);
                }

                doubleDic_virtualUsers_8wayWallDist.TryGetValue(i, out dic_data);
                for (int j = 0; j < 8; j++)
                {
                    dic_data.TryGetValue(j, out queue_data);

                    for (int k = 0; k < sptialInfo_windowSize; k++)
                    {
                        queue_data.Enqueue(virtualRoom_height_Half / 2);
                    }
                }



            }
        }

        /// <summary>
        /// calculate 8 distances from physical/virtual wall, obstacle, shutter
        /// </summary>
        /// <param name="_origintransform"></param>
        /// <param name="_distanceDic"></param>
        /// <param name="bActual"></param>
        public void Calc_8Way_Distances(Transform _origintransform, ref Dictionary<int, Queue<float>> _distanceDic, bool bActual)
        {
            float distance = 0.0f;

            List<Vector3> direction = new List<Vector3>();

            direction.Add(Vector3.right);
            direction.Add((Vector3.right + -Vector3.forward).normalized);
            direction.Add(-Vector3.forward);
            direction.Add((-Vector3.right + -Vector3.forward).normalized);
            direction.Add(-Vector3.right);
            direction.Add((-Vector3.right + Vector3.forward).normalized);
            direction.Add(Vector3.forward);
            direction.Add((Vector3.right + Vector3.forward).normalized);

            for (int i = 0; i < direction.Count; i++)
            {
                if (Physics.Raycast(_origintransform.position, direction[i], out rayCastHit, 100.0f))
                {
                    if (bActual)
                    {
                        if (rayCastHit.collider.gameObject.layer == LayerMask.NameToLayer("PhysicalWall"))
                        {
                            distance = rayCastHit.distance;
                            //Debug.Log(hit.collider.gameObject.name);
                        }
                        //else
                        //{
                        //    for (int j = 0; j < totalUserCount; j++)
                        //    {
                        //        if (hit.collider.gameObject.tag == "RealUser" + j)
                        //        {
                        //            distance = hit.distance;
                        //            break;
                        //        }
                        //    }
                        //}
                    }
                    else
                    {
                        if (rayCastHit.collider.gameObject.layer == LayerMask.NameToLayer("VirtualWall"))
                        {
                            distance = rayCastHit.distance;
                        }
                    }

                }


                Queue<float> queue_data;
                _distanceDic.TryGetValue(i, out queue_data);
                queue_data.Enqueue(distance);
                queue_data.Dequeue();

                distance = 0.0f;
            }
        }


        public void Enqueue_RoomSize(float element, ref Queue<float> _queue)
        {
            _queue.Enqueue(element);
            _queue.Dequeue();
        }

        public void Enqueue_Veloc(float element, ref Queue<float> _queue)
        {
            _queue.Enqueue(element);
            _queue.Dequeue();
        }

        private void CalculateCircleTangentPoint(out Vector3 P1, Vector3 circleCenterlPoint, Vector3 externalPoint, float radius)
        {
            float distanceBetP_C = Mathf.Sqrt(Mathf.Pow(externalPoint.x - circleCenterlPoint.x, 2) + Mathf.Pow(externalPoint.z - circleCenterlPoint.z, 2));
            float theta = Mathf.Acos(radius / distanceBetP_C);
            float d = Mathf.Atan2(externalPoint.z - circleCenterlPoint.z, externalPoint.x - circleCenterlPoint.x);
            float d1 = d + theta;
            float d2 = d - theta;
            Vector3 T1 = new Vector3(circleCenterlPoint.x + radius * Mathf.Cos(d1), 0.0f, circleCenterlPoint.z + radius * Mathf.Sin(d1));
            Vector3 T2 = new Vector3(circleCenterlPoint.x + radius * Mathf.Cos(d2), 0.0f, circleCenterlPoint.z + radius * Mathf.Sin(d2));

            P1 = T1;
        }

        private bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
        {
            Vector3 lineVec3 = linePoint2 - linePoint1;
            Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
            Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

            float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

            //is coplanar, and not parallel
            if (Mathf.Abs(planarFactor) < 0.0001f
                    && crossVec1and2.sqrMagnitude > 0.0001f)
            {
                float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
                intersection = linePoint1 + (lineVec1 * s);
                return true;
            }
            else
            {
                intersection = Vector3.zero;
                return false;
            }
        }

        private float GetSignedAngle(Vector3 vStart, Vector3 vEnd)
        {
            Vector3 v = vEnd - vStart;

            return Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        }

        private Vector3 RotateAroundSpecialAxis(Vector3 position, Vector3 center, Vector3 axis, float angle)
        {
            Vector3 point = Quaternion.AngleAxis(angle, axis) * (position - center);
            Vector3 resultVec3 = center + point;

            return resultVec3;
        }

        public static Vector2 rotateVec2D(Vector2 v, float delta)
        {
            return new Vector2(
                v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
                v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
            );
        }

        private float getStandardDeviation(List<float> floatList)
        {
            float average = floatList.Average();
            float sumOfDerivation = 0;
            foreach (float value in floatList)
            {
                sumOfDerivation += (value) * (value);
            }
            float sumOfDerivationAverage = sumOfDerivation / floatList.Count;
            return Mathf.Sqrt(sumOfDerivationAverage - (average * average));
        }

        private double getStandardDeviation(List<int> floatList)
        {
            double average = floatList.Average();
            int sumOfDerivation = 0;
            foreach (int value in floatList)
            {
                sumOfDerivation += (value) * (value);
            }
            int sumOfDerivationAverage = sumOfDerivation / floatList.Count;
            return Mathf.Sqrt((float)(sumOfDerivationAverage - (average * average)));
        }
    }
}