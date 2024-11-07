using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    /// 발사체 표준형
    /// </summary>
    public  class ProjectileStandard : ProjectileBase
    {
        #region Variables
        [SerializeField] private float speed = 300f;
        [SerializeField] private float gravity = 0f;
        [SerializeField] private float damage = 15f;
        [SerializeField] private float lifeTime = 2f;
        #endregion
    }
}


