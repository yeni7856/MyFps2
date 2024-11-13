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
        public Image healthBar;                     //�۽���

        public Transform healthBarPivot;       //�۽��� �Ǻ�

        [SerializeField] private bool hideFullHealthBar = true;  //hp�� Ǯ�̸� healthbar�� �����
        #endregion

        private void Update()
        {
            healthBar.fillAmount = health.GetRatio();

            //UI�� �÷��̾ �ٶ󺸵��� �Ѵ�.
            healthBarPivot.LookAt(Camera.main.transform.position);

            //Hp�� Ǯ�̸� healthBar�� ����
            if (hideFullHealthBar)
            {
                healthBarPivot.gameObject.SetActive(healthBar.fillAmount != 1f);
            }

        }
    }
}
