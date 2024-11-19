using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.AI
{
    /// <summary>
    /// Enemy ���� enum 
    /// </summary>
    public enum AIState
    {
        Patrol,
        Follow,
        Attack
    }
    /// <summary>
    /// �̵��ϴ� Enemy�� ���¸� �����ϴ� Ŭ����  
    /// </summary>
    public class EnemyMobile : MonoBehaviour
    {
        #region Variables
        public Animator animator;
        private EnemyController enemyController;

        public AIState AiState { get; private set; }

        //Audio
        //�̵�
        public AudioClip movementSound;
        public MinMaxFloat pitchMoveSpeed;          //����ӵ�

        private AudioSource audioSource;

        //������ - ����Ʈ
        public ParticleSystem[] randomHitSparks;

        //Detected
        public ParticleSystem[] detectedVfx;
        public AudioClip detectedAudio;

        //Attack
        [Range(0f, 1f)] 
        public float attackSkipDistacneRatio = 0.5f;

        //Animation parameter (�����)
        const string k_AnimAttackParameter = "Attack";
        const string k_AnimMoveSpeedParameter = "MoveSpeed";
        const string k_AnimAlertedParameter = "Alerted";
        const string k_AnimOnDamagedParameter = "OnDamaged";
        const string k_AnimDeathParameter = "Death";
        #endregion

        private void Start()
        {
            //����
            audioSource = GetComponent<AudioSource>();
            audioSource.clip = movementSound;
            audioSource.Play();

            enemyController = GetComponent<EnemyController>();
            enemyController.Damaged += OnDamaged;                       //���
            enemyController.OnDetectedTarget += OnDetected;         
            enemyController.OnLostTarget += OnLost;
            enemyController.OnAttack += Attacked;


            //�ʱ�ȭ
            AiState = AIState.Patrol;
        }

        private void Update()
        {
            //���±���
            UpdateCurrentAiState();

            //���º���
            UpdateAiStateTransition();

            //�ӵ��� ���� �ִ�/���� ȿ��
            float moveSpeed = enemyController.Agent.velocity.magnitude;
            animator.SetFloat(k_AnimMoveSpeedParameter, moveSpeed);         //�ִ�
            audioSource.pitch = pitchMoveSpeed.GetValueFromRatio(moveSpeed / enemyController.Agent.speed);

        }

        //���¿� ���� Enemy ����
        void UpdateCurrentAiState()
        {
            switch (AiState)
            {
                case AIState.Patrol:
                    enemyController.UpdatePathDestination(true);    //��ǥ��������
                    enemyController.SetNavDestination(enemyController.GetDestinationOnPath());
                    break;
                case AIState.Follow:
                    //Ÿ���� ���ؼ�
                    enemyController.SetNavDestination(enemyController.KnownDetectedTarget.transform.position);
                    enemyController.OrientToward(enemyController.KnownDetectedTarget.transform.position);
                    enemyController.OrientWeaponsToward(enemyController.KnownDetectedTarget.transform.position);
                    break;
                case AIState.Attack:
                    //�����Ÿ� ������ �̵��ϸ鼭 ����
                    float distacne = Vector3.Distance(enemyController.KnownDetectedTarget.transform.transform.position,
                        enemyController.DetectionModule.detectionSourcePoint.position);
                    if(distacne >= enemyController.DetectionModule.attackRange * attackSkipDistacneRatio) //5�̻��̸�
                    {
                        //Ÿ�� ��ġ
                        enemyController.SetNavDestination(enemyController.KnownDetectedTarget.transform.transform.position);
                    }
                    else
                    {
                        enemyController.SetNavDestination(transform.position);          //���ڸ��� ���� 
                    }

                    enemyController.OrientToward(enemyController.KnownDetectedTarget.transform.position);
                    enemyController.OrientWeaponsToward(enemyController.KnownDetectedTarget.transform.position);
                    enemyController.TryAttack(enemyController.KnownDetectedTarget.transform.position);
                    break;
            }
        }

        //���� ���濡 ���� ����
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
                        enemyController.SetNavDestination(transform.position);  //����
                    }
                    /*else if (enemyController.IsTargetInAttackRange == false)
                    {
                        AiState = AIState.Patrol;
                    }*/
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
            //����ũ ��ƼŬ - �����ϰ� �ϳ� �����ؼ� �÷���
            if (randomHitSparks.Length > 0)
            {
                int randNum = Random.Range(0, randomHitSparks.Length);
                randomHitSparks[randNum].Play();
            }
            //������ �ִ�
            animator.SetTrigger(k_AnimOnDamagedParameter);
        }

        //Ÿ�� ã������
        void OnDetected()
        {

            //���� ����
            if (AiState == AIState.Patrol)
            {
                AiState = AIState.Follow;
            }

            //Vfx
            for (int i = 0; i < detectedVfx.Length; i++)
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

        //Ÿ�� �Ҿ��������
        void OnLost()
        {
            //���� ����
            if(AiState == AIState.Follow || AiState == AIState.Attack)
            {
                AiState = AIState.Patrol;
            }

            //Vfx
            for (int i = 0; i < detectedVfx.Length; i++)
            {
                detectedVfx[i].Stop();
            }

            //anim
            animator.SetBool(k_AnimAlertedParameter, false);
        }
        void Attacked()
        {
            //�ִϸ��̼�
            animator.SetTrigger(k_AnimAttackParameter);
        }
    }
}