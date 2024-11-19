using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    /// <summary>
    /// 데미지, 힐 플레시 효과
    /// </summary>
    public class PlayerHealthBar : MonoBehaviour
    {
        #region Variables
        private Health playerHealth;
        public Image healthFillImage;
        #endregion

        private void Start()
        {
            //참조
            PlayerCharacterController playerCharacterController = 
                GameObject.FindObjectOfType<PlayerCharacterController>();

            playerHealth = playerCharacterController.GetComponent<Health>();
        }

        private void Update()
        {
            healthFillImage.fillAmount = playerHealth.GetRatio();
        }
    }
}
