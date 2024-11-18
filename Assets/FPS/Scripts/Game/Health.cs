using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Game
{
    /// <summary>
    /// 체력을 관리하는 클래스
    /// </summary>
    public class Health : MonoBehaviour
    {
        #region Variables
        [SerializeField] private float maxHealth = 100f;    //최대 Hp
        public float CurrentHealth { get; private set; }    //현재 Hp
        private bool isDeath = false;                               //죽음 체크

        public UnityAction<float, GameObject> OnDamaged;
        public UnityAction OnDie;
        public UnityAction<float> OnHeal;

        //체력 위험 경계
        [SerializeField] private float criticalHealRatio = 0.3f;

        public bool Invincible { get; private set; }    //무적
        #endregion

        //힐 아이템을 먹을수 있는지 체크 //풀
        public bool CanPickUp() => CurrentHealth < maxHealth;

        //UI Hp 게이지 값
        public float GetRatio() => CurrentHealth / maxHealth;

        //Hp위험 체크 
        public bool IsCritical() => GetRatio() <= criticalHealRatio;

        void Start()
        {
            //초기화
            CurrentHealth = maxHealth;
            Invincible = false;
        }
        //damageSource데미지를 주는 주체 
        public void TakeDamage(float damage, GameObject damageSource)
        {
            //무적체크
            if (Invincible)
                return;
            float beforeHp = CurrentHealth; //데미지 입기전에 hp
            CurrentHealth -= damage;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);
            //Debug.Log($"CurrentHealth : {CurrentHealth}");

            //real Damage 구하기
            float realDamage = beforeHp - CurrentHealth;
            if (realDamage > 0)
            {
                //데미지 구현 ? 널이아니면 뒤에꺼 호출
                OnDamaged?.Invoke(realDamage, damageSource);
            }
            //죽음처리
            HandleDeath();
        }

        void HandleDeath()
        {
            if (isDeath)
                return;
            if (CurrentHealth <= 0)
            {
                isDeath = true;

                //죽음 구현
                OnDie?.Invoke();
            }
        }

        //힐
        public void Heal(float heal)
        {
            float beforeHp = CurrentHealth; //데미지 입기전에 hp
            CurrentHealth += heal;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);

            //real Heal 구하기
            float realHeal = CurrentHealth - beforeHp;
            if (realHeal > 0)
            {
                //힐 구현 ? 널이아니면 뒤에꺼 호출
                OnHeal?.Invoke(realHeal);
            }
        }
    }
}