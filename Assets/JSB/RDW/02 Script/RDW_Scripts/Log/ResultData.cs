using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ResultData
{
    //private int totalReset, wallReset, userReset;
    //private int episodeID, unitID;
    //private float elapsedTime;
    //private float sumOfAppliedTranslationGain, sumOfAppliedRotationGain, sumOfAppliedCurvatureGain;

    private Dictionary<string, float> data;

    public ResultData() {
        data = new Dictionary<string, float>
        {
            {"unitID", 0},
            {"setEpisodeID", 0},
            {"totalTime", 0},
            {"totalReset", 0},
            {"wallReset", 0},
            {"shutterReset", 0},
            {"userReset", 0},
        };
    }

    public void setData(Dictionary<string, float> dict)
    {
        foreach(KeyValuePair<string, float> pair in dict)
        {
            data.Add(pair.Key, pair.Value);
        }
    }

    public void setData(string key, float value)
    {
        if (!data.ContainsKey(key))
            data.Add(key, value);
        else
            data[key] = value;
    }

    public void AddData(string key, float value)
    {
        if (!data.ContainsKey(key))
            data.Add(key, 0);

        data[key] += value;
    }

    public void setEpisodeID(int episodeID)
    {
        data["episodeID"] = episodeID;
    }

    public void setUnitID(int unitID)
    {
        data["unitID"] = unitID;
    }

    public void setGains(GainType gaintype, float appliedGain)
    {
        switch (gaintype)
        {
            case GainType.Translation:
                AddData("sumOfAppliedTranslationGain", appliedGain);
                break;
            case GainType.Rotation:
                AddData("sumOfAppliedRotationGain", appliedGain);
                break;
            case GainType.Curvature:
                AddData("sumOfAppliedCurvatureGain", appliedGain);
                break;
            default:
                break;
        }
    }

    public void AddElapsedTime(float deltaTime)
    {
        data["totalTime"] += deltaTime;
    }

    public void AddWallReset()
    {
        data["wallReset"] += 1;
        data["totalReset"] += 1;
    }

    public void AddShutterReset()
    {
        data["shutterReset"] += 1;
        data["totalReset"] += 1;
    }

    public void AddUserReset()
    {
        data["userReset"] += 1;
        data["totalReset"] += 1;
    }

    public override string ToString()
    {
        string result = "";

        foreach(KeyValuePair<string, float> element in data)
        {
            result += element.Key + ": " + element.Value + "\n";
        }

        //result += "----- Result ----\n";
        //result += string.Format("UnitID: {0}, EpisodeID: {1}\n", unitID, episodeID);
        //result += string.Format("totalReset: {0}, wallReset: {1}, userReset: {2}\n", totalReset, wallReset, userReset);
        //result += string.Format("totalTranslationGain: {0}, totalRotationGain: {1}, totalCurvatureGain: {2}\n", sumOfAppliedTranslationGain, sumOfAppliedRotationGain, sumOfAppliedCurvatureGain);
        //result += string.Format("elapsedTime: {0}\n", elapsedTime);

        return result;
    }

    public float getTotalReset()
    {
        return data["totalReset"];
    }

    public float getWallReset()
    {
        return data["wallReset"];
    }
    public float getShutterReset()
    {
        return data["shutterReset"];
    }

    public float getUserReset()
    {
        return data["userReset"];
    }
}
