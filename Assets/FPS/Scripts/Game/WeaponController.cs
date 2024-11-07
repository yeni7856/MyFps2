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
    /// ���� ��Ÿ�� 
    /// </summary>
    public enum WeaponShootType
    {
        Manual,
        Automatic,
        Charge,
        Sniper
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
        public AudioClip swithcWeaponSfx;       //���ⱳȯ ����

        //����
        public WeaponShootType shootType;

        //Ammo �Ѿ� ���� 
        [SerializeField] private float maxAmmo = 8f;        //�ִ� �Ѿ� ����
        private float currentAmmo;

        [SerializeField] private float delayBetweenShots = 0.5f;    //�� ����
        private float lastTimeShot;                                               //���������� ���� �ð�

        //Vfx, Sfx
        public Transform weaponMuzzle;                                  //�ѱ� ��ġ 
        public GameObject muzzleFlashPrefab;                        //�ѱ� �߻� ȿ��
        public AudioClip shootSfx;                                            //�� �߻� ���� 


        //CrossHair
        public CrossHairData crosshairDefault;      //�⺻,���� 

        public CrossHairData crosshairTartgetInSight;   //���� ����

        //����
        public float aimZoomRatio = 1f;                 //���ؽ� ���� �����ܾƿ� ������
        public Vector3 animOffset;                      //���ؽ� ���� ��ġ����

        //�ݵ�
        public float recoilForce = 0.5f;                //�ݵ�

        //Projectile
        public ProjectileBase projectilePrefab;

        public Vector3 MuzzleWorldVelocity {  get; private set; }       //�ѱ��� �߻�ü �Ѱ��ֱ�
        private Vector3 lastMuzzlePosition;                                     //�ѱ� ������ ��ġ 
        public float CurrentCharge { get; private set; }                    
        #endregion

        private void Awake()
        {
            //����
            shootAudioSource = this.GetComponent<AudioSource>();
        }

        private void Start()
        {
            //�ʱ�ȭ
            currentAmmo = maxAmmo;  //�� �� 
            lastTimeShot = Time.time;     //�����Ҷ� �ð� ����   
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
        //�� ���� - Ű �Է¿� ���� 
        public bool HandleShootInputs(bool inputDown, bool inputHeld, bool inputUp)
        {
            switch (shootType)
            {
                case WeaponShootType.Manual:
                    if (inputDown)
                    {
                        return TryShoot();
                    
                    }
                    break;
                case WeaponShootType.Automatic:
                    if (inputHeld)
                    {
                        return TryShoot();
                    }
                    break;
                case WeaponShootType.Charge:
                    break;
                case WeaponShootType.Sniper:
                    if (inputDown)
                    {
                        return TryShoot();
                    }
                    break;
            }

            return false;
        }

        //���� + ���ϱ� 
        bool TryShoot()
        {
            //������ �ð� + �����̽ð� < ����ð��̴� ũ�� ������ ������
            //�ѹߺ��� ũ�ų������� ���� ���ϰ� �ð�����������
            if(currentAmmo >= 1f && (lastTimeShot + delayBetweenShots) < Time.time)
            {
                currentAmmo -= 1f;
                Debug.Log($"currentAmmo : {currentAmmo}");

                HandleShoot();

                return true;
            }
            return false;   //���Ǿȸ����� 

        }

        //�� ���� 
        void HandleShoot()
        {
            //Vfx
            if(muzzleFlashPrefab != null)
            {
                GameObject effectGo = Instantiate(muzzleFlashPrefab, weaponMuzzle.position, weaponMuzzle.rotation, weaponMuzzle);
                Destroy(effectGo, 2f);
            }

            //Sfx
            if(shootSfx != null)
            {
                shootAudioSource.PlayOneShot(shootSfx);
            }
            lastTimeShot = Time.time; //�� ���������� ���� �ð� ����
            
        }
    }
}
