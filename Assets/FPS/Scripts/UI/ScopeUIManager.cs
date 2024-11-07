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
            //참조
            weaponsManager = GameObject.FindObjectOfType<PlayerWeaponsManager>();

            //이벤트 함수 등록
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
