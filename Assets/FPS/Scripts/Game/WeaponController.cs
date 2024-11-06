using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// ������ ũ�ν��� �׸������� ���� �ϴ� ������ 
    /// </summary>
    [System.Serializable]
    public struct CrossHairData
    {
        public Sprite CraossHairSprite;
        public float CrossHairSize;
        public Color CraossHairColor;
    }

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

        //CrossHair
        public CrossHairData crosshairDefault;      //�⺻,���� 

        public CrossHairData crosshairTartgetInSight;   //���� ����

        //����
        public float aimZoomRatio = 1f;                 //���ؽ� ���� �����ܾƿ� ������
        public Vector3 animOffset;                      //���ؽ� ���� ��ġ����
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
