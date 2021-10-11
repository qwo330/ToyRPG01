using System.Collections;
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
}

public static class WaitTimeChache
{
    public static readonly WaitForEndOfFrame EndFrame = new WaitForEndOfFrame();

    static readonly WaitForSeconds tick = new WaitForSeconds(0.01f);
    public static IEnumerator<WaitForSeconds> WaitForSeconds(float seconds)
    {
        int loopCount = (int)seconds * 100;
        for (int i = 0; i < loopCount; i++)
        {
            yield return tick;
        }
    }

    public static IEnumerator TimerCallbackCoroutine(float time, Action action)
    {
        float t = 0;
        while (t < time)
        {
            t += Time.deltaTime;
            yield return null;
        }

        action?.Invoke();
    }
}
