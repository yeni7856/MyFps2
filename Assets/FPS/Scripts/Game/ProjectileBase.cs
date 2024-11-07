using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Game
{
    /// <summary>
    /// 발사체의 기본이 되는 부모 클래스
    /// </summary>
    public abstract class ProjectileBase : MonoBehaviour 
    {
        #region Variables
        public GameObject Owner {  get; private set; }  //누가 발사 했는지 발사한 주체
        public Vector3 InitialPosition { get; private set; }    //초기 포지션값
        public Vector3 InitialDirection { get; private set; }   //초기 방향값
        public Vector3 InheritedMuzzleVelocity { get; private set; }      //머즐 
        public float InitialCharge { get; private set; }    //초기 충전값

        public UnityAction OnShoot;                         //발사시 등록된 함수 호출
        #endregion
        public void Shoot(WeaponController controller)  //어떤무기에서 발사됬는지 
        {
            //초기값 설정 초기화
            Owner = controller.Owner;       //주인이 누군지
            InitialPosition = this.transform.position;      //발살체 포지션
            InitialDirection = this.transform.forward;          //포지션 앞방향
            InheritedMuzzleVelocity = controller.MuzzleWorldVelocity;   //총수 속도
            InitialCharge = controller.CurrentCharge;                       //충전량
                
            OnShoot?.Invoke();
        }
    }
}
