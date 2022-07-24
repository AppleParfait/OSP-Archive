using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterTurnResetter : RotationResetter
{
    public CenterTurnResetter() : base()
    {
    }

    public CenterTurnResetter(float translationSpeed, float rotationSpeed) : base(translationSpeed, rotationSpeed)
    {
    }

    public override string ApplyWallReset(Object2D realUser, Object2D virtualUser, Space2D realSpace)
    {
        if (isFirst)
        {
            Vector3 targetCenterPoint = Vector3.zero;



            //Vector2 userToCenter = realSpace.spaceObject.transform2D.localPosition - realUser.transform2D.localPosition;


            for (int i = 0; i < RDWSimulationManager.instance.GetRedirectedUnits.Length; i++)
            {
                if (realUser.gameObject == GameObject.FindWithTag("RealUser"+i))
                {
                    if(RDWSimulationManager.instance.GetRedirectedUnits[i].GetRedirector() is S2CRedirector)
                    {
                        targetCenterPoint = ((S2CRedirector)RDWSimulationManager.instance.GetRedirectedUnits[i].GetRedirector()).GetCenterPoint();
                    }
                    else
                    {
                        targetCenterPoint = Vector3.zero;
                    }


                    break;
                }
            }
            //if (realUser.gameObject == GameObject.FindWithTag("RealUser0"))
            //{
            //    targetCenterPoint = ((S2CRedirector)RDWSimulationManager.instance.GetRedirectedUnits[0].GetRedirector()).GetCenterPoint();
            //}
            //else if (realUser.gameObject == GameObject.FindWithTag("RealUser1"))
            //{
            //    targetCenterPoint = ((S2CRedirector)RDWSimulationManager.instance.GetRedirectedUnits[1].GetRedirector()).GetCenterPoint();
            //}

            Vector2 userToCenter = new Vector2(targetCenterPoint.x, targetCenterPoint.z) - realUser.transform2D.localPosition;

            targetAngle = Vector2.SignedAngle(realUser.transform2D.forward, userToCenter);

            realTargetRotation = Utility.RotateVector2(realUser.transform2D.forward, targetAngle);
            virtualTargetRotation = Utility.RotateVector2(virtualUser.transform2D.forward, 360);

            //realTargetRotation = Matrix3x3.CreateRotation(targetAngle) * realUser.transform2D.forward;
            //virtualTargetRotation = Matrix3x3.CreateRotation(360) * virtualUser.transform2D.forward;
            isFirst = false;

            maxRotTime = Mathf.Abs(targetAngle) / rotationSpeed;
            remainRotTime = 0;
        }
        //else
        //{
        //    Vector3 targetCenterPoint = Vector3.zero;


        //    if (realUser.gameObject == GameObject.FindWithTag("RealUser0"))
        //    {
        //        targetCenterPoint = ((S2CRedirector)RDWSimulationManager.instance.GetRedirectedUnits[0].GetRedirector()).GetCenterPoint();
        //    }
        //    else if (realUser.gameObject == GameObject.FindWithTag("RealUser1"))
        //    {
        //        targetCenterPoint = ((S2CRedirector)RDWSimulationManager.instance.GetRedirectedUnits[1].GetRedirector()).GetCenterPoint();
        //    }

        //    Vector2 userToCenter = new Vector2(targetCenterPoint.x, targetCenterPoint.z) - realUser.transform2D.localPosition;

        //    targetAngle = Vector2.SignedAngle(realUser.transform2D.forward, userToCenter);

        //    realTargetRotation = Utility.RotateVector2(realUser.transform2D.forward, targetAngle);
        //    virtualTargetRotation = Utility.RotateVector2(virtualUser.transform2D.forward, 360);
        //}

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
}
