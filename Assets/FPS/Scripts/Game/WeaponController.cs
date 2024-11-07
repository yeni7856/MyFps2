using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// 조준점 크로스헤어를 그리기위한 관리 하는 데이터 
    /// </summary>
    [System.Serializable]
    public struct CrossHairData
    {
        public Sprite CraossHairSprite;
        public float CrossHairSize;
        public Color CraossHairColor;
    }

    /// <summary>
    /// 무기 슛타입 
    /// </summary>
    public enum WeaponShootType
    {
        Manual,
        Automatic,
        Charge,
        Sniper
    }

    /// <summary>
    /// 무기를 관리하는 클래스
    /// </summary>
    public class WeaponController : MonoBehaviour
    {
        #region Variables
        //무기 활성화, 비활성
        public GameObject weaponRoot;
        
        public GameObject Owner { get; set; }               //무기의 주인

        //똑같은 무기를 중첩해서 갖지않도록 체크
        public GameObject SourcePrefab { get; set; }    //무기를 생성한 오리지널 프리팹
        public bool IsWeaponActive {  get; private set; }  //무기 활성화 여부

        private AudioSource shootAudioSource;
        public AudioClip swithcWeaponSfx;       //무기교환 사운드

        //슈팅
        public WeaponShootType shootType;

        //Ammo 총알 장전 
        [SerializeField] private float maxAmmo = 8f;        //최대 총알 갯수
        private float currentAmmo;

        [SerializeField] private float delayBetweenShots = 0.5f;    //슛 간격
        private float lastTimeShot;                                               //마지막으로 슛한 시간

        //Vfx, Sfx
        public Transform weaponMuzzle;                                  //총구 위치 
        public GameObject muzzleFlashPrefab;                        //총구 발사 효과
        public AudioClip shootSfx;                                            //총 발사 사운드 


        //CrossHair
        public CrossHairData crosshairDefault;      //기본,평상시 

        public CrossHairData crosshairTartgetInSight;   //적을 포착

        //조준
        public float aimZoomRatio = 1f;                 //조준시 무기 줌인줌아웃 설정값
        public Vector3 animOffset;                      //조준시 무기 위치조정

        //반동
        public float recoilForce = 0.5f;                //반동

        //Projectile
        public ProjectileBase projectilePrefab;

        public Vector3 MuzzleWorldVelocity {  get; private set; }       //총구에 발사체 넘겨주기
        private Vector3 lastMuzzlePosition;                                     //총구 마지막 위치 
        public float CurrentCharge { get; private set; }                    
        #endregion

        private void Awake()
        {
            //참조
            shootAudioSource = this.GetComponent<AudioSource>();
        }

        private void Start()
        {
            //초기화
            currentAmmo = maxAmmo;  //총 알 
            lastTimeShot = Time.time;     //시작할때 시간 저장   
        }

        //무기 활성화, 비활성화 
        public void ShowWeapon(bool show)
        {
            weaponRoot.SetActive(show);

            //this 무기로 변경
            if(show == true && swithcWeaponSfx != null)
            {
                //무기 변경 효과음 플레이
                shootAudioSource.PlayOneShot(swithcWeaponSfx);
            }
            
            IsWeaponActive = show;
        }
        //슛 구현 - 키 입력에 따른 
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

        //장전 + 슛하기 
        bool TryShoot()
        {
            //마지막 시간 + 딜레이시간 < 현재시간이더 크면 딜레이 지나감
            //한발보다 크거나같으면 슛을 못하게 시간재장전으로
            if(currentAmmo >= 1f && (lastTimeShot + delayBetweenShots) < Time.time)
            {
                currentAmmo -= 1f;
                Debug.Log($"currentAmmo : {currentAmmo}");

                HandleShoot();

                return true;
            }
            return false;   //조건안맞으면 

        }

        //슛 연출 
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
            lastTimeShot = Time.time; //슛 간격을위해 슛한 시간 저장
            
        }
    }
}
