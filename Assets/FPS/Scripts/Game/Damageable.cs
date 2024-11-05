using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Game
{
    /// <summary>
    /// �浹ü(hit box)�� �����Ǿ� �������� �����ϴ� Ŭ����
    /// </summary>
    public class Damageable : MonoBehaviour
    {
        #region Variables
        private Health health;
        //������ ���
        [SerializeField] private float damageMultiplier = 1f;
        //�ڽ��� ���� ������ ��� 
        [SerializeField] private float sensiblilityToSeltDamage = 0.5f;
        #endregion
        private void Awake()
        {
            health = GetComponent<Health>();
            if(health == null )
            {
                health = GetComponentInParent<Health>();
            }
        }
        public void InflictDamage(float damage, bool isExplosionDamage, GameObject damageSource)
        {
            if(health == null)  //�ｺ�������� ������x
                return;
            //totalDamage�� ���� ��������
            var totalDamage = damage;
            
            //���� ������ üũ - ���� �������϶��� DamageMultiplier�� ������� �ʴ´�
            if(isExplosionDamage == false)
            {
                totalDamage *= damageMultiplier;
            }

            //�ڽ��� ���� �������� 
            if(health.gameObject == damageSource)
            {
                totalDamage *= sensiblilityToSeltDamage;
            }

            //������ ������
            health.TakeDamage(totalDamage,damageSource);

        }
    }
}