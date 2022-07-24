using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Episode
{
    protected static int totalID = 0;
    protected int id;
    protected Vector2? currentTargetPosition;
    protected int currentEpisodeIndex;
    protected int episodeLength;
    public GameObject targetPrefab = null;
    protected GameObject targetObject = null;
    public bool showTarget = false;
    private bool wrongEpisode = false;

    protected Vector2 realAgentInitialPosition;
    protected Vector2 virtualAgentInitialPosition;

    public List<float> List_DiscreteAngle = new List<float>();

    public Vector2 GetRealAgentInitialPosition()
    {
        return realAgentInitialPosition;
    }
    
    public void SetRealAgentInitialPosition(Vector2 realAgentInitialPosition)
    {
        this.realAgentInitialPosition = realAgentInitialPosition;
    }

    public Vector2 GetVirtualAgentInitialPosition()
    {
        return virtualAgentInitialPosition;
    }

    public void SetVirtualAgentInitialPosition(Vector2 virtualAgentInitialPosition)
    {
        this.virtualAgentInitialPosition = virtualAgentInitialPosition;
    }

    public void setShowTarget(bool showTarget)
    {
        this.showTarget = showTarget;
    }

    public bool GetWrongEpisode()
    {
        return wrongEpisode;
    }

    public void SetWrongEpisode(bool wrongEpisode)
    {
        this.wrongEpisode = wrongEpisode;
    }

    public Episode() { // 기본 생성자
        id = totalID++;
        currentEpisodeIndex = 0;
        currentTargetPosition = null;
        this.episodeLength = 0;

        List_DiscreteAngle.Clear();
        for (int i = 0; i < 36; i++)
        {
            List_DiscreteAngle.Add(-180.0f + (i * 10));
        }
    }

    public Episode(int episodeLength) // 생성자
    {
        id = totalID++;
        currentEpisodeIndex = 0;
        currentTargetPosition = null;
        this.episodeLength = episodeLength;

        List_DiscreteAngle.Clear();
        for (int i = 0; i < 36; i++)
        {
            List_DiscreteAngle.Add(-180.0f + (i * 10));
        }

    }

    public void ResetEpisode()
    {
        id = totalID++;
        currentEpisodeIndex = 0;
        currentTargetPosition = null;

        List_DiscreteAngle.Clear();
        for (int i = 0; i < 36; i++)
        {
            List_DiscreteAngle.Add(-180.0f + (i * 10));
        }
    }

    public int GetCurrentEpisodeIndex()
    {
        return currentEpisodeIndex;
    }

    public void SetCurrentEpisodeIndex(int currentEpisodeIndex)
    {
        this.currentEpisodeIndex = currentEpisodeIndex;
    }

    public int GetEpisodeLength()
    {
        return episodeLength;
    }

    public int getID()
    {
        return id;
    }

    protected void InstaniateTarget()
    {
        targetObject = GameObject.Instantiate(targetPrefab, Vector3.zero, Quaternion.identity, GameObject.Find("Virtual Space").transform);
        targetObject.transform.localPosition = Utility.CastVector2Dto3D(currentTargetPosition.Value) + new Vector3(0, 1.35f, 0);
    }

    protected void InstaniateTarget(Vector2 manualTargetPosition)
    {
        targetObject = GameObject.Instantiate(targetPrefab, Vector3.zero, Quaternion.identity, GameObject.Find("Virtual Space").transform);
        targetObject.transform.localPosition = Utility.CastVector2Dto3D(manualTargetPosition) + new Vector3(0, 1.35f, 0);
    }

    public bool IsNotEnd()
    {
        if (currentEpisodeIndex < episodeLength)
            return true;
        else
            return false;
    }

    public void DeleteTarget()
    {
        GameObject.Destroy(targetObject);
        currentEpisodeIndex += 1;
        currentTargetPosition = null;
    }

    public void ReLocateTarget()
    {
        GameObject.Destroy(targetObject);
        currentTargetPosition = null;
    }

    public virtual Vector2 GetTarget(Transform2D virtualUserTransform, Space2D virtualSpace, Object2D virtualUser)
    {
        if (!currentTargetPosition.HasValue)
        {
            GenerateEpisode(virtualUserTransform, virtualSpace, virtualUser);
            if(targetPrefab != null && showTarget) InstaniateTarget();
        }

        return currentTargetPosition.Value;
    }

    protected virtual void GenerateEpisode(Transform2D virtualUserTransform, Space2D virtualSpace, Object2D virtualUser) { }
}
