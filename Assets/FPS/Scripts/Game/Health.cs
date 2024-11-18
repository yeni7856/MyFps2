using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Game
{
    /// <summary>
    /// ü���� �����ϴ� Ŭ����
    /// </summary>
    public class Health : MonoBehaviour
    {
        #region Variables
        [SerializeField] private float maxHealth = 100f;    //�ִ� Hp
        public float CurrentHealth { get; private set; }    //���� Hp
        private bool isDeath = false;                               //���� üũ

        public UnityAction<float, GameObject> OnDamaged;
        public UnityAction OnDie;
        public UnityAction<float> OnHeal;

        //ü�� ���� ���
        [SerializeField] private float criticalHealRatio = 0.3f;

        public bool Invincible { get; private set; }    //����
        #endregion

        //�� �������� ������ �ִ��� üũ //Ǯ
        public bool CanPickUp() => CurrentHealth < maxHealth;

        //UI Hp ������ ��
        public float GetRatio() => CurrentHealth / maxHealth;

        //Hp���� üũ 
        public bool IsCritical() => GetRatio() <= criticalHealRatio;

        void Start()
        {
            //�ʱ�ȭ
            CurrentHealth = maxHealth;
            Invincible = false;
        }
        //damageSource�������� �ִ� ��ü 
        public void TakeDamage(float damage, GameObject damageSource)
        {
            //����üũ
            if (Invincible)
                return;
            float beforeHp = CurrentHealth; //������ �Ա����� hp
            CurrentHealth -= damage;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);
            //Debug.Log($"CurrentHealth : {CurrentHealth}");

            //real Damage ���ϱ�
            float realDamage = beforeHp - CurrentHealth;
            if (realDamage > 0)
            {
                //������ ���� ? ���̾ƴϸ� �ڿ��� ȣ��
                OnDamaged?.Invoke(realDamage, damageSource);
            }
            //����ó��
            HandleDeath();
        }

        void HandleDeath()
        {
            if (isDeath)
                return;
            if (CurrentHealth <= 0)
            {
                isDeath = true;

                //���� ����
                OnDie?.Invoke();
            }
        }

        //��
        public void Heal(float heal)
        {
            float beforeHp = CurrentHealth; //������ �Ա����� hp
            CurrentHealth += heal;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);

            //real Heal ���ϱ�
            float realHeal = CurrentHealth - beforeHp;
            if (realHeal > 0)
            {
                //�� ���� ? ���̾ƴϸ� �ڿ��� ȣ��
                OnHeal?.Invoke(realHeal);
            }
        }
    }
}