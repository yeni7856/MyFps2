using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.FPS.UI
{
    public class WeaponHUDManager : MonoBehaviour
    {
        #region Variables
        public RectTransform ammoPanel;              //ammoCountUI 부모 오브젝트
        public GameObject ammoCountPrefab;      //ammoCountUI 프리팹

        private PlayerWeaponsManager weaponManager;     //플레이어 위폰매니져 가지고오기
        #endregion

        //무기호출과 UI 호출이 같이 Start에있으면 안뜰수있음 -> Awake로 먼저 호출 
        private void Awake()
        {
            //참조 //파인드오브잭트로 찾아괴
            weaponManager = GameObject.FindObjectOfType<PlayerWeaponsManager>();

            weaponManager.OnAddedWeapon += AddWeapon;
            weaponManager.OnRemoveWeapon += RemoveWeapon;
            weaponManager.OnSwitchToWeapon += SwitchWeapon;
        }


        //무기 생성시 추가 ammo UI 하나 추가 
        void AddWeapon(WeaponController newWeapon, int weaponIndex)     //무기추가시 + 인덱스번호까지 
        {
            GameObject ammoCountGo = Instantiate(ammoCountPrefab, ammoPanel);
            AmmoCountUI ammoCount = ammoCountGo.GetComponent<AmmoCountUI>(); //AmmoCountUI에서 값가져오기
            ammoCount.Initialize(newWeapon, weaponIndex); //추가하고 인덱스번호까지 
        }

        //무기 제거하면 ammo UI 하나 제거 
        void RemoveWeapon(WeaponController newWeapon, int weaponIndex)
        {

        }
        //UI 레이아웃 그룹 리빌딩 
        void SwitchWeapon(WeaponController weapon)  //매개변수랑 상관없이 리빌딩
        {
            //UI 레이아웃그룹 재빌딩 Ammo카운트 강제로 재정렬 주의 해서 쓰세요 
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(ammoPanel);
        }
    }
}
