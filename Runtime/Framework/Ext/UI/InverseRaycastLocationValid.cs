using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InverseRaycastLocationValid : MonoBehaviour, ICanvasRaycastFilter
{
    public GameObject[] InvalidAreas;

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        if (InvalidAreas != null)
        {
            foreach (var area in InvalidAreas)
            {
                if (area != null)
                {
                    var rtrans = area.transform as RectTransform;
                    if (rtrans != null)
                    {
                        if (RectTransformUtility.RectangleContainsScreenPoint(rtrans, sp, eventCamera))
                        {
                            var comps = area.GetComponents<Component>();
                            if (comps != null)
                            {
                                foreach (var comp in comps)
                                {
                                    var filter = comp as ICanvasRaycastFilter;
                                    if (filter != null)
                                    {
                                        if (filter.IsRaycastLocationValid(sp, eventCamera))
                                        {
                                            return false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        return true;
    }
}
