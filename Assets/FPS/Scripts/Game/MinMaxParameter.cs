using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// ratio매개 변수로 받아 float min에서 max사이의 Lerp 값 반환
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
    /// ratio매개 변수로 받아 color min에서 max사이의 Lerp 값 반환
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
    /// ratio매개 변수로 받아 Vector3 min에서 max사이의 Lerp 값 반환
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
