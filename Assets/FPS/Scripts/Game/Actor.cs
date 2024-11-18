using UnityEngine;
namespace Unity.FPS.Game
{
    /// <summary>
    /// ���ӿ� �����ϴ� Actor�� �����ϴ� Ŭ����
    /// </summary>
    public class Actor : MonoBehaviour
    {
        #region Variables
        //�Ҽ� - �Ʊ�, ���� ����
        public int affiliation;
        //������
        public Transform aimPoint;
        private ActorManager actorManager;
        #endregion
        private void Start()
        {
            //Actor ����Ʈ�� �߰�(���)
            actorManager = GameObject.FindObjectOfType<ActorManager>();
            //Actor ����Ʈ�� ���ԵǾ� �ִ��� üũ
            if (actorManager.Actors.Contains(this) == false)
            {
                actorManager.Actors.Add(this);
            }
        }
        private void OnDestroy()
        {
            //Actor ����Ʈ���� ����
            if (actorManager)
            {
                actorManager.Actors.Remove(this);
            }
        }
    }
}