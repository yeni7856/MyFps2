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
        /// WeaponController������ Ammo ī��Ʈ UI
        /// </summary>
        #region Variables
        private PlayerWeaponsManager weaponsManager;

        private WeaponController weaponController;
        private int weaponIndex;                                                        //���° ��������

        //UI
        public TextMeshProUGUI weaponIndexText;

        public Image ammoFillImage;                                                 //ammo rate�� ���� ������(�̹���)

        [SerializeField] private float ammoFillSharpness = 10f;             //������ ä���(����) �ӵ�
        [SerializeField] private float weaponSwitchSharpness = 10f;     //���� ��ü�� UI�� �ٲ�� �ӵ� 

        public CanvasGroup canvasGroup;                                          
        [SerializeField] [Range(0,1)] private float unSelectedOpacity = 0.5f;           //���İ� Range�����̵��
        private Vector3 unSelectedScale = Vector3.one * 0.8f;            //���õ��� �������� 80%�� �ٿ���   

        //�������� ������
        public FillBarColorChange fillBarColorChange;

        #endregion

        //AmmoCount UI �� �ʱ�ȭ
       public void Initialize(WeaponController weapon, int _weaponindex)
        {
            weaponController = weapon;
            weaponIndex = _weaponindex;

            //���� �ε���
            weaponIndexText.text = (weaponIndex + 1).ToString();        //0,1,2 -> 1,2,3 ���� ��������

            //�������� �� �ʱ�ȭ
            fillBarColorChange.IniInitialize(1f, 0.1f);

            //����
            weaponsManager = GameObject.FindObjectOfType<PlayerWeaponsManager>();
        }

        private void Update()
        {
            //������ ä���
            float currentFillRate = weaponController.CurretAmmoRatio;
            ammoFillImage.fillAmount = 
                Mathf.Lerp(ammoFillImage.fillAmount, currentFillRate, ammoFillSharpness * Time.deltaTime);

            //��Ƽ�� ���� ����
            bool isActiveWeapon = (weaponController == weaponsManager.GetActiveWeapon());
            float currentOpacity = isActiveWeapon ? 1.0f : unSelectedOpacity;     //���õ� �����̸� ���۽�Ƽ 1.0f �ƴϸ� 0.5f�� �ٿ��� �����ϰԺ��̱�
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, currentOpacity, 
                weaponSwitchSharpness * Time.deltaTime);        //UI ���İ��� ���ϴ� �ӵ�

            Vector3 currentScale = isActiveWeapon ? Vector3.one : unSelectedScale; //��ĳ���� ���õ������̸� ������ ���� �ϰ� �ƴϸ� ����
            transform.localScale = Vector3.Lerp(transform.localScale, currentScale,
                weaponSwitchSharpness * Time.deltaTime);    //UI ũ�Ⱚ�� ���ϴ� �ӵ�

            //���� ����
            fillBarColorChange.UpdateVisual(currentFillRate); 
        }
    }
}
