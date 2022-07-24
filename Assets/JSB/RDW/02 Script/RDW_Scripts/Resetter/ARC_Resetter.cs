using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;

public class ARC_Resetter : RotationResetter
{
    private List<float> List_ActualDistance3Way = new List<float>();
    private List<float> List_VirtualDistance3Way = new List<float>();

    public List<float> List_ActualDistance20Way = new List<float>();
    public List<float> List_VirtualDistance20Way = new List<float>();

    public ARC_Resetter() : base()
    {
    }

    public ARC_Resetter(float translationSpeed, float rotationSpeed) : base(translationSpeed, rotationSpeed)
    {
    }

    public override string ApplyWallReset(Object2D realUser, Object2D virtualUser, Space2D realSpace)
    {
        if (isFirst)
        {
            Vector3 targetCenterPoint = Vector3.zero;

            Calc_NWay_Distances(realUser.transform2D.transform, true, 20);
            //Calc_NWay_Distances(virtualUser.transform2D.transform, false, 20);

            // Find obstacle normal in physical squared env
            Vector3 wallnormalvec = Vector3.right;
            RaycastHit hit;
            List<Vector3> dir4way = new List<Vector3>();
            List<float> dist4way = new List<float>();
            dir4way.Add(Vector3.forward);
            dir4way.Add(Vector3.right);
            dir4way.Add(-Vector3.forward);
            dir4way.Add(-Vector3.right);

            for (int i = 0; i < 4; i++)
            {
                if (Physics.Raycast(realUser.transform2D.transform.position, dir4way[i], out hit, 1000.0f))
                {
                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("PhysicalWall") || hit.collider.gameObject.layer == LayerMask.NameToLayer("PhysicalUser"))
                    {
                        dist4way.Add(hit.distance);
                    }
                    else if (hit.collider.gameObject.name == "obstacle_" + i)
                    {
                        dist4way.Add(hit.distance);
                    }
                }
            }
            int tempindex = dist4way.IndexOf(dist4way.Min());
            wallnormalvec = -dir4way[tempindex];



            // Find values that satisfy condi 1
            List<int> list_dir_FirstConditionSatisfied = new List<int>();
            for (int i = 0; i < 20; i++)
            {
                float value = 0.0f;
                Vector3 dir1 = Quaternion.AngleAxis(18 * i, Vector3.up) * Vector3.forward;
                //value = Vector3.Dot(wallnormalvec, dir1);
                value = Mathf.Abs(Vector3.Angle(wallnormalvec, dir1));

                //if (value > 0.0f)
                if (value < 80.0f)
                {
                    list_dir_FirstConditionSatisfied.Add(i);
                }
            }

            // Find values that satisfy condi 1 && condi 2
            float virtualDist = 0.0f;
            if (Physics.Raycast(virtualUser.transform2D.transform.position, virtualUser.transform2D.transform.forward, out hit, 1000.0f))
            {
                Debug.DrawLine(virtualUser.transform2D.transform.position, virtualUser.transform2D.transform.position + virtualUser.transform2D.transform.forward * 1000.0f, Color.blue, 10.0f);
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("VirtualWall"))
                {
                    virtualDist = hit.distance;
                }
            }

            List<int> list_dir_SecondConditionSatisfied = new List<int>();
            List<float> list_dir_SecondConditionSatisfied_distance = new List<float>();
            for (int i = 0; i < list_dir_FirstConditionSatisfied.Count; i++)
            {
                float diff = List_ActualDistance20Way[list_dir_FirstConditionSatisfied[i]] - virtualDist;
                
                if (diff >= 0.0f)
                {
                    list_dir_SecondConditionSatisfied.Add(list_dir_FirstConditionSatisfied[i]);
                    list_dir_SecondConditionSatisfied_distance.Add(diff);
                }
            }


            // condi 1 && condi 2
            int dirIndex = 0;

            if (list_dir_SecondConditionSatisfied.Count > 0)
            {
                dirIndex = list_dir_SecondConditionSatisfied[list_dir_SecondConditionSatisfied_distance.IndexOf(list_dir_SecondConditionSatisfied_distance.Min())];
            }
            else
            {
                List<float> list_dist_ABS_FirstConditionSatisfied = new List<float>();
                for (int i = 0; i < list_dir_FirstConditionSatisfied.Count; i++)
                {
                    list_dist_ABS_FirstConditionSatisfied.Add(Mathf.Abs(List_ActualDistance20Way[list_dir_FirstConditionSatisfied[i]] - virtualDist));
                }

                dirIndex = list_dir_FirstConditionSatisfied[list_dist_ABS_FirstConditionSatisfied.IndexOf(list_dist_ABS_FirstConditionSatisfied.Min())];

            }
            

            Vector3 temp = Quaternion.AngleAxis(18 * dirIndex, Vector3.up) * Vector3.forward;

            targetAngle = Vector2.SignedAngle(realUser.transform2D.forward, new Vector2(temp.x, temp.z));

            realTargetRotation = Utility.RotateVector2(realUser.transform2D.forward, targetAngle);
            virtualTargetRotation = Utility.RotateVector2(virtualUser.transform2D.forward, 360);

            //realTargetRotation = Matrix3x3.CreateRotation(targetAngle) * realUser.transform2D.forward;
            //virtualTargetRotation = Matrix3x3.CreateRotation(360) * virtualUser.transform2D.forward;
            isFirst = false;

            maxRotTime = Mathf.Abs(targetAngle) / rotationSpeed;
            remainRotTime = 0;
        }
      
        if (remainRotTime < maxRotTime)
        {
            realUser.transform2D.Rotate(Mathf.Sign(targetAngle) * rotationSpeed * Time.deltaTime);
            virtualUser.transform2D.Rotate((360 / maxRotTime) * Time.deltaTime);
            remainRotTime += Time.fixedDeltaTime;
        }
        else
        {
            Utility.SyncDirection(virtualUser, realUser, virtualTargetRotation, realTargetRotation);
            //realUser.transform2D.localPosition = realUser.transform2D.localPosition + realUser.transform2D.forward * Random.Range(0.1f, translationSpeed) * Time.fixedDeltaTime;
            realUser.transform2D.localPosition = realUser.transform2D.localPosition + realUser.transform2D.forward * translationSpeed * Time.fixedDeltaTime;

            isFirst = true;
            return "WALL_RESET_DONE";
        }

        return "IDLE";
    }


    public void Calc_NWay_Distances(Transform _transform, bool bActual, int N_waycount)
    {
        float distance = 0.0f;
        RaycastHit hit;
        List<Vector3> direction = new List<Vector3>();

        int totalUserCount = RDWSimulationManager.instance.GetRedirectedUnits.Length;



        if (N_waycount == 3)
        {
            direction.Add(Vector3.forward);
            direction.Add(Vector3.right);
            direction.Add(-Vector3.right);

            if (bActual)
            {
                List_ActualDistance3Way.Clear();
            }
            else
            {
                List_VirtualDistance3Way.Clear();
            }
        }
        else if (N_waycount == 20)
        {
            for (int i = 0; i < 20; i++)
            {
                Vector3 result = Quaternion.AngleAxis(18 * i, Vector3.up) * Vector3.forward;
                direction.Add(result);
            }


            if (bActual)
            {
                List_ActualDistance20Way.Clear();
            }
            else
            {
                List_VirtualDistance20Way.Clear();
            }
        }

        for (int i = 0; i < direction.Count; i++)
        {
            if (Physics.Raycast(_transform.position, direction[i], out hit, 1000.0f))
            {
                if (bActual)
                {
                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("PhysicalWall") || hit.collider.gameObject.layer == LayerMask.NameToLayer("PhysicalUser"))
                    {
                        distance = hit.distance;
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
                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("VirtualWall"))
                    {
                        distance = hit.distance;
                        //Debug.DrawLine(_transform.position, _transform.position + direction[i] * 10, Color.red, 10.0f);
                    }
                    else
                    {
                        //Debug.Log(hit.transform.gameObject.name);
                        //Debug.DrawLine(_transform.position, _transform.position + direction[i] * 10, Color.blue, 10.0f);
                    }
                    //else
                    //{
                    //    for (int j = 0; j < totalUserCount; j++)
                    //    {
                    //        if (hit.collider.gameObject.layer == LayerMask.NameToLayer("VirtualUser" + j))
                    //        {
                    //            distance = hit.distance;
                    //            break;
                    //        }
                    //    }
                    //}
                }

            }

            if (N_waycount == 3)
            {
                if (bActual)
                {
                    List_ActualDistance3Way.Add(distance);
                }
                else
                {
                    List_VirtualDistance3Way.Add(distance);
                }
            }
            else if (N_waycount == 20)
            {
                if (bActual)
                {
                    List_ActualDistance20Way.Add(distance);
                }
                else
                {
                    List_VirtualDistance20Way.Add(distance);

                }
            }



            distance = 0.0f;
        }

        //if (!bActual)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    foreach (var item in List_VirtualDistance20Way)
        //    {
        //        sb.Append(item.ToString() + ",");
        //    }

        //    Debug.Log(sb.ToString());
        //}

    }
}
