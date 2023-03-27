using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class MyUtil
{
    public static T FindbyCondition<T>(this List<T> list, Func<T, bool> conditional)
    {
        int listCount = list.Count;
        for (int i = 0; i < listCount; ++i)
        {
            if (conditional(list[i]))
            {
                return list[i];
            }
        }

        return default;
    }

    static System.Random rand = new System.Random();
    public static void Shuffle(List<int> list)
    {
        int cnt = list.Count;
        while (cnt > 1)
        {
            cnt--;
            int k = rand.Next(cnt + 1);
            int value = list[k];
            list[k] = list[cnt];
            list[cnt] = value;
        }
    }

    public static Vector3 ConvertXYtoXZ(this Vector2 xy)
    {
        Vector3 result = new Vector3(xy.x, 0, xy.y);

        return result;
    }

    public static Vector3 NormalizedXZ(this Vector3 vector)
    {
        vector.y = 0;

        return vector;
    }
}

public static class WaitTimeCache
{
    public static readonly WaitForEndOfFrame EndFrame = new WaitForEndOfFrame();

    static readonly WaitForSecondsRealtime realTick = new WaitForSecondsRealtime(0.01f);

    public static IEnumerator<WaitForSeconds> WaitForSeconds(float seconds)
    {
        float t = 0;
        while (t < seconds)
        {
            t += Time.deltaTime;
            yield return null;
        }
    }

    public static IEnumerator<WaitForSecondsRealtime> WaitForSecondsRealtime(float seconds)
    {
        int loopCount = (int)seconds * 100;
        for (int i = 0; i < loopCount; i++)
        {
            yield return realTick;
        }
    }

    public static IEnumerator TimerCallbackCoroutine(float seconds, Action action)
    {
        float t = 0;
        while (t < seconds)
        {
            t += Time.deltaTime;
            yield return null;
        }

        action?.Invoke();
    }
}
