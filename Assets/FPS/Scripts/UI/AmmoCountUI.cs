using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class AmmoCountUI : MonoBehaviour
    {
        /// <summary>
        /// WeaponController무기의 Ammo 카운트 UI
        /// </summary>
        #region Variables
        private PlayerWeaponsManager weaponsManager;

        private WeaponController weaponController;
        private int weaponIndex;                                                        //몇번째 위폰인지

        //UI
        public TextMeshProUGUI weaponIndexText;

        public Image ammoFillImage;                                                 //ammo rate에 따른 게이지(이미지)

        [SerializeField] private float ammoFillSharpness = 10f;             //게이지 채우는(비우는) 속도
        [SerializeField] private float weaponSwitchSharpness = 10f;     //무기 교체시 UI가 바뀌는 속도 

        public CanvasGroup canvasGroup;                                          
        [SerializeField] [Range(0,1)] private float unSelectedOpacity = 0.5f;           //알파값 Range슬라이드로
        private Vector3 unSelectedScale = Vector3.one * 0.8f;            //선택되지 않은놈을 80%로 줄여서   

        //게이지바 색변경
        public FillBarColorChange fillBarColorChange;

        #endregion

        //AmmoCount UI 값 초기화
       public void Initialize(WeaponController weapon, int _weaponindex)
        {
            weaponController = weapon;
            weaponIndex = _weaponindex;

            //무기 인덱스
            weaponIndexText.text = (weaponIndex + 1).ToString();        //0,1,2 -> 1,2,3 으로 보여야함

            //게이지색 값 초기화
            fillBarColorChange.IniInitialize(1f, 0.1f);

            //참조
            weaponsManager = GameObject.FindObjectOfType<PlayerWeaponsManager>();
        }

        private void Update()
        {
            //게이지 채우기
            float currentFillRate = weaponController.CurretAmmoRatio;
            ammoFillImage.fillAmount = 
                Mathf.Lerp(ammoFillImage.fillAmount, currentFillRate, ammoFillSharpness * Time.deltaTime);

            //액티브 무기 구분
            bool isActiveWeapon = (weaponController == weaponsManager.GetActiveWeapon());
            float currentOpacity = isActiveWeapon ? 1.0f : unSelectedOpacity;     //선택된 위폰이면 오퍼시티 1.0f 아니면 0.5f로 줄여서 사용못하게보이기
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, currentOpacity, 
                weaponSwitchSharpness * Time.deltaTime);        //UI 알파값이 변하는 속도

            Vector3 currentScale = isActiveWeapon ? Vector3.one : unSelectedScale; //스캐일을 선택된위폰이면 스케일 조정 하고 아니면 안함
            transform.localScale = Vector3.Lerp(transform.localScale, currentScale,
                weaponSwitchSharpness * Time.deltaTime);    //UI 크기값이 변하는 속도

            //배경색 변경
            fillBarColorChange.UpdateVisual(currentFillRate); 
        }
    }
}
