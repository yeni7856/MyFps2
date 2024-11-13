using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.AI
{
    public class EnemyController : MonoBehaviour
    {
        #region Variables
        private Health health;

        //Death
        public GameObject deathVfxPrefab;
        public Transform deathVfxSpawnPostion;
        #endregion

        private void Start()
        {
            //참조
            health = GetComponent<Health>();
            health.OnDamaged += OnDamaged;
            health.OnDie += OnDie;
        }

        void OnDamaged(float damage, GameObject damageSource)
        {

        }

        void OnDie()
        {
            //폭발효과
            GameObject effectGo = Instantiate(deathVfxPrefab, deathVfxSpawnPostion.position, Quaternion.identity);
            Destroy(effectGo,5f);

            //Enemy 킬
            Destroy(this.gameObject);
        }
    }
}
