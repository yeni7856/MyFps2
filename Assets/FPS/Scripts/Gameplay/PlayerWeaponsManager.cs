using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;
using UnityEditor.Search;
using System;
using UnityEngine.Events;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    /// ���� ��ü ����
    /// </summary>
    public enum WeaponSwithState
    {
        Up,
        Down,
        PutDownPrvious, //�������� �����ܰ�
        PutUpNew,
    }

    /// <summary>
    /// �÷��̾ ���� ������� �����ϴ� Ŭ���� 
    /// </summary>
    public class PlayerWeaponsManager : MonoBehaviour
    {
        #region Variables
        //���� ���� - ������ �����Ҷ� ó�� �������� ���޵Ǵ� ���� ����Ʈ(�κ��丮)
        public List<WeaponController> startingWeapons = new List<WeaponController>();

        //���⸦ �����ϴ� ������Ʈ
        public Transform weaponParentSocket;

        //�÷��̾ �����߿� ��� �ٴϴ� ���� ����Ʈ
        private WeaponController[] weaponSlots = new WeaponController[9];

        //���� ����Ʈ(����)�� Ȱ��ȭ�� ���⸦ �����ϴ� �ε��� ������Ƽ��
        public int ActiveWeaponIndex { get; private set; }

        //���� ��ü
        public UnityAction<WeaponController> OnSwitchToWeapon;  //���� ��ü�� ��ϵ��Լ� ȣ��

        private WeaponSwithState weaponSwithState;      //���� ��ü�� ����

        //�÷��̾� ��ǲ 
        private PlayerInputHandler playerInputHandler;

        //���� ��ü�� ���Ǵ� ������ġ 
        private Vector3 weaponMainlocalPosition;

        public Transform defaultWeaponPostion;
        public Transform downWeaponPostion;

        private int weaponSwitchNewIndex;                   //���� �ٲ�� ���� �ε��� 
        private float weaponSwitchTimeStarted = 0;      //�ð�
        [SerializeField] private float weaponSwitchDelay = 1f;
        #endregion

        private void Start()
        {
            //���� 
            playerInputHandler = GetComponent<PlayerInputHandler>();
            //�ʱ�ȭ
            ActiveWeaponIndex = -1;
            weaponSwithState = WeaponSwithState.Down;

            OnSwitchToWeapon += OnWeaponSwitched;

            //���� ���� ���� ����
            foreach (var weapon in startingWeapons) //�ϳ��� ������
            {
                AddWeapon(weapon);
            }
            //ù��° ���� Ȱ��ȭ 
            SwitchWeapon(true);
        }
        private void Update()
        {
            //���ⳡ������ �ٲٱ� 
            if(weaponSwithState == WeaponSwithState.Up || weaponSwithState == WeaponSwithState.Down)
            {
                int switchWeaponInput = playerInputHandler.GetSwitchWeaponInput();
                if (switchWeaponInput != 0)
                {
                    {
                        bool switchUp = switchWeaponInput > 0;
                        SwitchWeapon(switchUp);
                    }
                }
            }
        }
        private void LateUpdate()
        {
            UpdateWeaponSwitching();
            //���� ���� ��ġ
           weaponParentSocket.localPosition = weaponMainlocalPosition;  

        }
        //���¿� ���� ���� ����
        void UpdateWeaponSwitching()
        {
            //Lerp ����
            float switchingTimeFactor = 0f;
            if(weaponSwitchDelay == 0f)
            {
                switchingTimeFactor = 1f;
            }
            else
            {
                switchingTimeFactor = Mathf.Clamp01((Time.time - weaponSwitchTimeStarted) / weaponSwitchDelay);
            }

            //���� �ð� ���� ���� ���� �ٲٱ� 
            //��Ƽ�갡 ������� ����������� �����´����� ���ο�� �ٲ���� �ö�
            if(switchingTimeFactor >= 1f)   //�����̰� ������ 1�ʰ������� 
            {
                if(weaponSwithState == WeaponSwithState.PutDownPrvious)
                {
                    //���� ���� false, ���ο�� true
                    WeaponController oldWeapon = GetActiveWeapon();
                    if(oldWeapon != null)
                    {
                        oldWeapon.ShowWeapon(false);
                    }
                    ActiveWeaponIndex = weaponSwitchNewIndex; //���ⳡ���� ���ο� �ε��� ��
                    WeaponController newWapon = GetActiveWeapon();
                    OnSwitchToWeapon?.Invoke(newWapon);     //����ٲ� 

                    switchingTimeFactor = 0f;
                    if(newWapon != null)
                    {
                        weaponSwitchTimeStarted = Time.time;    // �ٽ� ����
                        weaponSwithState = WeaponSwithState.PutUpNew; //�ٽ� ������ 
                    }
                    else
                    {
                        weaponSwithState = WeaponSwithState.Down;       //��Ƽ���Ѱ� ������ �ٿ�
                    }
                }
                else if (weaponSwithState == WeaponSwithState.PutUpNew) //�����ϸ� Up���� 
                {
                    weaponSwithState = WeaponSwithState.Up;
                }
            }


            //�����ð����� ���� ��ġ �̵� 
            if (weaponSwithState == WeaponSwithState.PutDownPrvious)
            {
                weaponMainlocalPosition = Vector3.Lerp(defaultWeaponPostion.localPosition, downWeaponPostion.localPosition, switchingTimeFactor);
            }
            else if (weaponSwithState == WeaponSwithState.PutUpNew)
            {
                weaponMainlocalPosition = Vector3.Lerp(downWeaponPostion.localPosition, defaultWeaponPostion.localPosition, switchingTimeFactor);
            }
        }

        //���������� ������ ������Ʈ�ѷ� ������Ʈ �߰�
        public bool AddWeapon(WeaponController weaponPrefab)
        {
            //�߰��ϴ� ���� ���� ���� üũ - �ߺ��˻�
            if(HasWeapon(weaponPrefab) != null)
            {
                Debug.Log("���� ����");
                return false;
            }
            //�ϳ��� �޾ƿ� //������Ʈ�η��� ��ȯ�� 
            for(int i=0; i<weaponSlots.Length; i++)
            {
                if(weaponSlots[i]== null)
                {
                    //���Կ� �������
                    WeaponController weaponInstance = Instantiate(weaponPrefab, weaponParentSocket);
                    weaponInstance.transform.localPosition = Vector3.zero;  //�θ���ġ�� ������ ��ġ�� ������
                    weaponInstance.transform.localRotation = Quaternion.identity;   

                    weaponInstance.Owner = this.gameObject; //�̿�����Ʈ�� ����� ������ ����
                    weaponInstance.SourcePrefab = weaponPrefab.gameObject; //���� ������ ���� �����տ�����Ʈ
                    weaponInstance.ShowWeapon(false);

                    weaponSlots[i] = weaponInstance;

                    return true;
                }
            }
            Debug.Log("���� �޽� Ǯ");
            return false;
        }
        //�Ű������� ����
        private WeaponController HasWeapon(WeaponController weaponPrefab)
        {
            for(int i=0; i<weaponSlots.Length; i++)
            {
                //���� �ҽ� �������� �Ű������� ���� �������̸� ��ȯ 
                if (weaponSlots[i] != null && weaponSlots[i].SourcePrefab == weaponPrefab )
                {
                    return weaponSlots[i];
                }
            }
            //�����Ѱ� ������ �� 
            return null;
        }

        //���� Ȱ��ȭ�� ����
        public WeaponController GetActiveWeapon()
        {
            return GetWeaponAtSlotIndex(ActiveWeaponIndex); //�� �ε����� �ش�Ǵ� �� ��Ƽ�� ����
        }


        //������ ���Կ� ���Ⱑ �ִ��� ���� 
        public WeaponController GetWeaponAtSlotIndex(int index)
        {
            //�������� ū���� ������ �ε��� ���
            if(index >= 0 && index < weaponSlots.Length)
            {
                return weaponSlots[index];
            }
            return null;
        }

        //0������ 9������  0,1,2
        //���� �ٲٱ�, ���� ��� �ִ� ���� false, ���ο� ���� true
        public void SwitchWeapon(bool ascendingOrder)
        {
            //���ο� ��Ƽ���� ���� �ε��� 
            int newWeaponIndex = -1;

            //����� ���Ա��� �Ÿ�
            int closestSlotDistance = weaponSlots.Length;

            //���� ����� ���� ã��
            for(int i=0;i<weaponSlots.Length;i++)
            {
                //���� Ȱ��ȭ�� Index �� i�� Ʋ������  //���԰� �Ÿ� �ּ� �Ÿ� 
                if (i != ActiveWeaponIndex && GetWeaponAtSlotIndex(i) != null)  
                {
                    int distanceToActiveIndex = GetDistanceBetweenWeaponSlot(ActiveWeaponIndex, i, ascendingOrder);
                    if(distanceToActiveIndex < closestSlotDistance)
                    {
                        closestSlotDistance = distanceToActiveIndex;    //Ŭ���� ���Ͻ��� �ּ� ���̵�
                        newWeaponIndex = i; //���ο� �Ÿ��� �� 
                    }
                }
            }
            //���� ��Ƽ���� ���� �ε����� ���� ��ü (���ϰ���� �ε�����)
            SwitchToWeaponIndex(newWeaponIndex);
        }

        private void SwitchToWeaponIndex(int newWeaponIndex)
        {
            //newWeaponIndex�� üũ 
            if (newWeaponIndex >= 0 && newWeaponIndex != ActiveWeaponIndex)
            {
                weaponSwitchNewIndex = newWeaponIndex;
                weaponSwitchTimeStarted = Time.time;  

                //���� ��Ƽ���� ���Ⱑ �ִ���?
                if(GetActiveWeapon() == null)
                {
                    //�Ʒ� ��ġ�� �����ǿ� 
                    weaponMainlocalPosition = downWeaponPostion.position;
                    weaponSwithState = WeaponSwithState.PutUpNew; //������ �ö󰡴�
                    ActiveWeaponIndex = newWeaponIndex; //�ٸ��ɷ� �ٲ�

                    //�����ͼ� �����ֱ� 
                    WeaponController weaponController = GetWeaponAtSlotIndex(newWeaponIndex);
                    OnSwitchToWeapon?.Invoke(weaponController);
                }
                else
                {
                    //�����ö󰣰� 
                    weaponSwithState = WeaponSwithState.PutDownPrvious;
                }

                /*if(ActiveWeaponIndex >= 0) //���̳ʽ��� �Ǹ� �ȵ� 
                {
                    WeaponController nowWeapon = GetWeaponAtSlotIndex(ActiveWeaponIndex);
                    nowWeapon.ShowWeapon(false);
                }
                WeaponController newWeapon = GetWeaponAtSlotIndex(newWeaponIndex);
                newWeapon.ShowWeapon(true);
                ActiveWeaponIndex = newWeaponIndex;*/
            }
        }

        //���԰� �Ÿ�,
        private int GetDistanceBetweenWeaponSlot(int fromSlotIndex, int toSlotIndex, bool ascendingOrder)
        {
            int distanceBetweenSlots = 0;
            if (ascendingOrder)
            {
                distanceBetweenSlots = toSlotIndex - fromSlotIndex;
            }
            else
            {
                distanceBetweenSlots = fromSlotIndex - toSlotIndex;
            }
            if(distanceBetweenSlots < 0)
            {
                distanceBetweenSlots += weaponSlots.Length; //�Ÿ� ���̳ʽ� ���� 
            }
            return distanceBetweenSlots;
        }
    void OnWeaponSwitched(WeaponController newWeapon)
    {
        if(newWeapon != null)
        {
                newWeapon.ShowWeapon(true);
        }
    }
    }
}
