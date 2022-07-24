using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomEpisode : Episode
{
    public RandomEpisode() : base() { }

    public RandomEpisode(int episodeLength) : base(episodeLength) { }

    bool bfirst = true;
    int count = 0;



    private int targetCount = 0;
    private int targetCount_max = 3;
    int currentPathPatternType = 0;

    protected override void GenerateEpisode(Transform2D virtualUserTransform, Space2D virtualSpace, Object2D virtualUser)
    {
        Vector2 samplingPosition = Vector2.zero;
        Vector2 userPosition = virtualUserTransform.localPosition;




        do
        {
            //float angle = Utility.sampleUniform(-180.0f, 180.0f);

            float angle = 0.0f;
            //if (bfirst)
            //{
            //    bfirst = false;
            //    Debug.LogError("bfirst");
            //}
            //else
            //{
                angle = List_DiscreteAngle[Random.Range(0, 36)];
            //}

            //Debug.LogWarning(angle);
            //float distance = Utility.sampleUniform(2.0f, 5.0f);
            float distance = 0.0f;

            //if (virtualUser.gameObject == GameObject.FindWithTag("VirtualUser0"))
            //{
            //    if (count < 10)
            //    {
            //        count++;
            //        distance = 0.0f;
            //        break;
            //    }
            //    else
            //    {
            //        count = 0;
            //    }

            //    //distance = Utility.sampleUniform(0.1f, 5.0f);

            //    /// high frequency Short Range
            //    float temp = 0.0f;
            //    //while (temp < 5.0)
            //    {
            //        temp = Utility.sampleNormal(0.0f, 1.0f, -4.9f, 4.9f);
            //        //temp = RandomGaussian(0.1f, 9.9f);
            //    }

            //    //distance = temp - 4.9f;
            //    distance = Mathf.Abs(temp) + 0.1f;
            //    //Debug.Log(distance.ToString("F3"));

            //}
            //else if (virtualUser.gameObject == GameObject.FindWithTag("VirtualUser1"))
            //{
            //    //distance = Utility.sampleUniform(0.1f, 5.0f);



            //    /// high frequency Long Range
            //    float temp = 10.0f;
            //    //while (temp > 5.0)
            //    {
            //        temp = Utility.sampleNormal(0.0f, 0.5f, -4.9f, 4.9f);
            //        //temp = RandomGaussian(0.1f, 9.9f);
            //    }

            //    distance = Mathf.Abs(Mathf.Abs(temp) - 5.0f);
            //    //Debug.Log(distance.ToString("F3"));
            //}

            ////else
            //{
            //distance = Utility.sampleUniform(0.1f, 5.0f);
            //}


            //distance = Utility.sampleUniform(0.2f, 3.0f);
            distance = Utility.sampleUniform(0.2f, 3.0f);

            ///adfadf
            //if (Random.Range(1, 3) % 2 == 0)
            //{
            //    distance = Utility.sampleUniform(0.2f, 3.0f);
            //}
            //else
            //{
            //    distance = Utility.sampleUniform(1.0f, 3.0f);
            //}








            /// Curriculum learning 
            if (_OSP.OSP_Agent.instance.bMixedExploration)
            {
                int pathPatternturningPoint_min = 3;
                int pathPatternturningPoint_max = 5;
                int pathPatternType_max = 1;

                switch (_OSP.OSP_Agent.instance.currnet_CurriculumState)
                {
                    case _OSP.Enum_CurriculumState._2ndQuater:
                        pathPatternType_max = 2;
                        break;

                    case _OSP.Enum_CurriculumState._3rdQuater:
                        pathPatternType_max = 3;
                        break;

                    case _OSP.Enum_CurriculumState._4thQuater:
                        pathPatternType_max = 3;
                        pathPatternturningPoint_min = 2;
                        pathPatternturningPoint_max = 6;
                        break;
                }

                if (targetCount == targetCount_max)
                {
                    targetCount = 0;
                    targetCount_max = Random.Range(pathPatternturningPoint_min, pathPatternturningPoint_max);
                    currentPathPatternType = Random.Range(0, pathPatternType_max);
                    //Debug.Log("currentPathPatternType " + currentPathPatternType);
                    //Debug.Log("currnet_CurriculumState " + _OSP.OSP_Agent.instance.currnet_CurriculumState);
                }
                else
                {
                    targetCount++;
                }

                if (currentPathPatternType == 0)
                {
                    distance = Utility.sampleUniform(1.0f, 3.0f);
                    //Debug.Log("LE");
                }
                else if (currentPathPatternType == 1)
                {
                    distance = Utility.sampleUniform(0.2f, 1.0f);
                    //Debug.Log("SE");
                }
                else if (currentPathPatternType == 2)
                {
                    distance = Utility.sampleUniform(0.001f, 0.002f);
                    //Debug.Log("IE");
                }
            }
            else
            {
                distance = Utility.sampleUniform(1.0f, 3.0f);
                //Debug.Log("LE");
            }




            Vector2 sampleForward = Utility.RotateVector2(virtualUserTransform.forward, angle);

            samplingPosition = userPosition + sampleForward * distance; // local 좌표계에서 절대 위치 기준

        } while (!virtualSpace.IsInside(samplingPosition, Space.Self, 0.5f).Item1); // !virtualSpace.IsPossiblePath(samplingPosition, userPosition, Space.Self, 0.2f)

        currentTargetPosition = samplingPosition;
    }

    public static float RandomGaussian(float minValue = 0.0f, float maxValue = 1.0f)
    {
        float u, v, S;

        do
        {
            u = 2.0f * UnityEngine.Random.value - 1.0f;
            v = 2.0f * UnityEngine.Random.value - 1.0f;
            S = u * u + v * v;
        }
        while (S >= 1.0f);

        // Standard Normal Distribution
        float std = u * Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);

        // Normal Distribution centered between the min and max value
        // and clamped following the "three-sigma rule"
        float mean = (minValue + maxValue) / 2.0f;
        float sigma = (maxValue - mean) / 3.0f;
        return Mathf.Clamp(std * sigma + mean, minValue, maxValue);
    }
}
