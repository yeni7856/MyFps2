using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class WorldSpaceHealthBar : MonoBehaviour
    {
        #region Variables
        public Health health;                          //current health
        public Image healthBar;                     //핼스바

        public Transform healthBarPivot;       //핼스바 피봇

        [SerializeField] private bool hideFullHealthBar = true;  //hp가 풀이면 healthbar를 숨긴다
        #endregion

        private void Update()
        {
            healthBar.fillAmount = health.GetRatio();

            //UI가 플레이어를 바라보도록 한다.
            healthBarPivot.LookAt(Camera.main.transform.position);

            //Hp가 풀이면 healthBar를 숨김
            if (hideFullHealthBar)
            {
                healthBarPivot.gameObject.SetActive(healthBar.fillAmount != 1f);
            }

        }
    }
}
