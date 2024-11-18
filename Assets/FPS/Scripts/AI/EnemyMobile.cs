using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.AI
{
    /// <summary>
    /// Enemy 상태 enum 
    /// </summary>
    public enum AIState
    {
        Patrol,
        Follow,
        Attack
    }
    /// <summary>
    /// 이동하는 Enemy의 상태를 구현하는 클래스  
    /// </summary>
    public class EnemyMobile : MonoBehaviour
    {
        #region Variables
        public Animator animator;
        private EnemyController enemyController;

        public AIState AiState { get; private set; }

        //Audio
        //이동
        public AudioClip movementSound;
        public MinMaxFloat pitchMoveSpeed;          //재생속도

        private AudioSource audioSource;

        //데미지 - 이펙트
        public ParticleSystem[] randomHitSparks;

        //Detected
        public ParticleSystem[] detectedVfx;
        public AudioClip detectedAudio; 

        //Animation parameter (상수로)
        const string k_AnimAttackParameter = "Attack";
        const string k_AnimMoveSpeedParameter = "MoveSpeed";
        const string k_AnimAlertedParameter = "Alerted";
        const string k_AnimOnDamagedParameter = "OnDamaged";
        const string k_AnimDeathParameter = "Death";
        #endregion

        private void Start()
        {
            //참조
            audioSource = GetComponent<AudioSource>();
            audioSource.clip = movementSound;
            audioSource.Play();

            enemyController = GetComponent<EnemyController>();
            enemyController.Damaged += OnDamaged;                       //등록
            enemyController.OnDetectedTarget += OnDetected;         
            enemyController.OnLostTarget += OnLost;


            //초기화
            AiState = AIState.Patrol;
        }

        private void Update()
        {
            //상태구현
            UpdateCurrentAiState();

            //상태변경
            UpdateAiStateTransition();

            //속도에 따른 애니/사운드 효과
            float moveSpeed = enemyController.Agent.velocity.magnitude;
            animator.SetFloat(k_AnimMoveSpeedParameter, moveSpeed);         //애니
            audioSource.pitch = pitchMoveSpeed.GetValueFromRatio(moveSpeed / enemyController.Agent.speed);

        }

        //상태에 따른 Enemy 구현
        void UpdateCurrentAiState()
        {
            switch (AiState)
            {
                case AIState.Patrol:
                    enemyController.UpdatePathDestination(true);    //목표지점설정
                    enemyController.SetNavDestination(enemyController.GetDestinationOnPath());
                    break;
                case AIState.Follow:
                    //타겟을 향해서
                    enemyController.SetNavDestination(enemyController.KnownDetectedTarget.transform.position);
                    enemyController.OrientToward(enemyController.KnownDetectedTarget.transform.position);
                    enemyController.OrientWeaponsToward(enemyController.KnownDetectedTarget.transform.position);
                    break;
                case AIState.Attack:
                    enemyController.OrientToward(enemyController.KnownDetectedTarget.transform.position);
                    enemyController.OrientWeaponsToward(enemyController.KnownDetectedTarget.transform.position);
                    enemyController.TryAttack(enemyController.KnownDetectedTarget.transform.position);
                    break;
            }
        }

        //상태 변경에 따른 구현
       void UpdateAiStateTransition()
        {
            switch (AiState)
            {
                case AIState.Patrol:
                    break;
                case AIState.Follow:
                    if (enemyController.IsSeeingTarget && enemyController.IsTargetInAttackRange)
                    {
                        AiState = AIState.Attack;
                        enemyController.SetNavDestination(transform.position);  //정지
                    }
                    break;
                case AIState.Attack:
                    if(enemyController.IsTargetInAttackRange == false)
                    {
                        AiState = AIState.Follow;
                    }
                    break;
            }
        }

        void OnDamaged()
        {
            //스파크 파티클 - 랜덤하게 하나 선택해서 플레이
            if (randomHitSparks.Length > 0)
            {
                int randNum = Random.Range(0, randomHitSparks.Length);
                randomHitSparks[randNum].Play();
            }
            //데미지 애니
            animator.SetTrigger(k_AnimOnDamagedParameter);
        }

        //타겟 찾았을때
        void OnDetected()
        {
            //상태 변경
            AiState = AIState.Follow;

            //Vfx
            for(int i = 0; i < detectedVfx.Length; i++)
            {
                detectedVfx[i].Play();      
            }
            //Sfx
            if (detectedAudio)
            {
                AudioUtilty.CreateSfx(detectedAudio, this.transform.position, 1f);
            }
            //anim
            animator.SetBool(k_AnimAlertedParameter, true);
        }

        //타겟 잃어버렸을때
        void OnLost()
        {
            //Vfx
            for (int i = 0; i < detectedVfx.Length; i++)
            {
                detectedVfx[i].Stop();
            }

            //anim
            animator.SetBool(k_AnimAlertedParameter, false);
        }
    }
}