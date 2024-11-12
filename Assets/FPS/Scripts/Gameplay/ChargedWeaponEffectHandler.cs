using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ChargedWeaponEffectHandler : MonoBehaviour
    {
        #region Variables
        public GameObject chargingObject;               //�����ϴ� �߻�ü
        public GameObject spiningFrame;                 //�߻�ü�� ���ΰ� �ִ� ������
        public GameObject distOrbitParticePrefab;   //�߻�ü ȸ���ϴ� ����Ʈ

        public MinMaxVector3 scale;                       //�߻�ü ������

        [SerializeField] private Vector3 offset;
        public Transform parentTransform;

        public MinMaxFloat orbitY;                         //����Ʈ ������
        public MinMaxVector3 radius;                    //����Ʈ ������ 

        public MinMaxFloat spiningSpeed;            //ȸ�� ������

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
        private float chargeRatio;                      //���� ������
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
            //��ƼŬ �ý��� 
            diskOrbitParticle = particleInstance.GetComponent<ParticleSystem>();
            velocityOverLifetimeModule = diskOrbitParticle.velocityOverLifetime;

            weaponController = GetComponent<WeaponController>();    
        }

        private void Update()
        {
            //�ѹ��� ��ü ����� 
            if (particleInstance == null)        //���϶��� �����ؼ� ��ü�����
            {
                SpawParticleSystem();
            }
            //��ƼŬ IsWeaponActive �ϸ� Ȱ��ȭ �ƴϸ� ��Ȱ��ȭ 
            diskOrbitParticle.gameObject.SetActive(weaponController.IsWeaponActive);
            //chargeRatio �������� ���� ����Ʈ
            chargeRatio = weaponController.CurrentCharge;

            //disk, frame ȸ��,
            chargingObject.transform.localScale = scale.GetValueFromRatio(chargeRatio);
            if (spiningFrame)
            {
                //y �ุ ���ư��� 
                spiningFrame.transform.localRotation *= Quaternion.Euler(0f,
                    spiningSpeed.GetValueFromRatio(chargeRatio) * Time.deltaTime, 0f);
            }

            //VFX
            //particle
            velocityOverLifetimeModule.orbitalY = orbitY.GetValueFromRatio(chargeRatio);
            diskOrbitParticle.transform.localScale = radius.GetValueFromRatio(chargeRatio);

            //SFX
            if (chargeRatio > 0f)        //��������
            {
                if (audioSourceLoop.isPlaying == false &&
                     weaponController.lastChargeTriggerTimeStamp > lastChargeTriggerTimeStamp)   //���� �ð����� ��ũ�� 
                {
                    lastChargeTriggerTimeStamp = weaponController.lastChargeTriggerTimeStamp;   //��� ������ ���ϰ� //�ð�üũ
                    if (useProceduralPitchOnLoop == false)   //������� ������
                    {
                        //����ð� + ��Ŭ���� ���� 
                        endChargeTime = Time.time + chargeSound.length;
                        audioSource.Play();
                    }
                    audioSourceLoop.Play();
                }
                if (useProceduralPitchOnLoop == false) //�ΰ��� ���� ���̵� ȿ���� ���� ǥ��
                {
                    //�෹�̼ǵ��� 0���� 1 , ��� ����
                    float volumeRatio = Mathf.Clamp01((endChargeTime - Time.time - fadeLoopDuration) / fadeLoopDuration);
                    audioSource.volume = volumeRatio;                   //�������� �ö� 0 - 1 
                    audioSourceLoop.volume = 1f - volumeRatio;      //1 - �������� ������ 1 - 0 

                }
                else  //���������� ����ӵ��� ���� ǥ��
                {
                    audioSourceLoop.pitch = Mathf.Lerp(1.0f, maxProceduralPitchValue, chargeRatio); //�ƽ����� ����ӵ� �ö󰥲���
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