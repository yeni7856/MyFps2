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
        public  NavMeshAgent Agent { get; private set; }
        public PatrolPath PatrolPath {  get; set; }
        private int pathDestinationIndex;               //��ǥ ��������Ʈ �ε��� 
        private float pathReachingRadius = 1f;      //��������

        #endregion

        private void Start()
        {
            //����
            Agent = GetComponent<NavMeshAgent>();
            health = GetComponent<Health>();
            health.OnDamaged += OnDamaged;
            health.OnDie += OnDie;

            //body Material �� ������ �ִ� ������ ���� 
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);     //������ ���� ���ϱ� (�迭��)
            foreach(var renderer in renderers)
            {
                for(int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    if (renderer.sharedMaterials[i] == bodyMaterial)        //�ٵ� ��Ʈ�����̶� ������ ã�Ƽ�
                    {
                        bodyRenderer.Add(new RendererIndexData(renderer, i));   
                    }
                }
            }
            bodyFlashMaterialPropertyBlock = new MaterialPropertyBlock(); //��Ʈ���� �ʱ�ȭ
        }

        private void Update()
        {
            //������ ȿ��
            Color currentColor = OnHitBodyGradient.Evaluate((Time.time - lastTimeDamaged) / flashOnHitDuration);
            bodyFlashMaterialPropertyBlock.SetColor("_EmissionColor", currentColor);        //������ �÷�
            foreach(var data in bodyRenderer)
            {
                data.renderer.SetPropertyBlock(bodyFlashMaterialPropertyBlock, data.materialIndex);
            }
            //
            wasDamagedThisFrmae = false;        //�޽��θ���� (�ߺ�ó�� �ȵǰ�)
        }

        void OnDamaged(float damage, GameObject damageSource)
        {
            if(damageSource && !damageSource.GetComponent<EnemyController>())        //�̳ʹ̰�ü ������ �ҽ��κ���
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
            //����ȿ��
            GameObject effectGo = Instantiate(deathVfxPrefab, deathVfxSpawnPostion.position, Quaternion.identity);
            Destroy(effectGo,5f);

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
                if(distance < closestDistance)
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
