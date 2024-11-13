using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MySample
{
    public class MaterialTest : MonoBehaviour
    {
        #region Variables
        private Renderer m_renderer;

        private MaterialPropertyBlock m_blocks;   
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            m_renderer = GetComponent<Renderer>();

            //메터리얼 컬러 바꾸기 //같은 메터리얼 한번에 그리니까 (배칭) 이렇게 바꾸면 최적화x t사본이 여러개생겨서 배칭이 달라짐
            //m_renderer.material.SetColor("_BaseColor", Color.red);                //스크립트 없으면 안바뀜
                                                                                  //m_renderer.sharedMaterial.SetColor("_BaseColor", Color.red);        //동시에 바뀜 
            //객체 생성 new로 
            m_blocks = new MaterialPropertyBlock();
            m_blocks.SetColor("_BaseColor", Color.red);     //먼저 넣기 
            m_renderer.SetPropertyBlock(m_blocks);          //한번에 적용 
        }
    }
}

