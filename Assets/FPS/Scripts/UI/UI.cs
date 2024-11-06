using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI 
{ 

    public class UI : MonoBehaviour
    {
        #region Variables
        public Image crosshairImage;        //ũ�ν���� UI �̹���
        public Sprite nullCrosshairSprite;   //��Ƽ���� ���Ⱑ ������

        private RectTransform crosshairRectTransform;

        private CrossHairData crossHairDefault;     //����,�⺻
        private CrossHairData crossHairTarget;      //Ÿ���� �Ǿ�����

        private CrossHairData crossHairCurret;      //���������� �׸��� ũ�ν����
        [SerializeField] private float crosshairUpdateShrpness = 5.0f; //Lerp ����

        private PlayerWeaponsManager weaponsManager;

        //���°� ���ϴ� ���� ���ϴ� ����
        private bool wasPointingAtEnemy;        //was ���̸� Ʈ���μ��� ���ϴ� ������¼����¼��
        #endregion

        void Start ()
        {
            weaponsManager = GameObject.FindObjectOfType<PlayerWeaponsManager>();
            //��Ƽ���� ���� ũ�ν� ��� ���̱� 
            OnWeaponChanged(weaponsManager.GetActiveWeapon());

            weaponsManager.OnSwitchToWeapon += OnWeaponChanged;
        }

        private void Update()
        {
            UpdateCrossHairPointAtEnemy(false);

            //���ϴ� Ÿ�� ���� 
            wasPointingAtEnemy = weaponsManager.IsPointingAtEnemy;
        }
        //ũ�ν� ��� �׸���
        void UpdateCrossHairPointAtEnemy(bool force) 
        {
            if (crossHairDefault.CraossHairSprite == null)  //�ƿ������ֱ�
                return;

            //���ÿ� Ÿ���� //�ٲ�� ���� ��ƾ���
            //force��� ������� �׸� 
            if ((force || wasPointingAtEnemy == false) && weaponsManager.IsPointingAtEnemy == true) //���� �����ϴ¼���
            {
                crossHairCurret = crossHairTarget;
                crosshairImage.sprite = crossHairCurret.CraossHairSprite;
                crosshairRectTransform.sizeDelta = crossHairCurret.CrossHairSize * Vector2.one;
            }
            else if ((force || wasPointingAtEnemy == true) && weaponsManager.IsPointingAtEnemy == false) //���� ��ġ�� ����
            {
                crossHairCurret = crossHairDefault;
                crosshairImage.sprite = crossHairCurret.CraossHairSprite;
                crosshairRectTransform.sizeDelta = crossHairCurret.CrossHairSize * Vector2.one;
            }
            //crosshairImage.sprite = crossHairCurret.CraossHairSprite;
            //weaponsManager.IsPointingAtEnemy
            //crossHairCurret = crossHairDefault;
            //crossHairCurret = crossHairTarget;

            //crosshairImage.sprite = crossHairDefault.CraossHairSprite;
            //crosshairImage.sprite = crossHairTarget.CraossHairSprite;

            crosshairImage.color = Color.Lerp(crosshairImage.color, crossHairCurret.CraossHairColor,
                crosshairUpdateShrpness * Time.deltaTime);
            crosshairRectTransform.sizeDelta = Mathf.Lerp(crosshairRectTransform.sizeDelta.x, crossHairCurret.CrossHairSize,
                 crosshairUpdateShrpness * Time.deltaTime) * Vector2.one; //����2�����ؾ� ���ͷ� �����ü����� 
        }

        //���Ⱑ �ٲ� ���� crosshairImage�� ������ ���� CrossHair �̹����� �ٲٱ�
        void OnWeaponChanged(WeaponController newWeapon)
        {
            if(newWeapon)
            {
                crosshairImage.enabled = true;
                crosshairRectTransform = crosshairImage.GetComponent<RectTransform>();

                //��Ƽ�� ������ ũ�ν���� ���� ��������
                crossHairDefault = newWeapon.crosshairDefault;
                crossHairTarget = newWeapon.crosshairTartgetInSight;
                //crosshairImage.sprite = newWeapon.crosshairDefault.CraossHairSprite;
            }
            else
            {
                if (nullCrosshairSprite)   
                {
                    crosshairImage.sprite = nullCrosshairSprite;
                }
                else  //��� �� ���Ⱑ ������ �ƞ� �Ⱥ��̱� 
                {
                    crosshairImage.enabled = false;
                }
            }
            UpdateCrossHairPointAtEnemy(true);
        }
    }
}
