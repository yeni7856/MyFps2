using System.Linq;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.AI
{
    /// <summary>
    /// �� ������ ����
    /// </summary>
    public class DetectionModule : MonoBehaviour
    {
        #region Variables
        private ActorManager actorManager;      //���ӿ� �����ϴ� Actor���� �����ϴ� Ŭ����
        public UnityAction OnDetectedTarget;     //���� �����ϸ� ��ϵ� �Լ� ȣ��
        public UnityAction OnLostTarget;         //���� ��ġ�� ��ϵ� �Լ� ȣ��

        public GameObject KnownDetectedTarget { get; private set; }
        public bool HadKnownTarget { get; private set; }   
        public bool IsSeeingTarget { get; private set; }

        public Transform detectionSourcePoint;
        public float detectionRange = 20f;                                          //������ �Ÿ�

        public float knownTargetTimeout = 4f;
        private float timeLastSeenTarget = Mathf.NegativeInfinity;

        //attack
        public float attackRange = 10f;                                                 //�� ���� �Ÿ�
        public bool IsTragetInAttackRange { get; private set; } 
        #endregion
        private void Start()
        {
            //����
            actorManager = GameObject.FindObjectOfType<ActorManager>();
        }
        //������
        public void HandleTargetDetection(Actor actor, Collider[] selfCollider)
        {
            if(KnownDetectedTarget && !IsSeeingTarget &&        
                (Time.time - timeLastSeenTarget) > knownTargetTimeout)          
            {
                KnownDetectedTarget = null;              //ó���� ���ؾ���
            }

            float sqrDetectionRange = detectionRange * detectionRange;
            IsSeeingTarget = false;
            float closetSqrdistance = Mathf.Infinity;

            foreach(var otherActor in actorManager.Actors)
            {
                if (otherActor.affiliation != actor.affiliation)             //�����޶���� (����)
                    continue;
                float sqrDistacne = (otherActor.aimPoint.position - detectionSourcePoint.position).sqrMagnitude; //�ܼ��� �Ÿ�����̸� ��Ʈ�����������
                if(sqrDistacne < sqrDetectionRange 
                    && sqrDistacne < closetSqrdistance)                     //�����ðŸ��ȿ� �־���� //�ּҰŸ�
                {
                    RaycastHit[] hits = Physics.RaycastAll(detectionSourcePoint.position,
                        (otherActor.aimPoint.position - detectionSourcePoint.position).normalized,
                        detectionRange, -1, QueryTriggerInteraction.Ignore);
                    RaycastHit cloestHit = new RaycastHit(); 
                    cloestHit.distance = Mathf.Infinity;            //�ִ밪���ϱ�
                    bool foundValidHit = false;

                    foreach(var hit in hits)
                    {
                        //�ּҰŸ�
                        if(hit.distance < cloestHit.distance && selfCollider.Contains(hit.collider) == false)   //�������� ���尡����ã�� //���������������ʴ� �ݶ��̴�
                        {
                            cloestHit = hit;    //�ٽ�����
                            foundValidHit = true;   //��ȿ 
                        }
                    }

                    //���� ã������
                    if(foundValidHit == true)
                    {
                        Actor hitActor = cloestHit.collider.GetComponentInParent<Actor>(); //��ȿ��hit���� Actor ��ü��������
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
                Vector3.Distance(transform.position, KnownDetectedTarget.transform.position) <= attackRange; //Ÿ���� attackRange �ȿ� �־��

            //���� �𸣰� �ִٰ� ���� �߰��� ������ ����
            if(HadKnownTarget == false && KnownDetectedTarget != null)    
            {
                OnDetected();    
            }

            //���� ��� �ֽ��ϰ� �ִٰ� ��ġ�� ���� ����
            if(HadKnownTarget == true && KnownDetectedTarget == null)
            {
                OnLost();
            }
            //������ ���� ����
            HadKnownTarget = KnownDetectedTarget != null;
        }

        //���� �����ϸ� ����
        public void OnDetected()
        {
            OnDetectedTarget?.Invoke();
        }

        //���� ��ġ��
        public void OnLost()
        {
            OnLostTarget?.Invoke();
        }
    }
}