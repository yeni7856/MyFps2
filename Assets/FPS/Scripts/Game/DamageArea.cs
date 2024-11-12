using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// 일정 범위 안에 있는 콜라이더 오브젝트 데미지 주기
    /// </summary>
    public class DamageArea : MonoBehaviour
    {
        #region Variables
        [SerializeField] private float areaOFEffectDistance = 10f;       //거리 10안에있는 오브젝트한테
        [SerializeField] private AnimationCurve damageRatioOverDistance;
        #endregion
        
        public void InflictDamageArea(float damage, Vector3 ceneter, LayerMask layers, QueryTriggerInteraction interaction, GameObject owner)
        {
            Dictionary<Health, Damageable> uniqueDamagedHealth = new Dictionary<Health, Damageable>();
            //중심으로부터 10만큼 떨어져있는 충돌체
            Collider[] affectedColliders = Physics.OverlapSphere(ceneter, areaOFEffectDistance, layers, interaction);
            foreach (Collider collider in affectedColliders)
            {
                Damageable damageable = collider.GetComponent<Damageable>();
                if (damageable)
                {
                    Health health = damageable.GetComponentInParent<Health>();  //부모쪽에있는 핼스 가져옴
                   //등록한 디션어리에 등록되어있는지 없는지 //하나의 오브젝트에 하나의 데미지만 
                   if(health != null && uniqueDamagedHealth.ContainsKey(health) ==false)
                    {
                        uniqueDamagedHealth.Add(health, damageable);
                    }
                }
            }

            //데미지 주기 디션어리에서 하나하나씩 꺼냄
            foreach(var uniqueDamageable in uniqueDamagedHealth.Values)
            {
                //uniqueDamageable.transform.position에서 센터 
                float distance = Vector3.Distance(uniqueDamageable.transform.position, ceneter);
                float curveDamage = damage * damageRatioOverDistance.Evaluate(distance);
                Debug.Log($"curveDamage{curveDamage}");
                uniqueDamageable.InflictDamage(damage, true, owner);
            }
        }
    }
}