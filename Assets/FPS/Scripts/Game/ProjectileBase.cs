using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Game
{
    /// <summary>
    /// �߻�ü�� �⺻�� �Ǵ� �θ� Ŭ����
    /// </summary>
    public abstract class ProjectileBase : MonoBehaviour 
    {
        #region Variables
        public GameObject Owner {  get; private set; }  //���� �߻� �ߴ��� �߻��� ��ü
        public Vector3 InitialPosition { get; private set; }    //�ʱ� �����ǰ�
        public Vector3 InitialDirection { get; private set; }   //�ʱ� ���Ⱚ
        public Vector3 InheritedMuzzleVelocity { get; private set; }      //���� 
        public float InitialCharge { get; private set; }    //�ʱ� ������

        public UnityAction OnShoot;                         //�߻�� ��ϵ� �Լ� ȣ��
        #endregion
        public void Shoot(WeaponController controller)  //����⿡�� �߻����� 
        {
            //�ʱⰪ ���� �ʱ�ȭ
            Owner = controller.Owner;       //������ ������
            InitialPosition = this.transform.position;      //�߻�ü ������
            InitialDirection = this.transform.forward;          //������ �չ���
            InheritedMuzzleVelocity = controller.MuzzleWorldVelocity;   //�Ѽ� �ӵ�
            InitialCharge = controller.CurrentCharge;                       //������
                
            OnShoot?.Invoke();
        }
    }
}
