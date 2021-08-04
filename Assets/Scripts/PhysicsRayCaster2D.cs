using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


/// <summary>
/// Class to detect Physics2D collision
/// </summary>
public class PhysicsRayCaster2D 
{
    /// <summary>
    /// Used to find out the hit point information towards an aim direction 
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="direction"></param>
    /// <param name="maxDistance"></param>
    /// <param name="isTophit"></param>
    /// <param name="hitInfo"></param>
    /// <param name="segmentPoints"></param>
    /// <param name="reflectLayer"></param>
    /// <param name="targetLayer"></param>
    /// <param name="topLayer"></param>
    /// <returns></returns>
    public static bool AimRayCast2D(Vector2 origin , Vector2 direction, float maxDistance , out bool isTophit ,out RaycastHit2D hitInfo, out List<Vector2> segmentPoints ,  int reflectLayer , int targetLayer, int topLayer)
    {

        hitInfo = new RaycastHit2D();
        segmentPoints = new List<Vector2>();
        isTophit = false;
        segmentPoints.Add(origin);

        int bubbleLayer = targetLayer | reflectLayer;

        if (RayCastWithTarget(origin,direction,maxDistance, bubbleLayer,  ref hitInfo,ref segmentPoints))
        {
            return true;
        }
        else if(RayCastWithReflectLayer(origin,direction,maxDistance,reflectLayer,out Vector2 reflectDir,ref hitInfo,ref segmentPoints))
        {
            if (RayCastWithTarget(hitInfo.point, reflectDir, maxDistance, targetLayer,  ref hitInfo, ref segmentPoints))
            {
                return true;
            }
            if (RayCastWithTopLayer(hitInfo.point, reflectDir, maxDistance, topLayer, ref hitInfo, ref segmentPoints , ref isTophit))
            {
                return true;
            }
        }
        else
        {
            if (RayCastWithTopLayer(origin, direction, maxDistance, topLayer, ref hitInfo, ref segmentPoints, ref isTophit))
            {
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// Raycast to find target by specifying the layer while aiming
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="direction"></param>
    /// <param name="maxDistance"></param>
    /// <param name="targetLayer"></param>
    /// <param name="hitInfo"></param>
    /// <param name="segmentPoints"></param>
    /// <returns></returns>
    static bool RayCastWithTarget(Vector2 origin, Vector2 direction, float maxDistance, int targetLayer, ref RaycastHit2D hitInfo, ref List<Vector2> segmentPoints)
    {
        RaycastHit2D targetHit = Physics2D.Raycast(origin, direction, maxDistance, targetLayer);
        if (targetHit.collider != null && (targetHit.collider.tag != "wall"))
        {
            
            segmentPoints.Add(targetHit.point);
            hitInfo = targetHit;
            return true;
        }
        return false;
    }
    /// <summary>
    /// Raycast from a vertical surface to  reflected direction while aiming
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="direction"></param>
    /// <param name="maxDistance"></param>
    /// <param name="reflectLayer"></param>
    /// <param name="reflectDir"></param>
    /// <param name="hitInfo"></param>
    /// <param name="segmentPoints"></param>
    /// <returns></returns>
    static bool RayCastWithReflectLayer(Vector2 origin, Vector2 direction, float maxDistance, int reflectLayer,out Vector2 reflectDir , ref RaycastHit2D hitInfo, ref List<Vector2> segmentPoints)
    {
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxDistance, reflectLayer);
        reflectDir = Vector2.zero;
        if (hit.collider != null)
        {
             reflectDir = Vector2.Reflect(direction, Vector2.right);
             segmentPoints.Add(hit.point);
            hitInfo = hit;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Raycast to find the top postion of the bubbles while aiming
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="direction"></param>
    /// <param name="maxDistance"></param>
    /// <param name="topLayer"></param>
    /// <param name="hitInfo"></param>
    /// <param name="segmentPoints"></param>
    /// <param name="isTopHit"></param>
    /// <returns></returns>
    static bool RayCastWithTopLayer(Vector2 origin, Vector2 direction, float maxDistance, int topLayer, ref RaycastHit2D hitInfo, ref List<Vector2> segmentPoints , ref bool isTopHit)
    {
        
        RaycastHit2D topHit = Physics2D.Raycast(origin, direction, maxDistance, topLayer);
        if (topHit.collider != null)
        {
            float minDist = Mathf.Infinity;
            Vector2 point = Vector2.zero;
            foreach (Vector2 topPoints in BubbleFactory.Instance.TopRowPostions)
            {
                float dist = Vector2.Distance(topPoints, topHit.point);
                if (dist < minDist)
                {
                    point = topPoints;
                    minDist = dist;
                }
            }
            isTopHit = true;
            hitInfo = topHit;
            segmentPoints.Add(point);
            return true;
        }
        return false;
    }

}
