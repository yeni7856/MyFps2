using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Gameplay;
using UnityEngine;

namespace Unity.Fps.UI
{
    public class ScopeUIManager : MonoBehaviour
    {
        #region
        public GameObject scopeUI;
        private PlayerWeaponsManager weaponsManager;
        #endregion

        void Start()
        {
            //����
            weaponsManager = GameObject.FindObjectOfType<PlayerWeaponsManager>();

            //�̺�Ʈ �Լ� ���
            weaponsManager.OnScopedWeapon += OnScope;
            weaponsManager.OffScopedWeapon += OffScope;
        }
        
        public void OnScope()
        {
            scopeUI.SetActive(true);
        }
        public void OffScope()
        {
            scopeUI.SetActive(false);
        }
    }
}
