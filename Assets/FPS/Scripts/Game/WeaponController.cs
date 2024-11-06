using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// 조준점 크로스헤어를 그리기위한 관리 하는 데이터 
    /// </summary>
    [System.Serializable]
    public struct CrossHairData
    {
        public Sprite CraossHairSprite;
        public float CrossHairSize;
        public Color CraossHairColor;
    }

    /// <summary>
    /// 무기를 관리하는 클래스
    /// </summary>
    public class WeaponController : MonoBehaviour
    {
        #region Variables
        //무기 활성화, 비활성
        public GameObject weaponRoot;
        
        public GameObject Owner { get; set; }               //무기의 주인

        //똑같은 무기를 중첩해서 갖지않도록 체크
        public GameObject SourcePrefab { get; set; }    //무기를 생성한 오리지널 프리팹
        public bool IsWeaponActive {  get; private set; }  //무기 활성화 여부

        private AudioSource shootAudioSource;
        public AudioClip swithcWeaponSfx;

        //CrossHair
        public CrossHairData crosshairDefault;      //기본,평상시 

        public CrossHairData crosshairTartgetInSight;   //적을 포착

        //조준
        public float aimZoomRatio = 1f;                 //조준시 무기 줌인줌아웃 설정값
        public Vector3 animOffset;                      //조준시 무기 위치조정
        #endregion

        private void Awake()
        {
            //참조
            shootAudioSource = this.GetComponent<AudioSource>();
        }

        //무기 활성화, 비활성화 
        public void ShowWeapon(bool show)
        {
            weaponRoot.SetActive(show);

            //this 무기로 변경
            if(show == true && swithcWeaponSfx != null)
            {
                //무기 변경 효과음 플레이
                shootAudioSource.PlayOneShot(swithcWeaponSfx);
            }
            
            IsWeaponActive = show;
        }
    }
}
