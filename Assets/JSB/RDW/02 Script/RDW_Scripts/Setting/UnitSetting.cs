using System;
using UnityEngine;

public enum RedirectType { Null, Default, S2C, APF, Space, Arrangement,
    APF_OSP, ARC
}
public enum ResetType { Default, TwoOneTurn, APF_R_Turn, FreezeTurn, CenterTurn,
    APF_R_Turn_OSP, ARC_Turn
}
public enum EpisodeType { LongWalk, Random, PreDefined, WanderingEpisodeForFixedReset, WanderingEpisodeForAnyReset };

[System.Serializable]
public class UnitSetting
{
    public RedirectType redirectType;
    public ResetType resetType;
    public EpisodeType episodeType;
    public int episodeLength;
    public string episodeFileName;

    public bool useRandomStartReal;
    public bool useRandomStartVirtual;
    public GameObject userPrefab;
    public float userStartRotation;
    public float realStartRotation;
    public float virtualStartRotation;
    public Vector2 realStartPosition;
    public Vector2 virtualStartPosition;
    public float translationSpeed;
    public float rotationSpeed;



    public RedirectedUnit GetUnit(Space2D realSpace, Space2D virtualSpace, int _idex)
    {
        Object2D realUser, virtualUser;

        if (useRandomStartReal)
        {
            //realStartPosition = realSpace.GetRandomPoint(0.2f);
            //realStartPosition = _ranSampledPos;
            realStartRotation = Utility.sampleUniform(0f, 360f);
        }

        if (useRandomStartVirtual)
        {
            virtualStartPosition = virtualSpace.GetRandomPoint(10.0f);
            //virtualStartRotation = Utility.sampleUniform(0f, 360f);
            virtualStartRotation = realStartRotation;
        }

        if (userPrefab != null)
        {
            switch (userPrefab.tag) // 좀더 깔끔한 코드 있을 꺼 같은데 (추상화 가능성)
            {
                default:
                    realUser = new Polygon2DBuilder().SetPrefab(userPrefab).SetLocalPosition(realStartPosition).SetLocalRotation(realStartRotation).SetParent(realSpace.spaceObject).Build();
                    
                    if(virtualSpace.tileMode)
                        virtualUser = new Polygon2DBuilder().SetPrefab(userPrefab).SetLocalPosition(virtualStartPosition).SetLocalRotation(virtualStartRotation).SetParent(virtualSpace.parentSpaceObject).Build();
                    else
                        virtualUser = new Polygon2DBuilder().SetPrefab(userPrefab).SetLocalPosition(virtualStartPosition).SetLocalRotation(virtualStartRotation).SetParent(virtualSpace.spaceObject).Build();

                    break;
                case "Circle":
                    realUser = new Circle2DBuilder().SetPrefab(userPrefab).SetLocalPosition(realStartPosition).SetLocalRotation(realStartRotation).SetParent(realSpace.spaceObject).Build();
                    realUser.gameObject.tag = "RealUser" + _idex;
                    realUser.gameObject.layer = LayerMask.NameToLayer("PhysicalUser");

                    if (virtualSpace.tileMode)
                        virtualUser = new Circle2DBuilder().SetPrefab(userPrefab).SetLocalPosition(virtualStartPosition).SetLocalRotation(virtualStartRotation).SetParent(virtualSpace.parentSpaceObject).Build();
                    else
                        virtualUser = new Circle2DBuilder().SetPrefab(userPrefab).SetLocalPosition(virtualStartPosition).SetLocalRotation(virtualStartRotation).SetParent(virtualSpace.spaceObject).Build();
                    virtualUser.gameObject.tag = "VirtualUser" + _idex;
                    break;
            }
        }
        else
        {
            realUser = new Circle2DBuilder().SetLocalPosition(realStartPosition).SetLocalRotation(realStartRotation).SetRadius(0.5f).SetParent(realSpace.spaceObject).Build();

            if(virtualSpace.tileMode)
                virtualUser = new Circle2DBuilder().SetLocalPosition(virtualStartPosition).SetLocalRotation(virtualStartRotation).SetRadius(0.5f).SetParent(virtualSpace.parentSpaceObject).Build();
            else
                virtualUser = new Circle2DBuilder().SetLocalPosition(virtualStartPosition).SetLocalRotation(virtualStartRotation).SetRadius(0.5f).SetParent(virtualSpace.spaceObject).Build();
        }

        return new RedirectedUnitBuilder()
            .SetController(GetController())
            .SetRedirector(GetRedirector())
            .SetResetter(GetRestter())
            .SetRealSpace(realSpace)
            .SetVirtualSpace(virtualSpace)
            .SetRealUser(realUser)
            .SetVirtualUser(virtualUser)
            .Build();
    }

    public SimulationController GetController()
    {
        return new SimulationController(GetEpisode(), translationSpeed, rotationSpeed);
    }

    public Redirector GetRedirector()
    {
        Redirector redirector;

        switch (redirectType)
        {
            case RedirectType.S2C:
                S2CRedirector s2c = new S2CRedirector();
                s2c.SetCenterPoint(realStartPosition);
                redirector = s2c;
                break;
            case RedirectType.APF:
                redirector = new APFRedirector();
                break;
            case RedirectType.APF_OSP:
                redirector = new APFRedirector_OSP();
                break;
            case RedirectType.ARC:
                redirector = new ARCRedirector();
                break;
            case RedirectType.Null:
                redirector = new NullRedirector();
                break;
            default:
                redirector = new Redirector();
                break;
        }

        return redirector;
    }

    public Resetter GetRestter()
    {
        Resetter resetter;

        switch (resetType)
        {
            case ResetType.TwoOneTurn:
                resetter = new TwoOneTurnResetter(translationSpeed, rotationSpeed);
                break;
            case ResetType.FreezeTurn:
                resetter = new FreezeTurnResetter(translationSpeed, rotationSpeed);
                break;
            case ResetType.APF_R_Turn:
                resetter = new APF_R_Resetter(translationSpeed, rotationSpeed);
                break;
            case ResetType.APF_R_Turn_OSP:
                resetter = new APF_R_Resetter_OSP(translationSpeed, rotationSpeed);
                break;
            case ResetType.CenterTurn:
                resetter = new CenterTurnResetter(translationSpeed, rotationSpeed);
                break;
            case ResetType.ARC_Turn:
                resetter = new ARC_Resetter(translationSpeed, rotationSpeed);
                break;
            default:
                resetter = new Resetter(translationSpeed, rotationSpeed);
                break;
        }

        return resetter;
    }

    public Episode GetEpisode()
    {
        Episode episode;

        switch (episodeType)
        {
            case EpisodeType.LongWalk:
                episode = new LongWalkEpisode(episodeLength);
                break;
            case EpisodeType.Random:
                episode = new RandomEpisode(episodeLength);
                break;
            case EpisodeType.PreDefined:
                episode = new PreDefinedEpisode(episodeFileName);
                break;
            case EpisodeType.WanderingEpisodeForFixedReset:
                episode = new WanderingEpisodeForFixedReset(episodeLength);
                break;
            case EpisodeType.WanderingEpisodeForAnyReset:
                episode = new WanderingEpisodeForAnyReset(episodeLength);
                break;
            default:
                episode = new Episode(episodeLength);
                break;
        }

        return episode;
    }
}
