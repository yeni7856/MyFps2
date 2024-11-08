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
        public RectTransform ammoPanel;              //ammoCountUI �θ� ������Ʈ
        public GameObject ammoCountPrefab;      //ammoCountUI ������

        private PlayerWeaponsManager weaponManager;     //�÷��̾� �����Ŵ��� ���������
        #endregion

        //����ȣ��� UI ȣ���� ���� Start�������� �ȶ������ -> Awake�� ���� ȣ�� 
        private void Awake()
        {
            //���� //���ε������Ʈ�� ã�Ʊ�
            weaponManager = GameObject.FindObjectOfType<PlayerWeaponsManager>();

            weaponManager.OnAddedWeapon += AddWeapon;
            weaponManager.OnRemoveWeapon += RemoveWeapon;
            weaponManager.OnSwitchToWeapon += SwitchWeapon;
        }


        //���� ������ �߰� ammo UI �ϳ� �߰� 
        void AddWeapon(WeaponController newWeapon, int weaponIndex)     //�����߰��� + �ε�����ȣ���� 
        {
            GameObject ammoCountGo = Instantiate(ammoCountPrefab, ammoPanel);
            AmmoCountUI ammoCount = ammoCountGo.GetComponent<AmmoCountUI>(); //AmmoCountUI���� ����������
            ammoCount.Initialize(newWeapon, weaponIndex); //�߰��ϰ� �ε�����ȣ���� 
        }

        //���� �����ϸ� ammo UI �ϳ� ���� 
        void RemoveWeapon(WeaponController newWeapon, int weaponIndex)
        {

        }
        //UI ���̾ƿ� �׷� ������ 
        void SwitchWeapon(WeaponController weapon)  //�Ű������� ������� ������
        {
            //UI ���̾ƿ��׷� ����� Ammoī��Ʈ ������ ������ ���� �ؼ� ������ 
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(ammoPanel);
        }
    }
}
