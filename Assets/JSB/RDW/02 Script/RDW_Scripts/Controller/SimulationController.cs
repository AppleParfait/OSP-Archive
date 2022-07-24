using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationController
{
    Episode episode;
    float rotationSpeed;
    float translationSpeed;
    float translationSpeedmax;
    float maxRotTime, maxTransTime, remainRotTime, remainTransTime;
    bool isFirst = true, initializing = false, isFirst2 = true, isFirst3 = true;
    float initialAngleDirection = 0.0f;
    Vector2 initialToTarget;
    Vector2 targetPosition;
    Vector2 virtualTargetDirection = Vector2.zero;
    Vector2 virtualTargetPosition;
    bool RLActionReady = false; // Action을 위해 한번 이상의 Fixed Update 필요하기 때문

    [HideInInspector]
    public float deltaRotation;
    [HideInInspector]
    public Vector2 deltaPosition;

    private Vector2 previousPosition;
    private float previousRotation;
    private Vector2 previousForward;

    public SimulationController() { // 기본 생성자
        this.episode = new Episode();
    }

    public SimulationController(Episode episode, float translationSpeed, float rotationSpeed) { // 생성자
        this.episode = episode;
        //this.translationSpeedmax = translationSpeed;
        this.translationSpeed = translationSpeed;
        this.rotationSpeed = rotationSpeed;
        this.deltaPosition = Vector2.zero;
        this.deltaRotation = 0;
    }

    public (Vector2, float) GetDelta(Vector2 directionToTarget)
    {
        Vector2 retTrans = Vector2.zero;
        float retRot = 0;

        if (deltaPosition.magnitude > (translationSpeed - 0.1f))
            retTrans = directionToTarget * translationSpeed;
        else
            retTrans = deltaPosition;

        if (Mathf.Abs(deltaRotation) > (rotationSpeed - 0.1f))
            retRot = Mathf.Sign(deltaRotation) * rotationSpeed;
        else
            retRot = deltaRotation;

        return (retTrans, retRot);
    }

    public int GetEpisodeID()
    {
        return episode.getID();
    }

    public Episode GetEpisode()
    {
        return episode;
    }

    public void UpdateCurrentState(Transform2D virtualUserTransform)
    {
        deltaPosition = (virtualUserTransform.localPosition - previousPosition) / Time.fixedDeltaTime;
        //deltaRotation = (virtualUserTransform.localRotation - previousRotation) / Time.fixedDeltaTime;
        deltaRotation = Vector2.SignedAngle(previousForward, virtualUserTransform.forward) / Time.fixedDeltaTime;
 
        previousPosition = virtualUserTransform.localPosition;
        //previousRotation = virtualUserTransform.localRotation;
        previousForward = virtualUserTransform.forward;
    }

    public void ResetCurrentState(Transform2D virtualUserTransform)
    {
        deltaPosition = Vector2.zero;
        deltaRotation = 0;
        previousPosition = virtualUserTransform.localPosition;
        //previousRotation = virtualUserTransform.localRotation;
        previousForward = virtualUserTransform.forward;
    }

    public void SyncDirection(Object2D virtualUser, Vector2 virtualTargetDirection)
    {
        if (virtualTargetDirection.magnitude > 1)
            virtualTargetDirection = virtualTargetDirection.normalized;

        virtualUser.transform2D.forward = virtualTargetDirection;
        if (virtualUser.gameObject != null) virtualUser.gameObject.transform.forward = Utility.CastVector2Dto3D(virtualTargetDirection);
    }

    public void SyncPosition(Object2D virtualUser, Vector2 virtualTargetPosition)
    {
        virtualUser.transform2D.localPosition = virtualTargetPosition;
        if (virtualUser.gameObject != null) virtualUser.gameObject.transform.localPosition = Utility.CastVector2Dto3D(virtualTargetPosition);
    }

    public void SetRLActionReady(bool RLActionReady)
    {
        this.RLActionReady = RLActionReady;
    }

    public void SetControllerFirstTrue()
    {
        this.isFirst = false;
        this.isFirst2 = false;
        this.isFirst3 = false;
    }

    public void SetControllerFirstFalse()
    {
        this.isFirst = false;
        this.isFirst2 = false;
        this.isFirst3 = false;
    }

    public (Vector2, float) VirtualMove(Object2D virtualUser, Space2D virtualSpace, Object2D realUser = null, Space2D realSpace = null)
    {
        Transform2D virtualUserTransform = virtualUser.transform2D;

        //translationSpeed = UnityEngine.Random.Range(0.1f, translationSpeedmax);

        if (!initializing)
        {
            ResetCurrentState(virtualUserTransform);
            initializing = false;
        }

        if (episode.IsNotEnd())
        {

            if (isFirst && RLActionReady) // 배치 적용 없이 한번 초기화 되면 안바뀌는 문제 해결. 다음 FixedUpdate에서 First를 시키는 방식.
            {   
                // Debug.Log("VirtualMove Initial");
                isFirst = false;
                targetPosition = episode.GetTarget(virtualUserTransform, virtualSpace, virtualUser);
                if(episode.GetWrongEpisode())
                {
                    //episode.SetWrongEpisode(false);
                    //episode.DeleteTarget();
                    SetControllerFirstTrue();
                    //remainRotTime = maxRotTime;
                    //remainTransTime = maxTransTime;
                    return GetDelta(virtualUserTransform.forward);
                }

                initialToTarget = targetPosition - virtualUserTransform.localPosition;
                float InitialAngle = Vector2.SignedAngle(virtualUserTransform.forward, initialToTarget);
                float initialDistance = Vector2.Distance(virtualUserTransform.localPosition, targetPosition);
                initialAngleDirection = Mathf.Sign(InitialAngle);

                virtualTargetDirection = Matrix3x3.CreateRotation(InitialAngle) * virtualUser.transform2D.forward; // target을 향하는 direction(forward)를 구함
                virtualTargetPosition = virtualUser.transform2D.localPosition + virtualTargetDirection * initialDistance; // target에 도달하는 position을 구함

                maxRotTime = Mathf.Abs(InitialAngle) / rotationSpeed;
                maxTransTime = initialDistance / translationSpeed;
                remainRotTime = 0;
                remainTransTime = 0;

                //if (maxTransTime - remainTransTime > 0.06f)
                //{
                //    Debug.Log(maxTransTime - remainTransTime);
                //}

            }
            else if(!RLActionReady)
            {
                RLActionReady = true;
                return GetDelta(virtualUserTransform.forward);
            }

            //if (remainRotTime < maxRotTime)
            //{
            //    virtualUser.transform2D.Rotate(initialAngleDirection * rotationSpeed * Time.fixedDeltaTime);
            //    remainRotTime += Time.fixedDeltaTime;
            //}
            //else if (remainTransTime < maxTransTime)
            //{
            //    if (isFirst2) // 방향을 동기화
            //    {
            //        isFirst2 = false;
            //        SyncDirection(virtualUser, virtualTargetDirection);
            //    }
            //    else
            //    {
            //        virtualUser.transform2D.Translate(virtualUserTransform.forward * translationSpeed * Time.fixedDeltaTime, Space.World);
            //        remainTransTime += Time.fixedDeltaTime;
            //    }
            //}
            //else
            //{
            //    if (isFirst3) // 위치를 동기화
            //    {
            //        isFirst3 = false;
            //        SyncPosition(virtualUser, virtualTargetPosition);
            //    }
            //    else
            //    {
            //        //Debug.Log("Completed!");
            //        episode.DeleteTarget();

            //        isFirst = true;
            //        isFirst2 = true;
            //        isFirst3 = true;
            //    }
            //}
            bool jumpRelocation = false;
            WanderingEpisodeForFixedReset wEpisode1 = null;
            WanderingEpisodeForAnyReset wEpisode2 = null;
            if(episode is WanderingEpisodeForFixedReset)
            {
                wEpisode1 = (WanderingEpisodeForFixedReset) episode;

                if(wEpisode1.syncMode)
                {
                    wEpisode1.syncMode = false;
                    // Debug.Log("realUser localPosition in SyncMode: " + realUser.transform2D.localPosition);
                    // Debug.Log("virtualUser localPosition in SyncMode: " + virtualUser.transform2D.localPosition);
                    Polygon2D realSpaceObject = (Polygon2D) realSpace.spaceObject;
                    Polygon2D virtualSpaceObject = (Polygon2D) virtualSpace.spaceObject;

                    List<Vector2> realSpaceCrossBoundaryPoints = realSpaceObject.GetCrossBoundaryPoints();

                    if(wEpisode1.resetType == "1R")
                    {
                        virtualUser.transform2D.localPosition = wEpisode1.resetPoint + realSpaceCrossBoundaryPoints[0] - realUser.transform2D.localPosition;
                    }
                    else if(wEpisode1.resetType == "1T" || wEpisode1.resetType == "4T")
                    {
                        virtualUser.transform2D.localPosition = wEpisode1.resetPoint + realSpaceCrossBoundaryPoints[1] - realUser.transform2D.localPosition;
                    }
                    else if(wEpisode1.resetType == "1L")
                    {
                        virtualUser.transform2D.localPosition = wEpisode1.resetPoint + realSpaceCrossBoundaryPoints[2] - realUser.transform2D.localPosition;
                    }
                    else if(wEpisode1.resetType == "1B" || wEpisode1.resetType == "4B")
                    {
                        virtualUser.transform2D.localPosition = wEpisode1.resetPoint + realSpaceCrossBoundaryPoints[3] - realUser.transform2D.localPosition;
                    }
                    else if(wEpisode1.resetType == "2R")
                    {
                        virtualUser.transform2D.localPosition = wEpisode1.resetPoint - realSpaceCrossBoundaryPoints[2] + realUser.transform2D.localPosition;
                    }
                    else if(wEpisode1.resetType == "2T" || wEpisode1.resetType == "3T")
                    {
                        virtualUser.transform2D.localPosition = wEpisode1.resetPoint - realSpaceCrossBoundaryPoints[3] + realUser.transform2D.localPosition;
                    }
                    else if(wEpisode1.resetType == "2L")
                    {
                        virtualUser.transform2D.localPosition = wEpisode1.resetPoint - realSpaceCrossBoundaryPoints[0] + realUser.transform2D.localPosition;
                    }
                    else if(wEpisode1.resetType == "2B" || wEpisode1.resetType == "3B")
                    {
                        virtualUser.transform2D.localPosition = wEpisode1.resetPoint - realSpaceCrossBoundaryPoints[1] + realUser.transform2D.localPosition;
                    }

                    UpdateCurrentState(virtualUserTransform);
                    return GetDelta(virtualUserTransform.forward);
                    jumpRelocation = true;
                }
            }
            else if(episode is WanderingEpisodeForAnyReset)
            {
                wEpisode2 = (WanderingEpisodeForAnyReset) episode;
                jumpRelocation = true;
            }

            if (virtualSpace.IsInside(virtualUser, 0.0f) && !virtualSpace.IsPossiblePath(virtualUser.transform2D.localPosition, targetPosition, Space.Self) && !jumpRelocation)
            //if (virtualSpace.IsInside(virtualUser, 0.0f) && !virtualSpace.IsPossiblePath(virtualUser.transform2D.localPosition, targetPosition, Space.Self))
            {
                //Debug.Log("Re-Located");
                //episode.DeleteTarget();

                //WanderingEpisodeForFixedReset wEpisode = (WanderingEpisodeForFixedReset) episode;
                //if(!wEpisode.skipBit)

                episode.ReLocateTarget();

                isFirst = true;
                isFirst2 = true;
                isFirst3 = true;

            }
            else
            {
                if (remainRotTime < maxRotTime)
                {                    
                    virtualUser.transform2D.Rotate(initialAngleDirection * rotationSpeed * Time.fixedDeltaTime);
                    remainRotTime += Time.fixedDeltaTime;
                }
                else if (remainTransTime < maxTransTime)
                {
                    if(remainTransTime > 0.64)
                    {
                        ;
                    }
                    if (isFirst2) // 방향을 동기화
                    {
                        isFirst2 = false;
                        //if(!(this.episode is WanderingEpisodeForFixedReset) && !(this.episode is WanderingEpisodeForAnyReset))
                            //SyncDirection(virtualUser, virtualTargetDirection);
                    }
                    else
                    {
                        virtualUser.transform2D.Translate(virtualUserTransform.forward * translationSpeed * Time.fixedDeltaTime, Space.World);
                        remainTransTime += Time.fixedDeltaTime;
                    }
                }
                else
                {
                    if (isFirst3) // 위치를 동기화
                    {
                        isFirst3 = false;
                        //if(!(this.episode is WanderingEpisodeForFixedReset) && !(this.episode is WanderingEpisodeForAnyReset))
                        //    SyncPosition(virtualUser, virtualTargetPosition);
                    }
                    else
                    {
                        //Debug.Log("Completed!");
                        episode.DeleteTarget();

                        isFirst = true;
                        isFirst2 = true;
                        isFirst3 = true;
                    }
                }
            }

            UpdateCurrentState(virtualUserTransform);
        }


        return GetDelta(virtualUserTransform.forward);
    }

    public (GainType, float) RealMove(Object2D realUser, GainType type, float degree)
    {
        Transform2D realUserTransform = realUser.transform2D;
        float appliedGain = 0;

        switch (type)
        {
            case GainType.Translation:
                realUser.transform2D.Translate(realUserTransform.forward * degree * Time.fixedDeltaTime, Space.World);
                break;
            case GainType.Rotation:
                realUser.transform2D.Rotate(degree * Time.fixedDeltaTime);
                break;
            case GainType.Curvature:
                realUser.transform2D.Translate(realUserTransform.forward * deltaPosition.magnitude * Time.fixedDeltaTime, Space.World);
                realUser.transform2D.Rotate(degree * Time.fixedDeltaTime);
                break;
            default:
                break;
        }

        return (type, appliedGain);
    }

    public void RealMove(Object2D realUser, GainType type, List<float> degree)
    {
        Transform2D realUserTransform = realUser.transform2D;

        switch (type)
        {
            case GainType.Translation:
                realUser.transform2D.Translate(realUserTransform.forward * degree[0] * Time.fixedDeltaTime, Space.World);
                break;
            case GainType.Rotation:
                realUser.transform2D.Rotate(degree[0] * Time.fixedDeltaTime);
                //Debug.Log("a");
                break;
            case GainType.Curvature:
                realUser.transform2D.Rotate(degree[0] * Time.fixedDeltaTime);
                realUser.transform2D.Translate(realUserTransform.forward * degree[1] * Time.fixedDeltaTime, Space.World);

                //Debug.Log("GGG");
                //realUser.transform2D.Translate(realUserTransform.forward * deltaPosition.magnitude * Time.fixedDeltaTime, Space.World);
                //realUser.transform2D.Rotate(degree[0] * Time.fixedDeltaTime);
                //Debug.Log("b  " + degree[1]);
                break;
            default:
                break;
        }
    }
}
