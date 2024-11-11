using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    /// ������ �߻�ü�� �߻��Ҷ� �������� ���� �߻�ü�� ���ӿ�����Ʈ ũ�����
    /// </summary>
    public class ChargedProjectileEffectHandler : MonoBehaviour
    {
        #region Variables
        private ProjectileBase projectileBase;

        public GameObject charageObject;
        public MinMaxVector3 scale;
        #endregion

        private void OnEnable()
        {
            projectileBase = GetComponent<ProjectileBase>();
            projectileBase.OnShoot += OnShoot;
        }
        void OnShoot()
        {
            charageObject.transform.localScale = scale.GetValueFromRatio(projectileBase.InitialCharge);
        }
    }
}