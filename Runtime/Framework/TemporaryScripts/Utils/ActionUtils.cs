using UnityEngine;
using System.Collections.Generic;

public static class ActionUtils
{

    public static T RandomChooseOne<T>(T[] array)
    {
        if (array.Length == 1)
        {
            return array[0];
        }
        return array[MathUtils.Random.RandomInt(array.Length)];
    }

    /// <summary>
    /// 从array中随机选n个
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <param name="n">n>=2</param>
    /// <returns></returns>
    public static T[] RandomChooseN<T>(T[] array, int n)
    {
        if(array != null)
        {
            if(n >= array.Length)
            {
                return array;
            }
            else
            {
                int[] pool = new int[n];
                for (int i = 0; i < n; i++) pool[i] = i;
                for(int i = n; i < array.Length; i++)
                {
                    int pos = MathUtils.Random.RandomInt(i + 1);
                    if(pos < n)
                    {
                        pool[pos] = i;
                    }
                }
                T[] ret = new T[n];
                for(int i = 0; i < n; i++)
                {
                    ret[i] = array[pool[i]];
                }
                return ret;
            }
        }
        return null;
    }

    /// <summary>
    /// 洗牌并返回洗牌之后的数组
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <returns></returns>
    public static T[] Shuffle<T>(T[] array)
    {
        T[] ret = new T[array.Length];
        if(array != null && array.Length > 0)
        {
            ret[0] = array[0];
            for(int i = 1; i < array.Length; i++)
            {
                int pos = MathUtils.Random.RandomInt(i);
                ret[i] = ret[pos];
                ret[pos] = array[i];
            }
        }
        return ret;
    }

    public static void Shuffle<T>(List<T> list)
    {
        if (list != null && list.Count > 0)
        {
            for (int i = 1; i < list.Count; i++)
            {
                int pos = MathUtils.Random.RandomInt(i);
                var temp = list[i];
                list[i] = list[pos];
                list[pos] = temp;
            }
        }
    }

    public static float CalculateRotationAngle(Vector3 from, Vector3 to)
    {
        float angle = Vector3.Angle(from, to);
        if (Vector3.Cross(from, to).y < 0)
        {
            angle *= -1f;
        }
        return angle;
    }

    public static int Sign(float f)
    {
        if (f > 1e-6f)
        {
            return 1;
        }
        else if (f < -1e-6f)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }

    public static float RoundWithMinStep(float number, float precision)
    {
        float ret = Mathf.Round(number / precision) * precision;
        return ret < precision ? precision : ret;
    }

    public static int CmpF(float a, float b)
    {
        return Sign(a - b);
    }
}
