using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Unity.FPS.Game
{
    /// <summary>
    /// ����� �÷��� ���� ��� ����
    /// </summary>
    public class AudioUtilty : MonoBehaviour    //����ƽ���� ��밡���ϰ� �����ٰ�
    {
        //������ ��ġ�� ���� ������Ʈ �����ϰ� AudioSource ������Ʈ�� �߰��ؼ� ������ Ŭ���� �÷���
        //Ŭ�� ���� �÷��̰� ������ �ڵ����� ų - TimeSelfDestruct ������Ʈ �̿�
      
        public static void CreateSfx(AudioClip clip, Vector3 position, float spatialBlend, float rolloffDistanceMin = 1f)
        {
            GameObject impactSfxInstance = new GameObject(); //�������Ʈ �����
            impactSfxInstance.transform.position = position;

            //Audio Clip Play
            AudioSource source = impactSfxInstance.AddComponent<AudioSource>();  //������Ʈ�� ������ҽ� ����� ��ü��������
            source.clip = clip;                                        //�ҽ�Ŭ��
            source.spatialBlend = spatialBlend;             //�־����� �ȵ鸲 ���������鸲
            source.minDistance = rolloffDistanceMin;   //�ּ� ��
            source.Play();                                              //�ҽ� �÷��� 

            //������Ʈ ų
            TimeSelfDestruct timeSelfDestruct = impactSfxInstance.AddComponent<TimeSelfDestruct>();
            timeSelfDestruct.lifeTime = clip.length;        //����� �÷��� ���� �÷��� Ÿ�� ������ ������Ʈ ų
            

        }
    }
}
