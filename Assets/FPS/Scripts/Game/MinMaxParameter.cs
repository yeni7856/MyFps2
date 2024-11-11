using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// ratio�Ű� ������ �޾� float min���� max������ Lerp �� ��ȯ
    /// </summary>
    [System.Serializable]
    public struct MinMaxFloat
    {
        public float Min; 
        public float Max;

        public float GetValueFromRatio(float ratio)
        {
            return Mathf.Lerp(Min, Max, ratio); 
        }
    }
    /// <summary>
    /// ratio�Ű� ������ �޾� color min���� max������ Lerp �� ��ȯ
    /// </summary>
    [System.Serializable]
    public struct MinMaxColor
    {
        public Color Min;
        public Color Max;

        public Color GetValueFromRatio(float ratio)
        {
            return Color.Lerp(Min, Max, ratio);

        }
    }
    /// <summary>
    /// ratio�Ű� ������ �޾� Vector3 min���� max������ Lerp �� ��ȯ
    /// </summary>
    [System.Serializable]
    public struct MinMaxVector3
    {
        public Vector3 Min;
        public Vector3 Max;

        public Vector3 GetValueFromRatio(float ratio)
        {
            return Vector3.Lerp(Min, Max, ratio);
        }
    }

}
