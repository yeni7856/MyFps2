using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    /// �߻�ü ǥ����
    /// </summary>
    public class ProjectileStandard : ProjectileBase
    {
        #region Variables
        //����
        private ProjectileBase projectileBase;
        private float maxLifeTime = 5f;

        //�̵� 
        [SerializeField] private float speed = 300f;      //���ǵ�
        [SerializeField] private float gravity = 0f;        //�߷�
        public Transform root;
        public Transform tip;                        //�� �ص� �κ�
        private Vector3 velocity;                   //���� �ӵ�
        private Vector3 lastRootPosition;      //��Ʈ�� ������ ������
        private float shootTime;
        [SerializeField] private float damage = 15f;        //������
        [SerializeField] private float lifeTime = 2f;           //������ Ÿ��
        private float spwanTime;

        //�浹
        private float radius = 0.01f;                          //�浹 �˻��ϴ� ��ü�� �ݰ�
        public LayerMask hittableLayers = -1;          //Hit�� ������ Layer
        private List<Collider> ignoredColliders;      //Hit ������ �����ϴ� �浹ü ����Ʈ   

        //�浹����
        public GameObject impackVfxPrefab;                    //Ÿ�� ȿ�� ����Ʈ
        [SerializeField] private float impactVfxLifeTime = 5f;
        private float impactVfxSpwanOffset = 0.1f;

        public AudioClip impactSfxClip;                     //Ÿ�� ȿ����

        #endregion


        private void OnEnable()
        {
            projectileBase = GetComponent<ProjectileBase>();
            projectileBase.OnShoot += OnShoot;

            Destroy(gameObject, maxLifeTime);       //������ ������ ���������� 
        }


        //�� �� ����
        new void OnShoot()
        {
            velocity = transform.forward * speed;
            transform.position += projectileBase.InheritedMuzzleVelocity * Time.deltaTime;   //������������ �ѱ��ӵ�
            
            lastRootPosition = root.position;

            //���� �浹 ����Ʈ ���� - projectil�� �߻��ϴ� �ڽ��� ��� �浹ü�� �����ͼ� ���
            ignoredColliders = new List<Collider>();
            Collider[] ownercolliders =
                projectileBase.Owner.GetComponentsInChildren<Collider>(); //���ʿ��ִ� ��� �ݶ��̴��� �ͼ� ���� �ݶ��̴���
            ignoredColliders.AddRange(ownercolliders);  //�迭 �ѹ��� �߰� 


            //������Ÿ���� ���� �հ� ���ư��� ���� ���� (�ѱ��� ���Ѿ�� �Ѿ���ʰ�) projectileBase.Owner : ��������
            //ī�޶� �� ������̿� ��(hit) üũ �ؾ��� �ű⿡ ������ ������ ������� ��������
            PlayerWeaponsManager weaponsManager = projectileBase.Owner.GetComponent<PlayerWeaponsManager>();
            if(weaponsManager != null )
            {
                //������ġ�� ������ InitialPosition �������� ������ - �����Ŵ����� ī�޶� ������
                // : ī�޶� �����Ǻ��� ���� ��ġ���� ���� ���Ҽ�����
                Vector3 cameraToMuzzle = projectileBase.InitialPosition - weaponsManager.weaponCamera.transform.position;
               
                //�浹ü Ȯ�� //�浹ü�� ������ ������ �´´�
                //(��ġ�� ī�޶������� ī�޶� ���� normalized ī�޶���� magnitude, hit ���̾�, Ŀ�� Ʈ���� 
                if (Physics.Raycast(weaponsManager.weaponCamera.transform.position, cameraToMuzzle.normalized,
                    out RaycastHit hit, cameraToMuzzle.magnitude, hittableLayers, QueryTriggerInteraction.Collide))
                {
                    OnHit(hit.point, hit.normal, hit.collider);
                }
            }

        }
        private void Update()
        {
            //�̵�
            transform.position += velocity * Time.deltaTime;        //������ �̵�

            //�߷�
            if(gravity > 0f)
            {
                velocity += Vector3.down * gravity * Time.deltaTime;
            }

            //�浹
            RaycastHit cloestHit = new RaycastHit();    //���� ������
            cloestHit.distance = Mathf.Infinity;            //�ּҰ�
            bool foundHit = false;                               //hit�� �浹ü�� ã�Ҵ��� ����
   
            //Sphere Cast
            //�����Ÿ��� Tip �պ��� �浹ü �˻� 
            Vector3 displacementSinceLastFrame = tip.position - lastRootPosition;  //�������Ÿ� �κ��� ������̵��� ũ��
            RaycastHit[] hits = Physics.SphereCastAll(lastRootPosition, radius,
                displacementSinceLastFrame.normalized, displacementSinceLastFrame.magnitude,
                hittableLayers, QueryTriggerInteraction.Collide);   //��Ʈ�� ���̾� ����Ʈ

            foreach(var hit in hits)        //�ּҰŸ����ϱ�
            {
                if(IsHitValid(hit) && hit.distance < cloestHit.distance)    //��ȿ�� ��Ʈ���� �ϰ� �Ÿ��˻��ϱ�
                {
                    foundHit = true;      //Ʈ��
                    cloestHit = hit;        //���尡��� ��Ʈ ã����
                }
            }
            //hit �� �浹ü�� ã����
            if(foundHit)
            {
                if(cloestHit.distance <= 0f)        // ���̳ʽ��� �Ǹ� ��Ʈ�� ������
                {
                    cloestHit.point = root.position;        //�������� �ʰ� ��Ʈ ����������
                    cloestHit.normal = -transform.forward;  //�븻���⵵ ���̳ʽ���
                }
                OnHit(cloestHit.point, cloestHit.normal, cloestHit.collider);        //��Ʈ ���� 
            }

            lastRootPosition = root.position;           
        }

        //hit�� ��ȿ���� ����
        bool IsHitValid(RaycastHit hit)
        {
            //IgnoreHitDectection ������Ʈ�� ���� �ݶ��̴� ����
            if (hit.collider.GetComponent<IgnoreHitDectection>())   //���̾ƴϸ�
            {
                return false;   //��ȿ���� ����
            }
            //��Ʈ�� �ݶ��̴��� ���� ����Ʈ�� ���� IgnoreHitDectection�� ���Ե� �ݶ��̴� ����
            if (ignoredColliders != null && ignoredColliders.Contains(hit.collider)) 
            {
                return false;
            }

            //Trigger collider üũ�Ǿ������� ���� ���������� �ֵ� ����
            if (hit.collider.isTrigger && hit.collider.GetComponent<Damageable>() == null) 
            {
                return false;
            }
            return true;
        }

        //��Ʈ ����, ������, Vfx, Sfx
        void OnHit(Vector3 point, Vector3 normal, Collider collider)
        {
            //Vfx
            //�浹ü �ٷ����� offset����ؼ� �ణ������ ������ġ�� �븻��������
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
                //�浹��ġ�� ���ӿ�����Ʈ�� �����ϰ� AudioSource ������Ʈ�� �߰��ؼ� ������ Ŭ���� �÷���
                AudioUtilty.CreateSfx(impactSfxClip, point, 1f, 3f);
                Debug.Log("Play impactSfxClip");
            }


            //�߻�ü ų
            Destroy(this.gameObject);
        }
    }
}


