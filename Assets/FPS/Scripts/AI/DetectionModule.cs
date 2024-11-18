using System.Linq;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.AI
{
    /// <summary>
    /// 적 디텍팅 구현
    /// </summary>
    public class DetectionModule : MonoBehaviour
    {
        #region Variables
        private ActorManager actorManager;      //게임에 등장하는 Actor들을 관리하는 클래스
        public UnityAction OnDetectedTarget;     //적을 감지하면 등록된 함수 호출
        public UnityAction OnLostTarget;         //적을 놓치면 등록됨 함수 호출

        public GameObject KnownDetectedTarget { get; private set; }
        public bool HadKnownTarget { get; private set; }   
        public bool IsSeeingTarget { get; private set; }

        public Transform detectionSourcePoint;
        public float detectionRange = 20f;                                          //적감지 거리

        public float knownTargetTimeout = 4f;
        private float timeLastSeenTarget = Mathf.NegativeInfinity;

        //attack
        public float attackRange = 10f;                                                 //적 공격 거리
        public bool IsTragetInAttackRange { get; private set; } 
        #endregion
        private void Start()
        {
            //참조
            actorManager = GameObject.FindObjectOfType<ActorManager>();
        }
        //디텍팅
        public void HandleTargetDetection(Actor actor, Collider[] selfCollider)
        {
            if(KnownDetectedTarget && !IsSeeingTarget &&        
                (Time.time - timeLastSeenTarget) > knownTargetTimeout)          
            {
                KnownDetectedTarget = null;              //처음에 널해야함
            }

            float sqrDetectionRange = detectionRange * detectionRange;
            IsSeeingTarget = false;
            float closetSqrdistance = Mathf.Infinity;

            foreach(var otherActor in actorManager.Actors)
            {
                if (otherActor.affiliation != actor.affiliation)             //나랑달라야함 (적군)
                    continue;
                float sqrDistacne = (otherActor.aimPoint.position - detectionSourcePoint.position).sqrMagnitude; //단순히 거리계산이면 루트계산하지않음
                if(sqrDistacne < sqrDetectionRange 
                    && sqrDistacne < closetSqrdistance)                     //디텍팅거리안에 있어야함 //최소거리
                {
                    RaycastHit[] hits = Physics.RaycastAll(detectionSourcePoint.position,
                        (otherActor.aimPoint.position - detectionSourcePoint.position).normalized,
                        detectionRange, -1, QueryTriggerInteraction.Ignore);
                    RaycastHit cloestHit = new RaycastHit(); 
                    cloestHit.distance = Mathf.Infinity;            //최대값구하기
                    bool foundValidHit = false;

                    foreach(var hit in hits)
                    {
                        //최소거리
                        if(hit.distance < cloestHit.distance && selfCollider.Contains(hit.collider) == false)   //여러개중 가장가까운거찾음 //내가가지고있지않는 콜라이더
                        {
                            cloestHit = hit;    //다시저장
                            foundValidHit = true;   //유효 
                        }
                    }

                    //적을 찾았으면
                    if(foundValidHit == true)
                    {
                        Actor hitActor = cloestHit.collider.GetComponentInParent<Actor>(); //유효한hit에서 Actor 객체가져오기
                        if(hitActor == otherActor)
                        {
                            IsSeeingTarget = true;
                            closetSqrdistance = sqrDistacne;

                            timeLastSeenTarget = Time.time; 
                            KnownDetectedTarget = otherActor.aimPoint.gameObject;
                        }
                    }
                }
            }

            //attack Range check
            IsTragetInAttackRange = (KnownDetectedTarget != null) &&
                Vector3.Distance(transform.position, KnownDetectedTarget.transform.position) <= attackRange; //타겟이 attackRange 안에 있어야

            //적을 모르고 있다가 적을 발견한 순간에 실행
            if(HadKnownTarget == false && KnownDetectedTarget != null)    
            {
                OnDetected();    
            }

            //적을 계속 주시하고 있다가 놓치는 순간 실행
            if(HadKnownTarget == true && KnownDetectedTarget == null)
            {
                OnLost();
            }
            //디텍팅 상태 저장
            HadKnownTarget = KnownDetectedTarget != null;
        }

        //적을 감지하면 실행
        public void OnDetected()
        {
            OnDetectedTarget?.Invoke();
        }

        //적을 놓치면
        public void OnLost()
        {
            OnLostTarget?.Invoke();
        }
    }
}