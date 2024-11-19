using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Unity.FPS.AI
{
    /// <summary>
    /// ������ ������ : ���͸��� ���� ����
    /// </summary>
    [System.Serializable]
    public struct RendererIndexData
    {
        public Renderer renderer;
        public int materialIndex;


        public RendererIndexData(Renderer _renderer, int _Index)        //������
        {
            this.renderer = _renderer;
            this.materialIndex = _Index;
        }
    }

    /// <summary>
    /// Enemy �� �����ϴ�Ŭ����
    /// </summary>
    public class EnemyController : MonoBehaviour
    {
        #region Variables
        private Health health;

        //Death
        public GameObject deathVfxPrefab;
        public Transform deathVfxSpawnPostion;

        //Damage
        public UnityAction Damaged;

        //Sfx
        public AudioClip damageSfx;

        //Vfx
        public Material bodyMaterial;                           //�������� �� ���͸���
        [GradientUsage(true)]
        public Gradient OnHitBodyGradient;                //������ ȿ���� �÷� �׶���Ʈ   
        //body Material�� ������ �ִ� ������ ����Ʈ 
        private List<RendererIndexData> bodyRenderer = new List<RendererIndexData>();
        MaterialPropertyBlock bodyFlashMaterialPropertyBlock;

        [SerializeField] private float flashOnHitDuration = 0.5f;
        float lastTimeDamaged = float.NegativeInfinity;   //����������
        bool wasDamagedThisFrmae = false;                   //�̹������� ������ �Ծ�����

        //Patrol
        public NavMeshAgent Agent { get; private set; }
        public PatrolPath PatrolPath { get; set; }
        private int pathDestinationIndex;               //��ǥ ��������Ʈ �ε��� 
        private float pathReachingRadius = 1f;      //��������

        //Detection
        private Actor actor;
        private Collider[] selfColliders;
        public DetectionModule DetectionModule { get; private set; }

        public GameObject KnownDetectedTarget => DetectionModule.KnownDetectedTarget;
        public bool IsSeeingTarget => DetectionModule.IsSeeingTarget;

        public bool HadKnownTarget => DetectionModule.HadKnownTarget;

        public Material eyeColorMaterial;
        [ColorUsage(true, true)] public Color defaultEyeColor;
        [ColorUsage(true, true)] public Color attackEyeColor;

        //Eye Material�� �������ִ� ������ ������
        private RendererIndexData eyeRendererData;
        private MaterialPropertyBlock eyeColorMaterialPropertyBlock;

        public UnityAction OnDetectedTarget;
        public UnityAction OnLostTarget;

        //Attack
        public UnityAction OnAttack;

        private float orientSpeed = 10f;
        public bool IsTargetInAttackRange => DetectionModule.IsTragetInAttackRange;


        public bool swapToNextWeapon = false;
        public float delayAfterWeaponSwap = 0f;
        private float lastTimeWeaponSwapped = Mathf.NegativeInfinity;

        public int currentWeaponIndex;
        private WeaponController currentWeapon;
        private WeaponController[] weapons;

        //EnemyManager
        private EnemyManager enemyManager;
        #endregion

        private void Start()
        {
            //����
            enemyManager = GameObject.FindObjectOfType<EnemyManager>();
            enemyManager.RegisterEnemy(this);               //enemyManager�� ���

            Agent = GetComponent<NavMeshAgent>();
            actor = GetComponent<Actor>();
            selfColliders = GetComponentsInChildren<Collider>();

            var detectionModules = GetComponentsInChildren<DetectionModule>();
            DetectionModule = detectionModules[0];
            DetectionModule.OnDetectedTarget += OnDetected;     //����
            DetectionModule.OnLostTarget += OnLost;

            health = GetComponent<Health>();
            health.OnDamaged += OnDamaged;          
            health.OnDie += OnDie;

            //���� �ʱ�ȭ
            FindAndInitializeAllWeapons();  //����ã��
            var weapon = GetCurrentWeapon();    //��������ã��
            weapon.ShowWeapon(true);    //���⺸���ֱ�

            //body Material �� ������ �ִ� ������ ���� 
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);     //������ ���� ���ϱ� (�迭��)
            foreach (var renderer in renderers)
            {
                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    //Body
                    if (renderer.sharedMaterials[i] == bodyMaterial)        //�ٵ� ��Ʈ�����̶� ������ ã�Ƽ�
                    {
                        bodyRenderer.Add(new RendererIndexData(renderer, i));
                    }

                    //Eye
                    if(renderer.sharedMaterials[i] != eyeColorMaterial)
                    {
                        eyeRendererData = new RendererIndexData(renderer, i);
                    }
                }
            }
            //�ٵ�
            bodyFlashMaterialPropertyBlock = new MaterialPropertyBlock(); //��Ʈ���� �ʱ�ȭ

            //Eye
            if (eyeRendererData.renderer)
            {
                eyeColorMaterialPropertyBlock = new MaterialPropertyBlock();
                eyeColorMaterialPropertyBlock.SetColor("_EmissionColor", defaultEyeColor);
                eyeRendererData.renderer.SetPropertyBlock(eyeColorMaterialPropertyBlock,
                    eyeRendererData.materialIndex);
            }
        }

        private void Update()
        {
            //���ؼ�
            DetectionModule.HandleTargetDetection(actor, selfColliders); //���ڽŰ� ���ڽ����ݶ��̴��� �����ͼ� ������

            //������ ȿ��
            Color currentColor = OnHitBodyGradient.Evaluate((Time.time - lastTimeDamaged) / flashOnHitDuration);
            bodyFlashMaterialPropertyBlock.SetColor("_EmissionColor", currentColor);        //������ �÷�
            foreach (var data in bodyRenderer)
            {
                data.renderer.SetPropertyBlock(bodyFlashMaterialPropertyBlock, data.materialIndex);
            }
            //
            wasDamagedThisFrmae = false;        //�޽��θ���� (�ߺ�ó�� �ȵǰ�)
        }

        void OnDamaged(float damage, GameObject damageSource)
        {
            if (damageSource && !damageSource.GetComponent<EnemyController>())        //�̳ʹ̰�ü ������ �ҽ��κ���
            {
                //��ϵ� �Լ� ȣ��
                Damaged?.Invoke();

                //�������� �ؽð�
                lastTimeDamaged = Time.time;            //������ ���� �ð����� ���� 

                //Sfx
                if (damageSfx && wasDamagedThisFrmae == false)      //������������ üũ
                {
                    AudioUtilty.CreateSfx(damageSfx, this.transform.position, 0f);
                }
                wasDamagedThisFrmae = true;
            }
        }

        void OnDie()
        {
            //EnemyManager ����Ʈ�� ����
            enemyManager.RemoveEnemy(this);

            //����ȿ��
            GameObject effectGo = Instantiate(deathVfxPrefab, deathVfxSpawnPostion.position, Quaternion.identity);
            Destroy(effectGo, 5f);

            //Enemy ų
            Destroy(this.gameObject);
        }

        //Patrol�� �����Ѱ�?
        private bool IsPathVaild()
        {
            return PatrolPath && PatrolPath.wayPoints.Count > 0;
        }

        //���� ����� WayPoint ã��
        void SetPathDestinationToClosestWayPoint()
        {
            if (IsPathVaild() == false)    //��ȿ���������� return;
            {
                pathDestinationIndex = 0;   //���尡���� 0
                return;
            }

            int closestWayPointIndex = 0;
            for (int i = 0; i < PatrolPath.wayPoints.Count; i++)
            {
                float distance = PatrolPath.GetDistanceToWayPoint(transform.position, i);
                float closestDistance = PatrolPath.GetDistanceToWayPoint(transform.position, closestWayPointIndex);
                if (distance < closestDistance)
                {
                    closestWayPointIndex = i;
                }
            }
            pathDestinationIndex = closestWayPointIndex;
        }

        //��ǥ ������ ��ġ  �� ������ 
        public Vector3 GetDestinationOnPath()
        {
            if (IsPathVaild() == false)    //��ȿ���������� return;
            {
                return this.transform.position;
            }

            return PatrolPath.GetPositionWayPoint(pathDestinationIndex);
        }

        //��ǥ ���� ���� - Nav�ý��� �̿�
        public void SetNavDestination(Vector3 destination)
        {
            if (Agent)
            {
                Agent.SetDestination(destination);
            }
        }

        //���� ���� �� ���� ��ǥ���� ���� 
        public void UpdatePathDestination(bool inverseOrder = false)
        {
            if (IsPathVaild() == false)
                return;

            //��������
            float distance = (transform.position - GetDestinationOnPath()).magnitude;   //��ǥ�������� �Ÿ�
            if (distance <= pathReachingRadius)
            {
                pathDestinationIndex = inverseOrder ? (pathDestinationIndex - 1) : (pathDestinationIndex + 1);
                if (pathDestinationIndex < 0)
                {
                    pathDestinationIndex += PatrolPath.wayPoints.Count;
                }
                if (pathDestinationIndex >= PatrolPath.wayPoints.Count)
                {
                    pathDestinationIndex -= PatrolPath.wayPoints.Count;
                }
            }
        }

        //
        public void OrientToward(Vector3 lookPosition)  //�ٶ󺸴� �������� ����
        {
            Vector3 lookDirect = Vector3.ProjectOnPlane(lookPosition - transform.position,Vector3.up).normalized;
            if(lookDirect.sqrMagnitude != 0)
            {
                //Ÿ���� ���� �ٶ󺸵���
                Quaternion targetRotation = Quaternion.LookRotation(lookDirect);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, orientSpeed * Time.deltaTime);
            }
        }

        //�� ������ ȣ��Ǵ� �Լ�
        void OnDetected()
        {
            OnDetectedTarget?.Invoke();

            Debug.Log("========== OnDetected");
            
            if(eyeRendererData.renderer)
            {
                eyeColorMaterialPropertyBlock.SetColor("_EmissionColor", attackEyeColor);
                eyeRendererData.renderer.SetPropertyBlock(eyeColorMaterialPropertyBlock,
                    eyeRendererData.materialIndex);
            }
        }

        //�� �Ҿ�������� ȣ��Ǵ� �Լ�
        void OnLost()
        {
            OnLostTarget?.Invoke();

            Debug.Log("========== OnLost");

            if (eyeRendererData.renderer)
            {
                eyeColorMaterialPropertyBlock.SetColor("_EmissionColor", defaultEyeColor);
                eyeRendererData.renderer.SetPropertyBlock(eyeColorMaterialPropertyBlock,
                    eyeRendererData.materialIndex);
            }
        }
        //������ �ִ� ���� �ʱ�ȭ 
        void FindAndInitializeAllWeapons()
        {
            if(weapons == null)
            {
                weapons = this.GetComponentsInChildren<WeaponController>();

                for(int i = 0; i < weapons.Length; i++)
                {
                    weapons[i].Owner = gameObject;
                }
            }
        }

        //������ �ε����� �ش��ϴ� ���⸦ current �� ����
        void SetCurrentWeapon(int index)
        {
            currentWeaponIndex = index;
            currentWeapon = weapons[currentWeaponIndex];
            if(swapToNextWeapon)
            {
                lastTimeWeaponSwapped = Time.time;
            }
            else
            {
                lastTimeWeaponSwapped = Mathf.NegativeInfinity;
            }
        }

        //���� Current Weapon ã��
        public WeaponController GetCurrentWeapon()
        {
            //�����ʱ�ȭ 
            FindAndInitializeAllWeapons(); //�ѹ� ���É�����x 
            if(currentWeapon == null)
            {
               SetCurrentWeapon(0); //0������ ���� 
            }

            return currentWeapon;
        }

        //������ �ѱ��� ������
        public void OrientWeaponsToward(Vector3 lookPositon)
        {
            for(int i = 0;i < weapons.Length;i++)
            {
                Vector3 weaponForward = (lookPositon - weapons[i].transform.position).normalized;
                weapons[i].transform.forward = weaponForward;   
            }
        }

        //���� - ���ݼ���, ����
        public bool TryAttack(Vector3 targetPosition)
        {
            //���� ��ü�� ������ �ð����� ���� �Ұ�
            if(lastTimeWeaponSwapped + delayAfterWeaponSwap >= Time.time)
            {
                return false;
            }

            //���� Shoot 
            bool didFire = GetCurrentWeapon().HandleShootInputs(false, true, false);
            if (didFire && OnAttack != null)
            {
                OnAttack?.Invoke();

                //�߻� �ѹ� �Ҷ����� ���� ����� ��ü
                if (swapToNextWeapon == true && weapons.Length > 1)
                {
                    int nextWeaponIndex = (currentWeaponIndex + 1)% weapons.Length;
                    SetCurrentWeapon(nextWeaponIndex);
                }
            }

            return true;
        }
    }
}
