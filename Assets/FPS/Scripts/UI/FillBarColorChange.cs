using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Unity.FPS.UI
{
    /// <summary>
    /// 게이지바의 게이지컬러, 백그라운드색 변경 구현
    /// </summary>
    public class FillBarColorChange : MonoBehaviour
    {
        #region Variables
        public Image foregroundImage;
        public Color defaultForegroundColor;                //게이지의 기본컬러 
        public Color flashForceGroundColorFull;             //게이지가 풀로 차는 순간 색 플래시 효과

        public Image backgroundImage;                         //백그라운드 이미지
        public Color defaultbackgroundColor;                //백그라운드 기본색
        public Color flashBackgroundColorEmpty;          //백그라운드 비었을때 넣는색

        private float fullValue = 1f;                                   //게이지가 풀일때의 값
        private float emptyValue = 0f;                              //게이지가 비었을때의 값

        private float colorChangeSharpness = 5f;            //컬러 변경 속도
        private float prevousValue;                                 //게이지 풀로차는 순간을 찾는 변수
        #endregion

        //색 변경 관련 값 초기화 -> AmmoCountUI 에서 불러와서 사용 
        public void IniInitialize(float fullValueRatio, float emptyValueRaion)
        {
            fullValue = fullValueRatio;
            emptyValue = emptyValueRaion;

            prevousValue = fullValue;
        }

        //비쥬얼 업데이트 ->AmmoCountUI 에서 불러와서 사용 
        public void UpdateVisual(float currentRatio)
        {
            //게이지가 풀로 찬순간 currentRatio (fullValue)풀이되고 (prevousValue)아직 풀이 안됬을때 
            if (currentRatio == fullValue && currentRatio != prevousValue)
            {
                foregroundImage.color = flashForceGroundColorFull;
            }
            else if (currentRatio < emptyValue) 
            {
                //백그라운드 emptyValue 보다 작으면 
                backgroundImage.color = flashBackgroundColorEmpty;
            }
            else
            {
                //원래색 돌아오기 
                foregroundImage.color = Color.Lerp(foregroundImage.color, defaultForegroundColor,
                    colorChangeSharpness * Time.deltaTime);
                //백그라운드
                backgroundImage.color = Color.Lerp(backgroundImage.color, defaultbackgroundColor,
                    colorChangeSharpness * Time.deltaTime);
            }

            prevousValue = currentRatio;
        }
    }
}