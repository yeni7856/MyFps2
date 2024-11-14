using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UI;

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
        public  NavMeshAgent Agent { get; private set; }
        public PatrolPath PatrolPath {  get; set; }
        private int pathDestinationIndex;               //목표 웨이포인트 인덱스 
        private float pathReachingRadius = 1f;      //도착판정

        #endregion

        private void Start()
        {
            //참조
            Agent = GetComponent<NavMeshAgent>();
            health = GetComponent<Health>();
            health.OnDamaged += OnDamaged;
            health.OnDie += OnDie;

            //body Material 을 가지고 있는 렌더러 정보 
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);     //렌더러 정보 구하기 (배열로)
            foreach(var renderer in renderers)
            {
                for(int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    if (renderer.sharedMaterials[i] == bodyMaterial)        //바디 메트리얼이랑 같은거 찾아서
                    {
                        bodyRenderer.Add(new RendererIndexData(renderer, i));   
                    }
                }
            }
            bodyFlashMaterialPropertyBlock = new MaterialPropertyBlock(); //메트리얼 초기화
        }

        private void Update()
        {
            //데미지 효과
            Color currentColor = OnHitBodyGradient.Evaluate((Time.time - lastTimeDamaged) / flashOnHitDuration);
            bodyFlashMaterialPropertyBlock.SetColor("_EmissionColor", currentColor);        //빛나는 컬러
            foreach(var data in bodyRenderer)
            {
                data.renderer.SetPropertyBlock(bodyFlashMaterialPropertyBlock, data.materialIndex);
            }
            //
            wasDamagedThisFrmae = false;        //펄스로만들고 (중복처리 안되게)
        }

        void OnDamaged(float damage, GameObject damageSource)
        {
            if(damageSource && !damageSource.GetComponent<EnemyController>())        //이너미객체 데미지 소스로부터
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
            //폭발효과
            GameObject effectGo = Instantiate(deathVfxPrefab, deathVfxSpawnPostion.position, Quaternion.identity);
            Destroy(effectGo,5f);

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
                if(distance < closestDistance)
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
            if(distance <= pathReachingRadius)
            {
                pathDestinationIndex = inverseOrder ? (pathDestinationIndex- 1) : (pathDestinationIndex+ 1);     
                if(pathDestinationIndex < 0)
                {
                    pathDestinationIndex += PatrolPath.wayPoints.Count;
                }
                if(pathDestinationIndex >= PatrolPath.wayPoints.Count)
                {
                    pathDestinationIndex -= PatrolPath.wayPoints.Count;
                }
            }
        }
    }
}
