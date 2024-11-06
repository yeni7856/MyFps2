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
        public Image crosshairImage;        //크로스헤어 UI 이미지
        public Sprite nullCrosshairSprite;   //액티브한 무기가 없을때

        private RectTransform crosshairRectTransform;

        private CrossHairData crossHairDefault;     //평상시,기본
        private CrossHairData crossHairTarget;      //타겟팅 되었을때

        private CrossHairData crossHairCurret;      //실질적으로 그리는 크로스헤어
        [SerializeField] private float crosshairUpdateShrpness = 5.0f; //Lerp 변수

        private PlayerWeaponsManager weaponsManager;

        //상태가 변하는 구간 구하는 변수
        private bool wasPointingAtEnemy;        //was 붙이면 트루인순간 구하는 변수어쩌고저쩌고
        #endregion

        void Start ()
        {
            weaponsManager = GameObject.FindObjectOfType<PlayerWeaponsManager>();
            //액티브한 무기 크로스 헤어 보이기 
            OnWeaponChanged(weaponsManager.GetActiveWeapon());

            weaponsManager.OnSwitchToWeapon += OnWeaponChanged;
        }

        private void Update()
        {
            UpdateCrossHairPointAtEnemy(false);

            //변하는 타임 저장 
            wasPointingAtEnemy = weaponsManager.IsPointingAtEnemy;
        }
        //크로스 헤어 그리기
        void UpdateCrossHairPointAtEnemy(bool force) 
        {
            if (crossHairDefault.CraossHairSprite == null)  //아예막아주기
                return;

            //평상시에 타겟팅 //바뀌는 순간 잡아야함
            //force얘랑 상관없이 그림 
            if ((force || wasPointingAtEnemy == false) && weaponsManager.IsPointingAtEnemy == true) //적을 포착하는순간
            {
                crossHairCurret = crossHairTarget;
                crosshairImage.sprite = crossHairCurret.CraossHairSprite;
                crosshairRectTransform.sizeDelta = crossHairCurret.CrossHairSize * Vector2.one;
            }
            else if ((force || wasPointingAtEnemy == true) && weaponsManager.IsPointingAtEnemy == false) //적을 놓치는 순간
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
                 crosshairUpdateShrpness * Time.deltaTime) * Vector2.one; //백터2원곱해야 백터로 가져올수있음 
        }

        //무기가 바뀔때 마다 crosshairImage를 각각의 무기 CrossHair 이미지로 바꾸기
        void OnWeaponChanged(WeaponController newWeapon)
        {
            if(newWeapon)
            {
                crosshairImage.enabled = true;
                crosshairRectTransform = crosshairImage.GetComponent<RectTransform>();

                //액티브 무기의 크로스헤어 정보 가져오기
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
                else  //등록 한 무기가 없을때 아옝 안보이기 
                {
                    crosshairImage.enabled = false;
                }
            }
            UpdateCrossHairPointAtEnemy(true);
        }
    }
}
