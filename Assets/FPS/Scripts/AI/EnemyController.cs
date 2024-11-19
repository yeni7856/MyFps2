using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Unity.FPS.AI
{
    /// <summary>
    /// 렌더럴 데이터 : 메터리얼 정보 저장
    /// </summary>
    [System.Serializable]
    public struct RendererIndexData
    {
        public Renderer renderer;
        public int materialIndex;


        public RendererIndexData(Renderer _renderer, int _Index)        //값저장
        {
            this.renderer = _renderer;
            this.materialIndex = _Index;
        }
    }

    /// <summary>
    /// Enemy 를 관리하는클래스
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
        public Material bodyMaterial;                           //데미지를 줄 메터리얼
        [GradientUsage(true)]
        public Gradient OnHitBodyGradient;                //데미지 효과를 컬러 그라디언트   
        //body Material을 가지고 있는 렌더러 리스트 
        private List<RendererIndexData> bodyRenderer = new List<RendererIndexData>();
        MaterialPropertyBlock bodyFlashMaterialPropertyBlock;

        [SerializeField] private float flashOnHitDuration = 0.5f;
        float lastTimeDamaged = float.NegativeInfinity;   //가장작은값
        bool wasDamagedThisFrmae = false;                   //이번프레임 데미지 입었는지

        //Patrol
        public NavMeshAgent Agent { get; private set; }
        public PatrolPath PatrolPath { get; set; }
        private int pathDestinationIndex;               //목표 웨이포인트 인덱스 
        private float pathReachingRadius = 1f;      //도착판정

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

        //Eye Material을 가지고있는 렌더러 데이터
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
            //참조
            enemyManager = GameObject.FindObjectOfType<EnemyManager>();
            enemyManager.RegisterEnemy(this);               //enemyManager에 등록

            Agent = GetComponent<NavMeshAgent>();
            actor = GetComponent<Actor>();
            selfColliders = GetComponentsInChildren<Collider>();

            var detectionModules = GetComponentsInChildren<DetectionModule>();
            DetectionModule = detectionModules[0];
            DetectionModule.OnDetectedTarget += OnDetected;     //저장
            DetectionModule.OnLostTarget += OnLost;

            health = GetComponent<Health>();
            health.OnDamaged += OnDamaged;          
            health.OnDie += OnDie;

            //무기 초기화
            FindAndInitializeAllWeapons();  //무기찾고
            var weapon = GetCurrentWeapon();    //현재위폰찾고
            weapon.ShowWeapon(true);    //무기보여주기

            //body Material 을 가지고 있는 렌더러 정보 
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);     //렌더러 정보 구하기 (배열로)
            foreach (var renderer in renderers)
            {
                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    //Body
                    if (renderer.sharedMaterials[i] == bodyMaterial)        //바디 메트리얼이랑 같은거 찾아서
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
            //바디
            bodyFlashMaterialPropertyBlock = new MaterialPropertyBlock(); //메트리얼 초기화

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
            //디텍션
            DetectionModule.HandleTargetDetection(actor, selfColliders); //나자신과 나자신의콜라이더를 가져와서 디텍팅

            //데미지 효과
            Color currentColor = OnHitBodyGradient.Evaluate((Time.time - lastTimeDamaged) / flashOnHitDuration);
            bodyFlashMaterialPropertyBlock.SetColor("_EmissionColor", currentColor);        //빛나는 컬러
            foreach (var data in bodyRenderer)
            {
                data.renderer.SetPropertyBlock(bodyFlashMaterialPropertyBlock, data.materialIndex);
            }
            //
            wasDamagedThisFrmae = false;        //펄스로만들고 (중복처리 안되게)
        }

        void OnDamaged(float damage, GameObject damageSource)
        {
            if (damageSource && !damageSource.GetComponent<EnemyController>())        //이너미객체 데미지 소스로부터
            {
                //등록된 함수 호출
                Damaged?.Invoke();

                //데미지를 준시간
                lastTimeDamaged = Time.time;            //데미지 입은 시간부터 시작 

                //Sfx
                if (damageSfx && wasDamagedThisFrmae == false)      //데미지입은거 체크
                {
                    AudioUtilty.CreateSfx(damageSfx, this.transform.position, 0f);
                }
                wasDamagedThisFrmae = true;
            }
        }

        void OnDie()
        {
            //EnemyManager 리스트에 제거
            enemyManager.RemoveEnemy(this);

            //폭발효과
            GameObject effectGo = Instantiate(deathVfxPrefab, deathVfxSpawnPostion.position, Quaternion.identity);
            Destroy(effectGo, 5f);

            //Enemy 킬
            Destroy(this.gameObject);
        }

        //Patrol이 가능한가?
        private bool IsPathVaild()
        {
            return PatrolPath && PatrolPath.wayPoints.Count > 0;
        }

        //가장 가까운 WayPoint 찾기
        void SetPathDestinationToClosestWayPoint()
        {
            if (IsPathVaild() == false)    //유효하지않으면 return;
            {
                pathDestinationIndex = 0;   //가장가까운거 0
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

        //목표 지점의 위치  값 얻어오기 
        public Vector3 GetDestinationOnPath()
        {
            if (IsPathVaild() == false)    //유효하지않으면 return;
            {
                return this.transform.position;
            }

            return PatrolPath.GetPositionWayPoint(pathDestinationIndex);
        }

        //목표 지점 설정 - Nav시스템 이용
        public void SetNavDestination(Vector3 destination)
        {
            if (Agent)
            {
                Agent.SetDestination(destination);
            }
        }

        //도착 판정 후 다음 목표지점 설정 
        public void UpdatePathDestination(bool inverseOrder = false)
        {
            if (IsPathVaild() == false)
                return;

            //도착판정
            float distance = (transform.position - GetDestinationOnPath()).magnitude;   //목표지점까지 거리
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
        public void OrientToward(Vector3 lookPosition)  //바라보는 방향으로 변경
        {
            Vector3 lookDirect = Vector3.ProjectOnPlane(lookPosition - transform.position,Vector3.up).normalized;
            if(lookDirect.sqrMagnitude != 0)
            {
                //타겟을 향해 바라보도록
                Quaternion targetRotation = Quaternion.LookRotation(lookDirect);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, orientSpeed * Time.deltaTime);
            }
        }

        //적 감지시 호출되는 함수
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

        //적 잃어버렸을때 호출되는 함수
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
        //가지고 있는 무기 초기화 
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

        //지정한 인덱스에 해당하는 무기를 current 로 지정
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

        //현재 Current Weapon 찾기
        public WeaponController GetCurrentWeapon()
        {
            //무기초기화 
            FindAndInitializeAllWeapons(); //한번 셋팅됬으면x 
            if(currentWeapon == null)
            {
               SetCurrentWeapon(0); //0번으로 셋팅 
            }

            return currentWeapon;
        }

        //적에게 총구를 돌리기
        public void OrientWeaponsToward(Vector3 lookPositon)
        {
            for(int i = 0;i < weapons.Length;i++)
            {
                Vector3 weaponForward = (lookPositon - weapons[i].transform.position).normalized;
                weapons[i].transform.forward = weaponForward;   
            }
        }

        //공격 - 공격성공, 실패
        public bool TryAttack(Vector3 targetPosition)
        {
            //무기 교체시 딜레이 시간동안 공격 불가
            if(lastTimeWeaponSwapped + delayAfterWeaponSwap >= Time.time)
            {
                return false;
            }

            //무기 Shoot 
            bool didFire = GetCurrentWeapon().HandleShootInputs(false, true, false);
            if (didFire && OnAttack != null)
            {
                OnAttack?.Invoke();

                //발사 한번 할때마다 다음 무기로 교체
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
