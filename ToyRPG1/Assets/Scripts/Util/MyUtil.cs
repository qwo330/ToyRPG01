using System.Collections.Generic;
using UnityEngine;
using System;

public static class MyUtil
{
    public static T FindbyConditions<T>(this List<T> list, Func<T, bool> conditional)
    {
        int listCount = list.Count;
        for (int i = 0; i < listCount; ++i)
        {
            if (conditional(list[i]) == true)
                return list[i];
        }

        return default;
    }

     
}

public static class WaitTimeChache
{
    public static readonly WaitForEndOfFrame EndFrame = new WaitForEndOfFrame();

    static readonly WaitForSeconds tick = new WaitForSeconds(0.01f);
    public static IEnumerable<WaitForSeconds> WaitForSeconds(float seconds)
    {
        int loopCount = (int)seconds * 100;
        for (int i = 0; i < loopCount; i++)
        {
            yield return tick;
        }
    }
}
