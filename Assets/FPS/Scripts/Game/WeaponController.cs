using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// ���⸦ �����ϴ� Ŭ����
    /// </summary>
    public class WeaponController : MonoBehaviour
    {
        #region Variables
        //���� Ȱ��ȭ, ��Ȱ��
        public GameObject weaponRoot;
        
        public GameObject Owner { get; set; }               //������ ����

        //�Ȱ��� ���⸦ ��ø�ؼ� �����ʵ��� üũ
        public GameObject SourcePrefab { get; set; }    //���⸦ ������ �������� ������
        public bool IsWeaponActive {  get; private set; }  //���� Ȱ��ȭ ����

        private AudioSource shootAudioSource;
        public AudioClip swithcWeaponSfx;
        #endregion

        private void Awake()
        {
            //����
            shootAudioSource = this.GetComponent<AudioSource>();
        }

        //���� Ȱ��ȭ, ��Ȱ��ȭ 
        public void ShowWeapon(bool show)
        {
            weaponRoot.SetActive(show);

            //this ����� ����
            if(show == true && swithcWeaponSfx != null)
            {
                //���� ���� ȿ���� �÷���
                shootAudioSource.PlayOneShot(swithcWeaponSfx);
            }
            
            IsWeaponActive = show;
        }
    }
}
