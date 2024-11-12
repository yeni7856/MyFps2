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
        [SerializeField] private float maxAmmo = 8f;        //���� �ִ� �Ѿ� ����
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

        public Vector3 MuzzleWorldVelocity {  get; private set; }       //�ѱ� ������ �ӵ� 
        private Vector3 lastMuzzlePosition;                                     //�ѱ� ������ ��ġ 

        [SerializeField] private int bulletsPerShot = 1;                    //�ѹ� ���ϴµ� �߻�Ǵ� źȯ�� ����
        [SerializeField] private float bulletSpreadAngle = 0f;         //�淿�� ���� ������ ����

        //Charge : �߻� ��ư�� ������ ������ �ߘ����� ������, �ӵ��� ���������� Ŀ����
        public float CurrentCharge { get; private set; }                        //���� 0 ~ 1
        public bool IsCharging { get; private set; }

        [SerializeField] private float ammoUseOnStartCharge = 1f;           //���� ���� ��ư�� ������ ���� �ʿ��� ammo 
        [SerializeField] private float ammoUseRateWhileCharging = 1f;    //�����ϰ� �ִµ��� �Һ�Ǵ� ammo��
        private float maxChargeDuration = 2f;                                         //���� �ð� Max

        public float lastChargeTriggerTimeStamp;                                    //���� ���� �ð�

        //Reload : ������
        [SerializeField] private float ammoReloadRate = 1f;                     //�ʴ� �������Ǵ� ��
        [SerializeField] private float ammoReloadDelay = 2f;                   //�� �Ѵ��� ammoReloadRate 2�ʵڿ� ������
        [SerializeField] private bool automaticReload = true;                   //�ڵ�, ���� ����

        public float CurretAmmoRatio => currentAmmo / maxAmmo;      
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
            lastMuzzlePosition = weaponMuzzle.position;
        }

        private void Update()
        {
            //���� ���� ������Ʈ
            UpadateCharge();

            //Reload - Auto // �ڵ� ������ ������Ʈ
            UpdateAmmo();

            //MuzzleWorldVelocity
            if (Time.deltaTime > 0f)     //�������ӵ��µ��� //�������������� 0 
            {
                MuzzleWorldVelocity = (weaponMuzzle.position - lastMuzzlePosition) / Time.deltaTime;   //�Ÿ�/�ð� = �ӵ�  
                
                lastMuzzlePosition = weaponMuzzle.position;     //���������� �����ǿ� 
            }
        }

        //Reload - Auto
        public void UpdateAmmo()
        {
            //automaticReload + Ŀ��Ʈ�Ƹ� �ƽ��Ƹ𺸴� �۰� + ��¡�ȵ��ְ� + ��Ʈ���̶� �Ƹ������ Ÿ���� ����Ÿ�Ӻ��� �۾ƾ���
            if (automaticReload && currentAmmo < maxAmmo && IsCharging == false 
                && lastTimeShot + ammoReloadDelay < Time.time)
            {
                //Debug.Log("�ڵ� ������ ����");
                currentAmmo += ammoReloadRate * Time.deltaTime;     //�ʴ� ammoReloadRate�� ������
                currentAmmo = Mathf.Clamp(currentAmmo, 0, maxAmmo);
            }
        }

        //Reload - ����
        public void Reload()
        {
            if (automaticReload || currentAmmo >= maxAmmo || IsCharging)      //max�� �����ϸ� ������x
            {
                return;
            }
            currentAmmo = maxAmmo;
        }


        //����
        void UpadateCharge()
        {
            if (IsCharging)
            {
                if(CurrentCharge < 1f)      //1�̵Ǹ� ���̻� ����x
                {
                    //���� �����ִ� ������
                    float chargeLeft = 1f - CurrentCharge;

                    float chargeAdd = 0f;   //�̹� �����ӿ� ������ ��
                    if(maxChargeDuration <= 0f)
                    {
                        chargeAdd = chargeLeft;         //�ѹ��� Ǯ ����
                    }
                    else
                    {
                        chargeAdd = (1f/maxChargeDuration) * Time.deltaTime;
                    }
                    chargeAdd = Mathf.Clamp(chargeAdd, 0f, chargeLeft);     //�����ִ� ���������� �۾ƾߵ�

                    //chargeAdd ��ŭ Ammo �Һ��� ���Ѵ�
                    float ammoThisChargeRequire = chargeAdd * ammoUseRateWhileCharging;
                    if(ammoThisChargeRequire <= currentAmmo)
                    {
                        UseAmmo(ammoThisChargeRequire);
                        CurrentCharge = Mathf.Clamp01(CurrentCharge + chargeAdd);
                    }
                }
            }
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
                    if (inputHeld)
                    {
                        //��������
                        TryBeginCharge();
                    }
                    if (inputUp)
                    {
                        //���� �� �߻�
                        return TryReleaseCharge();
                    }
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

        //���� ���� //������ ������ ������ �ѹ���
        void TryBeginCharge()
        {
            //í¡ �޽� �� �������� �����ϴ¾纸�� ���ų� ũ��
            //���� ����
            if (IsCharging == false && currentAmmo >= ammoUseOnStartCharge
                && (lastTimeShot + delayBetweenShots) < Time.time) 
            {
                UseAmmo(ammoUseOnStartCharge);  //������ �����ϸ� �̸������Ѹ�ŭ����� ���� 
                lastChargeTriggerTimeStamp = Time.time; 
                IsCharging = true;  
            }
        }
    
        //���� �� - �߻�
        bool TryReleaseCharge()     //���� �������̿��ߵ�
        {
            if (IsCharging)
            {
                //��
                HandleShoot();

                //�ʱ�ȭ
                CurrentCharge = 0f;
                IsCharging = false;

                return true;
            }

            return false;
        }

        void UseAmmo(float amount)
        {
            currentAmmo = Mathf.Clamp( currentAmmo - amount, 0f, maxAmmo);
            lastTimeShot = Time.time;
        }

        //���� + ���ϱ� 
        bool TryShoot()
        {
            //������ �ð� + �����̽ð� < ����ð��̴� ũ�� ������ ������
            //�ѹߺ��� ũ�ų������� ���� ���ϰ� �ð�����������
            if(currentAmmo >= 1f && (lastTimeShot + delayBetweenShots) < Time.time)
            {
                currentAmmo -= 1f;
                //Debug.Log($"currentAmmo : {currentAmmo}");

                HandleShoot();

                return true;
            }
            return false;   //���Ǿȸ����� 

        }

        //�� ���� 
        void HandleShoot()
        {
            //project tile ����
            //�淿 ����ŭ 
            for (int i = 0; i < bulletsPerShot; i++)
            {
                Vector3 shotDirection = GetShotDirectionWithinSpread(weaponMuzzle);
                ProjectileBase projectileInstance =  Instantiate(projectilePrefab, weaponMuzzle.position, Quaternion.LookRotation(shotDirection));
                //Destroy(projectileInstance.gameObject, 5f);
                projectileInstance.Shoot(this); //��Ʈ�ѷ��� �Ű������� 
            }
           
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
        //projectile ���󰡴� ���� 
        Vector3 GetShotDirectionWithinSpread(Transform shootTransfrom)
        {
            float spreadAngleRatio = bulletSpreadAngle / 180f;
            return Vector3.Lerp(shootTransfrom.forward, UnityEngine.Random.insideUnitSphere, spreadAngleRatio);
        }
    }
}
