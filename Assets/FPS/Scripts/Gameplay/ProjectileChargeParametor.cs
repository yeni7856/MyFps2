using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    /// ������ �߻�ü�� �߻��Ҷ� �������� ���� �߻�ü�� �Ӽ����� ���� 
    /// </summary>
    public class ProjectileChargeParametor : MonoBehaviour
    {
        #region Variables
        private ProjectileBase projectileBase;

        public MinMaxFloat Damage;
        public MinMaxFloat Speed;
        public MinMaxFloat GravityDown;
        public MinMaxFloat Raidus;
        #endregion

        private void OnEnable()
        {
            //����
            projectileBase = GetComponent<ProjectileBase>();
            projectileBase.OnShoot += OnShoot;
        }

        //�߻�ü �߻�� projectileBase�� onShoot ��������Ʈ �Լ����� ȣ��
        //�߻��� �Ӽ����� Charge ���� ���� ����
        void OnShoot()
        {
            //�������� ���� �߻�ü �Ӽ����� ���� 
            ProjectileStandard projectileStandard = GetComponent<ProjectileStandard>();
            projectileStandard.damage = Damage.GetValueFromRatio(projectileBase.InitialCharge);
            projectileStandard.speed = Speed.GetValueFromRatio(projectileBase.InitialCharge);
            projectileStandard.gravity = GravityDown.GetValueFromRatio(projectileBase.InitialCharge);
            projectileStandard.radius = Raidus.GetValueFromRatio(projectileBase.InitialCharge);
        }
    }

}

