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

            //���͸��� �÷� �ٲٱ� //���� ���͸��� �ѹ��� �׸��ϱ� (��Ī) �̷��� �ٲٸ� ����ȭx t�纻�� ���������ܼ� ��Ī�� �޶���
            //m_renderer.material.SetColor("_BaseColor", Color.red);                //��ũ��Ʈ ������ �ȹٲ�
                                                                                  //m_renderer.sharedMaterial.SetColor("_BaseColor", Color.red);        //���ÿ� �ٲ� 
            //��ü ���� new�� 
            m_blocks = new MaterialPropertyBlock();
            m_blocks.SetColor("_BaseColor", Color.red);     //���� �ֱ� 
            m_renderer.SetPropertyBlock(m_blocks);          //�ѹ��� ���� 
        }
    }
}

