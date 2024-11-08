using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// TimeSelfDestruct ������ ���� ������Ʈ�� ���� �� ������ �ð��� ų
    /// </summary>
    public class TimeSelfDestruct : MonoBehaviour
    {
        #region Variables
        public float lifeTime = 1f;    //ų�ð�
        private float spawnTime;            //�����ɶ��� �ð�
        #endregion

        private void Awake()
        {
            //�����ð� ����
            spawnTime = Time.time;
        }
        public void Update()
        {
            if ((spawnTime + lifeTime) <= Time.time)
            {
                Destroy(this.gameObject);
            }
        }
    }
}
