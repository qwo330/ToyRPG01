using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class MyUtil
{
    public static bool IsNullOrDestroyed(this object obj)
    {
        if (obj is null)
            return true;
        
        if (obj.GetType().IsValueType)
            return false;

        if (obj is UnityEngine.Object unityObj)
            return unityObj == null;
        
        return false;
    }
    
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

    static readonly System.Random rand = new System.Random();
    public static void Shuffle(List<int> list)
    {
        var cnt = list.Count;
        while (cnt > 1)
        {
            cnt--;
            var k = rand.Next(cnt + 1);
            (list[k], list[cnt]) = (list[cnt], list[k]);
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

        return vector.normalized;
    }
    
    public static bool FloatEqual(float curr, float target)
    {
        return (Math.Abs(curr - target) < 0.0001f);
    }

    public static int Modulus(int x, int y)
    {
        return x - (x / y) * y;
    }

    static readonly string[] numberUnits = { "", "K", "M", "B", "T" };
    public static string ToNumberString(this long value, int decimals = 2)
    {
        if (value == 0) return "0";
        if (value < 0) return "-" + Math.Abs(value).ToNumberString(decimals);
        if (value < 1000) return value.ToString();

        var index = 0;
        float num = value; // 값 훼손을 막기 위해 계산용 실수로 변환하여 진행

        while (num >= 1000f && index < numberUnits.Length - 1)
        {
            num /= 1000f;
            index++;
        }

        var format = "F" + decimals;
        var result = num.ToString(format);

        if (decimals > 0 && result.Contains("."))
        {
            result = result.TrimEnd('0').TrimEnd('.');
        }

        return $"{result}{numberUnits[index]}";
    }

    public static string ToNumberString(this int value, int decimals = 2)
    {
        return ((long)value).ToNumberString(decimals);
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

public static class VectorExtensions
{
    public static void SetY(this ref Vector3 vector, float y)
    {
        vector.y = y;
    }
}
