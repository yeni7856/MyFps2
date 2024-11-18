using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Unity.FPS.Game
{
    /// <summary>
    /// ���ӿ� �����ϴ� Actor���� �����ϴ� Ŭ����
    /// </summary>
    public class ActorManager : MonoBehaviour
    {
        #region Variables
        public List<Actor> Actors { get; private set; }
        public GameObject Player { get; private set; }
        #endregion
        //�÷��̾� ����
        public void SetPlayer(GameObject player) => Player = player;
        private void Awake()
        {
            Actors = new List<Actor>();
        }
    }
}