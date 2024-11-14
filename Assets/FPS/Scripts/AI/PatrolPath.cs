using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.AI
{
    /// <summary>
    /// ��Ʈ�� WayPoint���� �����ϴ� Ŭ����
    /// </summary>
    public class PatrolPath : MonoBehaviour
    {
        #region Variables
        public List<Transform> wayPoints = new List<Transform>();

        //this Path�� ��Ʈ���ϴ� enemy��
        public List<EnemyController> enemiesToAssign = new List<EnemyController>();
        #endregion

        private void Start()
        {
            //��ϵ� enemy���� ��Ʈ�� �н�(this) ����
            foreach(var enemy in enemiesToAssign)
            {
                enemy.PatrolPath = this;
            }
        }

        //Ư�� ��ġ�� ���� ������ WayPoint���� �Ÿ� ���ϱ�
        public float GetDistanceToWayPoint(Vector3 origin, int wayPointIndex)
        {
            if(wayPointIndex < 0 || wayPointIndex >= wayPoints.Count
                || wayPoints[wayPointIndex] == null)
            {
                return -1f;
            }

            return (wayPoints[wayPointIndex].position - origin).magnitude;
        }

        //Index�� ������ WayPoint�� ��ġ ��ȯ
        public Vector3 GetPositionWayPoint(int wayPointIndex)
        {
            if(wayPointIndex < 0 || wayPointIndex >= wayPoints.Count
                || wayPoints[wayPointIndex] == null)
            {
                return Vector3.zero;
            }
            return wayPoints[wayPointIndex].position;
        }

        //������ Path �׸���
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            for(int i = 0; i < wayPoints.Count; i++)
            {
                int nextIndext = i + 1;
                if(nextIndext >= wayPoints.Count)
                {
                    nextIndext -= wayPoints.Count;
                }
                Gizmos.DrawLine(wayPoints[i].position, wayPoints[nextIndext].position);
                Gizmos.DrawSphere(wayPoints[i].position, 0.1f);
            }
        }
    }
}
