using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Unity.FPS.UI
{
    /// <summary>
    /// ���������� �������÷�, ��׶���� ���� ����
    /// </summary>
    public class FillBarColorChange : MonoBehaviour
    {
        #region Variables
        public Image foregroundImage;
        public Color defaultForegroundColor;                //�������� �⺻�÷� 
        public Color flashForceGroundColorFull;             //�������� Ǯ�� ���� ���� �� �÷��� ȿ��

        public Image backgroundImage;                         //��׶��� �̹���
        public Color defaultbackgroundColor;                //��׶��� �⺻��
        public Color flashBackgroundColorEmpty;          //��׶��� ������� �ִ»�

        private float fullValue = 1f;                                   //�������� Ǯ�϶��� ��
        private float emptyValue = 0f;                              //�������� ��������� ��

        private float colorChangeSharpness = 5f;            //�÷� ���� �ӵ�
        private float prevousValue;                                 //������ Ǯ������ ������ ã�� ����
        #endregion

        //�� ���� ���� �� �ʱ�ȭ -> AmmoCountUI ���� �ҷ��ͼ� ��� 
        public void IniInitialize(float fullValueRatio, float emptyValueRaion)
        {
            fullValue = fullValueRatio;
            emptyValue = emptyValueRaion;

            prevousValue = fullValue;
        }

        //����� ������Ʈ ->AmmoCountUI ���� �ҷ��ͼ� ��� 
        public void UpdateVisual(float currentRatio)
        {
            //�������� Ǯ�� ������ currentRatio (fullValue)Ǯ�̵ǰ� (prevousValue)���� Ǯ�� �ȉ����� 
            if (currentRatio == fullValue && currentRatio != prevousValue)
            {
                foregroundImage.color = flashForceGroundColorFull;
            }
            else if (currentRatio < emptyValue) 
            {
                //��׶��� emptyValue ���� ������ 
                backgroundImage.color = flashBackgroundColorEmpty;
            }
            else
            {
                //������ ���ƿ��� 
                foregroundImage.color = Color.Lerp(foregroundImage.color, defaultForegroundColor,
                    colorChangeSharpness * Time.deltaTime);
                //��׶���
                backgroundImage.color = Color.Lerp(backgroundImage.color, defaultbackgroundColor,
                    colorChangeSharpness * Time.deltaTime);
            }

            prevousValue = currentRatio;
        }
    }
}