using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongWalkEpisode : Episode
{
    public LongWalkEpisode() : base() { }

    public LongWalkEpisode(int episodeLength) : base(episodeLength) { }

    protected override void GenerateEpisode(Transform2D virtualUserTransform, Space2D virtualSpace, Object2D virtualUser)
    {
        float angle = 0;
        float distance = 12.0f;

        Vector2 sampleForward = Utility.RotateVector2(virtualUserTransform.forward, angle);
        Vector2 userPosition = virtualUserTransform.localPosition;

        currentTargetPosition = userPosition + sampleForward * distance;
    }
}
