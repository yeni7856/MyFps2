using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// ���� ���� �ȿ� �ִ� �ݶ��̴� ������Ʈ ������ �ֱ�
    /// </summary>
    public class DamageArea : MonoBehaviour
    {
        #region Variables
        [SerializeField] private float areaOFEffectDistance = 10f;       //�Ÿ� 10�ȿ��ִ� ������Ʈ����
        [SerializeField] private AnimationCurve damageRatioOverDistance;
        #endregion
        
        public void InflictDamageArea(float damage, Vector3 ceneter, LayerMask layers, QueryTriggerInteraction interaction, GameObject owner)
        {
            Dictionary<Health, Damageable> uniqueDamagedHealth = new Dictionary<Health, Damageable>();
            //�߽����κ��� 10��ŭ �������ִ� �浹ü
            Collider[] affectedColliders = Physics.OverlapSphere(ceneter, areaOFEffectDistance, layers, interaction);
            foreach (Collider collider in affectedColliders)
            {
                Damageable damageable = collider.GetComponent<Damageable>();
                if (damageable)
                {
                    Health health = damageable.GetComponentInParent<Health>();  //�θ��ʿ��ִ� �۽� ������
                   //����� ��Ǿ�� ��ϵǾ��ִ��� ������ //�ϳ��� ������Ʈ�� �ϳ��� �������� 
                   if(health != null && uniqueDamagedHealth.ContainsKey(health) ==false)
                    {
                        uniqueDamagedHealth.Add(health, damageable);
                    }
                }
            }

            //������ �ֱ� ��Ǿ���� �ϳ��ϳ��� ����
            foreach(var uniqueDamageable in uniqueDamagedHealth.Values)
            {
                //uniqueDamageable.transform.position���� ���� 
                float distance = Vector3.Distance(uniqueDamageable.transform.position, ceneter);
                float curveDamage = damage * damageRatioOverDistance.Evaluate(distance);
                Debug.Log($"curveDamage{curveDamage}");
                uniqueDamageable.InflictDamage(damage, true, owner);
            }
        }
    }
}