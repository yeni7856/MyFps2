using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Unity.FPS.Game
{
    /// <summary>
    /// 오디오 플레이 관련 기능 구현
    /// </summary>
    public class AudioUtilty : MonoBehaviour    //스테틱으로 사용가능하게 가져다가
    {
        //지정된 위치에 게임 오브젝트 생성하고 AudioSource 컴포넌트를 추가해서 지정된 클립을 플레이
        //클립 사운드 플레이가 끝나면 자동으로 킬 - TimeSelfDestruct 컴포넌트 이용
      
        public static void CreateSfx(AudioClip clip, Vector3 position, float spatialBlend, float rolloffDistanceMin = 1f)
        {
            GameObject impactSfxInstance = new GameObject(); //빈오브젝트 만들기
            impactSfxInstance.transform.position = position;

            //Audio Clip Play
            AudioSource source = impactSfxInstance.AddComponent<AudioSource>();  //오브젝트에 오디오소스 만들기 객체가져오기
            source.clip = clip;                                        //소스클립
            source.spatialBlend = spatialBlend;             //멀어지면 안들림 가까워지면들림
            source.minDistance = rolloffDistanceMin;   //최소 값
            source.Play();                                              //소스 플레이 

            //오브젝트 킬
            TimeSelfDestruct timeSelfDestruct = impactSfxInstance.AddComponent<TimeSelfDestruct>();
            timeSelfDestruct.lifeTime = clip.length;        //오디오 플레이 길이 플레이 타임 끝나고 오브젝트 킬
            

        }
    }
}
