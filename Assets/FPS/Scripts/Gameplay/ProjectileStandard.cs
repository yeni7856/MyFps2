using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    /// 발사체 표준형
    /// </summary>
    public class ProjectileStandard : ProjectileBase
    {
        #region Variables
        //생성
        private ProjectileBase projectileBase;
        private float maxLifeTime = 5f;

        //이동 
        [SerializeField] private float speed = 300f;      //스피드
        [SerializeField] private float gravity = 0f;        //중력
        public Transform root;
        public Transform tip;                        //팁 해드 부분
        private Vector3 velocity;                   //백터 속도
        private Vector3 lastRootPosition;      //루트의 마지막 포지션
        private float shootTime;
        [SerializeField] private float damage = 15f;        //데미지
        [SerializeField] private float lifeTime = 2f;           //라이프 타임
        private float spwanTime;

        //충돌
        private float radius = 0.01f;                          //충돌 검사하는 구체의 반경
        public LayerMask hittableLayers = -1;          //Hit가 가능한 Layer
        private List<Collider> ignoredColliders;      //Hit 판정시 무시하는 충돌체 리스트   

        //충돌연출
        public GameObject impackVfxPrefab;                    //타격 효과 이펙트
        [SerializeField] private float impactVfxLifeTime = 5f;
        private float impactVfxSpwanOffset = 0.1f;

        public AudioClip impactSfxClip;                     //타격 효과음

        #endregion


        private void OnEnable()
        {
            projectileBase = GetComponent<ProjectileBase>();
            projectileBase.OnShoot += OnShoot;

            Destroy(gameObject, maxLifeTime);       //맞추지 않으면 지워져야함 
        }


        //슛 값 설정
        new void OnShoot()
        {
            velocity = transform.forward * speed;
            transform.position += projectileBase.InheritedMuzzleVelocity * Time.deltaTime;   //현재프레임의 총구속도
            
            lastRootPosition = root.position;

            //무기 충돌 리스트 생성 - projectil을 발사하는 자신의 모든 충돌체를 가져와서 등록
            ignoredColliders = new List<Collider>();
            Collider[] ownercolliders =
                projectileBase.Owner.GetComponentsInChildren<Collider>(); //오너에있는 모든 콜라이더에 와서 무시 콜라이더로
            ignoredColliders.AddRange(ownercolliders);  //배열 한번에 추가 


            //프로젝타일이 벽을 뚫고 날아가는 버그 수정 (총구가 벽넘어로 넘어가지않게) projectileBase.Owner : 무기주인
            //카메라 와 머즐사이에 벽(hit) 체크 해야함 거기에 있으면 나가지 벽을쏘게 만들어야함
            PlayerWeaponsManager weaponsManager = projectileBase.Owner.GetComponent<PlayerWeaponsManager>();
            if(weaponsManager != null )
            {
                //머즐에위치에 생성된 InitialPosition 포지션을 가지고 - 위폰매니저에 카메라 포지션
                // : 카메라 포지션부터 머즐 위치까지 백터 구할수있음
                Vector3 cameraToMuzzle = projectileBase.InitialPosition - weaponsManager.weaponCamera.transform.position;
               
                //충돌체 확인 //충돌체가 있으면 무조건 맞는다
                //(위치는 카메라방향부터 카메라 머즐에 normalized 카메라머즐에 magnitude, hit 레이어, 커리 트리거 
                if (Physics.Raycast(weaponsManager.weaponCamera.transform.position, cameraToMuzzle.normalized,
                    out RaycastHit hit, cameraToMuzzle.magnitude, hittableLayers, QueryTriggerInteraction.Collide))
                {
                    OnHit(hit.point, hit.normal, hit.collider);
                }
            }

        }
        private void Update()
        {
            //이동
            transform.position += velocity * Time.deltaTime;        //포지션 이동

            //중력
            if(gravity > 0f)
            {
                velocity += Vector3.down * gravity * Time.deltaTime;
            }

            //충돌
            RaycastHit cloestHit = new RaycastHit();    //가장 가까운놈
            cloestHit.distance = Mathf.Infinity;            //최소값
            bool foundHit = false;                               //hit한 충돌체를 찾았는지 여부
   
            //Sphere Cast
            //실제거리는 Tip 앞부터 충돌체 검사 
            Vector3 displacementSinceLastFrame = tip.position - lastRootPosition;  //마지막거리 로부터 현재로이동한 크기
            RaycastHit[] hits = Physics.SphereCastAll(lastRootPosition, radius,
                displacementSinceLastFrame.normalized, displacementSinceLastFrame.magnitude,
                hittableLayers, QueryTriggerInteraction.Collide);   //히트한 레이어 리스트

            foreach(var hit in hits)        //최소거리구하기
            {
                if(IsHitValid(hit) && hit.distance < cloestHit.distance)    //유효한 히트인지 하고 거리검사하기
                {
                    foundHit = true;      //트루
                    cloestHit = hit;        //가장가까운 히트 찾으면
                }
            }
            //hit 한 충돌체를 찾으면
            if(foundHit)
            {
                if(cloestHit.distance <= 0f)        // 마이너스가 되면 루트가 지나감
                {
                    cloestHit.point = root.position;        //지나가지 않게 루트 포지션으로
                    cloestHit.normal = -transform.forward;  //노말방향도 마이너스로
                }
                OnHit(cloestHit.point, cloestHit.normal, cloestHit.collider);        //히트 구현 
            }

            lastRootPosition = root.position;           
        }

        //hit가 유효한지 판정
        bool IsHitValid(RaycastHit hit)
        {
            //IgnoreHitDectection 컴포넌트를 가진 콜라이더 무시
            if (hit.collider.GetComponent<IgnoreHitDectection>())   //널이아니면
            {
                return false;   //유효하지 않음
            }
            //히트에 콜라이더가 무시 리스트에 들어가면 IgnoreHitDectection에 포함된 콜라이더 무시
            if (ignoredColliders != null && ignoredColliders.Contains(hit.collider)) 
            {
                return false;
            }

            //Trigger collider 체크되어있으면 무시 데미지없는 애들 무시
            if (hit.collider.isTrigger && hit.collider.GetComponent<Damageable>() == null) 
            {
                return false;
            }
            return true;
        }

        //히트 구현, 데미지, Vfx, Sfx
        void OnHit(Vector3 point, Vector3 normal, Collider collider)
        {
            //Vfx
            //충돌체 바로위에 offset사용해서 약간위에서 스폰위치를 노말방향으로
            if (impackVfxPrefab)
            {
                GameObject impactObject = Instantiate(impackVfxPrefab, point + (normal * impactVfxSpwanOffset), Quaternion.LookRotation(normal));
                if(impactVfxLifeTime > 0f)
                {
                    Destroy(impactObject, impactVfxLifeTime);
                }
            }

            //Sfx
            if(impactSfxClip)
            {
                //충돌위치에 게임오브젝트를 생성하고 AudioSource 컴포넌트를 추가해서 지정된 클립을 플레이
                AudioUtilty.CreateSfx(impactSfxClip, point, 1f, 3f);
                Debug.Log("Play impactSfxClip");
            }


            //발사체 킬
            Destroy(this.gameObject);
        }
    }
}


