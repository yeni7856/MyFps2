using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ChargedWeaponEffectHandler : MonoBehaviour
    {
        #region Variables
        public GameObject chargingObject;               //충전하는 발사체
        public GameObject spiningFrame;                 //발사체를 감싸고 있는 프레임
        public GameObject distOrbitParticePrefab;   //발사체 회전하는 이펙트

        public MinMaxVector3 scale;                       //발사체 스케일

        [SerializeField] private Vector3 offset;
        public Transform parentTransform;

        public MinMaxFloat orbitY;                         //이펙트 설정값
        public MinMaxVector3 radius;                    //이펙트 설정값 

        public MinMaxFloat spiningSpeed;            //회전 설정값

        //Sfx
        public AudioClip chargeSound;
        public AudioClip loopChargeWeaponSfx;   

        [SerializeField] private float fadeLoopDuration = 0.5f;
        public bool useProceduralPitchOnLoop;

        public float maxProceduralPitchValue = 2f;

        private AudioSource audioSource;
        private AudioSource audioSourceLoop;

        //
        public GameObject particleInstance {  get; private set; }
        private ParticleSystem diskOrbitParticle;
        private ParticleSystem.VelocityOverLifetimeModule velocityOverLifetimeModule;

        private WeaponController weaponController;

        private float lastChargeTriggerTimeStamp;
        private float endChargeTime;
        private float chargeRatio;                      //현재 충전량
        #endregion

        private void Awake()
        {
            //chargeSound play
            audioSource =  gameObject.AddComponent<AudioSource>();
            audioSource.clip = chargeSound;
            audioSource.playOnAwake = false;

            audioSourceLoop = gameObject.AddComponent<AudioSource>();
            audioSourceLoop.clip = loopChargeWeaponSfx;
            audioSourceLoop.playOnAwake=false;
            audioSourceLoop.loop = true; 

        }

        void SpawParticleSystem()
        {
            particleInstance = Instantiate(distOrbitParticePrefab, parentTransform != null ? parentTransform : transform);
            particleInstance.transform.localPosition += offset;

            FindRefenece();
        }
        
        void FindRefenece()
        {
            //파티클 시스템 
            diskOrbitParticle = particleInstance.GetComponent<ParticleSystem>();
            velocityOverLifetimeModule = diskOrbitParticle.velocityOverLifetime;

            weaponController = GetComponent<WeaponController>();    
        }

        private void Update()
        {
            //한번만 객체 만들기 
            if (particleInstance == null)        //널일때만 스폰해서 객체만들고
            {
                SpawParticleSystem();
            }
            //파티클 IsWeaponActive 하면 활성화 아니면 비활성화 
            diskOrbitParticle.gameObject.SetActive(weaponController.IsWeaponActive);
            //chargeRatio 충전량에 따른 이펙트
            chargeRatio = weaponController.CurrentCharge;

            //disk, frame 회전,
            chargingObject.transform.localScale = scale.GetValueFromRatio(chargeRatio);
            if (spiningFrame)
            {
                //y 축만 돌아가게 
                spiningFrame.transform.localRotation *= Quaternion.Euler(0f,
                    spiningSpeed.GetValueFromRatio(chargeRatio) * Time.deltaTime, 0f);
            }

            //VFX
            //particle
            velocityOverLifetimeModule.orbitalY = orbitY.GetValueFromRatio(chargeRatio);
            diskOrbitParticle.transform.localScale = radius.GetValueFromRatio(chargeRatio);

            //SFX
            if (chargeRatio > 0f)        //충전시작
            {
                if (audioSourceLoop.isPlaying == false &&
                     weaponController.lastChargeTriggerTimeStamp > lastChargeTriggerTimeStamp)   //실제 시간보다 더크면 
                {
                    lastChargeTriggerTimeStamp = weaponController.lastChargeTriggerTimeStamp;   //계속 들어오지 못하게 //시간체크
                    if (useProceduralPitchOnLoop == false)   //사용하지 않으면
                    {
                        //현재시간 + 이클립의 길이 
                        endChargeTime = Time.time + chargeSound.length;
                        audioSource.Play();
                    }
                    audioSourceLoop.Play();
                }
                if (useProceduralPitchOnLoop == false) //두개의 사운드 페이드 효과로 충전 표현
                {
                    //듀레이션동안 0에서 1 , 계속 구함
                    float volumeRatio = Mathf.Clamp01((endChargeTime - Time.time - fadeLoopDuration) / fadeLoopDuration);
                    audioSource.volume = volumeRatio;                   //낮은데서 올라감 0 - 1 
                    audioSourceLoop.volume = 1f - volumeRatio;      //1 - 높은데서 내려옴 1 - 0 

                }
                else  //루프사운드의 재생속도로 충전 표현
                {
                    audioSourceLoop.pitch = Mathf.Lerp(1.0f, maxProceduralPitchValue, chargeRatio); //맥스까지 재생속도 올라갈꺼임
                } 
            }
            else
            {
                audioSource.Stop();
                audioSourceLoop.Stop();
            }
        }
    }
}