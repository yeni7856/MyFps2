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
    /// 무기 교체 상태
    /// </summary>
    public enum WeaponSwithState
    {
        Up,
        Down,
        PutDownPrvious, //내려가기 직전단계
        PutUpNew,
    }

    /// <summary>
    /// 플레이어가 가진 무기들을 관리하는 클래스 
    /// </summary>
    public class PlayerWeaponsManager : MonoBehaviour
    {
        #region Variables
        //무기 지급 - 게임을 시작할때 처음 유저에게 지급되는 무기 리스트(인벤토리)
        public List<WeaponController> startingWeapons = new List<WeaponController>();

        //무기를 장착하는 오브젝트
        public Transform weaponParentSocket;

        //플레이어가 게임중에 들고 다니는 무기 리스트
        private WeaponController[] weaponSlots = new WeaponController[9];

        //무기 리스트(슬롯)중 활성화된 무기를 관리하는 인덱스 프로퍼티로
        public int ActiveWeaponIndex { get; private set; }

        //무기 교체
        public UnityAction<WeaponController> OnSwitchToWeapon;  //무기 교체시 등록된함수 호출

        private WeaponSwithState weaponSwithState;      //무기 교체시 상태

        //플레이어 인풋 
        private PlayerInputHandler playerInputHandler;

        //무기 교체시 계산되는 최종위치 
        private Vector3 weaponMainlocalPosition;

        public Transform defaultWeaponPostion;
        public Transform downWeaponPostion;

        private int weaponSwitchNewIndex;                   //새로 바뀌는 무기 인데스 
        private float weaponSwitchTimeStarted = 0;      //시간
        [SerializeField] private float weaponSwitchDelay = 1f;
        #endregion

        private void Start()
        {
            //참조 
            playerInputHandler = GetComponent<PlayerInputHandler>();
            //초기화
            ActiveWeaponIndex = -1;
            weaponSwithState = WeaponSwithState.Down;

            OnSwitchToWeapon += OnWeaponSwitched;

            //지급 받은 무기 장착
            foreach (var weapon in startingWeapons) //하나씩 꺼내씀
            {
                AddWeapon(weapon);
            }
            //첫번째 무기 활성화 
            SwitchWeapon(true);
        }
        private void Update()
        {
            //연출끝나고나서 바꾸기 
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
            //무기 최종 위치
           weaponParentSocket.localPosition = weaponMainlocalPosition;  

        }
        //상태에 따른 무기 연출
        void UpdateWeaponSwitching()
        {
            //Lerp 변수
            float switchingTimeFactor = 0f;
            if(weaponSwitchDelay == 0f)
            {
                switchingTimeFactor = 1f;
            }
            else
            {
                switchingTimeFactor = Mathf.Clamp01((Time.time - weaponSwitchTimeStarted) / weaponSwitchDelay);
            }

            //지연 시간 이후 무기 상태 바꾸기 
            //액티브가 있을경우 내려가기시작 내려온다음에 새로운게 바뀐다음 올라감
            if(switchingTimeFactor >= 1f)   //딜레이가 지나면 1초가지나면 
            {
                if(weaponSwithState == WeaponSwithState.PutDownPrvious)
                {
                    //현재 무기 false, 새로운무기 true
                    WeaponController oldWeapon = GetActiveWeapon();
                    if(oldWeapon != null)
                    {
                        oldWeapon.ShowWeapon(false);
                    }
                    ActiveWeaponIndex = weaponSwitchNewIndex; //연출끝나고 새로운 인덱스 저
                    WeaponController newWapon = GetActiveWeapon();
                    OnSwitchToWeapon?.Invoke(newWapon);     //무기바꿈 

                    switchingTimeFactor = 0f;
                    if(newWapon != null)
                    {
                        weaponSwitchTimeStarted = Time.time;    // 다시 연출
                        weaponSwithState = WeaponSwithState.PutUpNew; //다시 딜레이 
                    }
                    else
                    {
                        weaponSwithState = WeaponSwithState.Down;       //액티브한거 없으면 다운
                    }
                }
                else if (weaponSwithState == WeaponSwithState.PutUpNew) //도착하면 Up으로 
                {
                    weaponSwithState = WeaponSwithState.Up;
                }
            }


            //지연시간동안 무기 위치 이동 
            if (weaponSwithState == WeaponSwithState.PutDownPrvious)
            {
                weaponMainlocalPosition = Vector3.Lerp(defaultWeaponPostion.localPosition, downWeaponPostion.localPosition, switchingTimeFactor);
            }
            else if (weaponSwithState == WeaponSwithState.PutUpNew)
            {
                weaponMainlocalPosition = Vector3.Lerp(downWeaponPostion.localPosition, defaultWeaponPostion.localPosition, switchingTimeFactor);
            }
        }

        //프리팹으로 생성한 웨폰컨트롤러 오브젝트 추가
        public bool AddWeapon(WeaponController weaponPrefab)
        {
            //추가하는 무기 소지 여부 체크 - 중복검사
            if(HasWeapon(weaponPrefab) != null)
            {
                Debug.Log("같은 무기");
                return false;
            }
            //하나씩 받아옴 //위폰컨트로러를 반환함 
            for(int i=0; i<weaponSlots.Length; i++)
            {
                if(weaponSlots[i]== null)
                {
                    //슬롯에 집어넣음
                    WeaponController weaponInstance = Instantiate(weaponPrefab, weaponParentSocket);
                    weaponInstance.transform.localPosition = Vector3.zero;  //부모위치와 동일한 위치에 설정됨
                    weaponInstance.transform.localRotation = Quaternion.identity;   

                    weaponInstance.Owner = this.gameObject; //이오브젝트가 무기들 장착중 주인
                    weaponInstance.SourcePrefab = weaponPrefab.gameObject; //무기 프리팹 장착 프리팹오브젝트
                    weaponInstance.ShowWeapon(false);

                    weaponSlots[i] = weaponInstance;

                    return true;
                }
            }
            Debug.Log("위폰 펄스 풀");
            return false;
        }
        //매개변수로 들어온
        private WeaponController HasWeapon(WeaponController weaponPrefab)
        {
            for(int i=0; i<weaponSlots.Length; i++)
            {
                //원래 소스 프리팹이 매개변수와 같은 프리팹이면 반환 
                if (weaponSlots[i] != null && weaponSlots[i].SourcePrefab == weaponPrefab )
                {
                    return weaponSlots[i];
                }
            }
            //동일한게 없으면 널 
            return null;
        }

        //현재 활성화된 위폰
        public WeaponController GetActiveWeapon()
        {
            return GetWeaponAtSlotIndex(ActiveWeaponIndex); //이 인덱스에 해당되는 게 액티브 위폰
        }


        //지정된 슬롯에 무기가 있는지 여부 
        public WeaponController GetWeaponAtSlotIndex(int index)
        {
            //렌스보다 큰값이 나오면 인덱스 벗어남
            if(index >= 0 && index < weaponSlots.Length)
            {
                return weaponSlots[index];
            }
            return null;
        }

        //0번부터 9번까지  0,1,2
        //무기 바꾸기, 현재 들고 있는 무기 false, 새로운 무기 true
        public void SwitchWeapon(bool ascendingOrder)
        {
            //새로운 엑티브할 무기 인덱스 
            int newWeaponIndex = -1;

            //가까운 슬롯까지 거리
            int closestSlotDistance = weaponSlots.Length;

            //가장 가까운 슬롯 찾기
            for(int i=0;i<weaponSlots.Length;i++)
            {
                //현재 활성화된 Index 와 i가 틀려야함  //슬롯간 거리 최소 거리 
                if (i != ActiveWeaponIndex && GetWeaponAtSlotIndex(i) != null)  
                {
                    int distanceToActiveIndex = GetDistanceBetweenWeaponSlot(ActiveWeaponIndex, i, ascendingOrder);
                    if(distanceToActiveIndex < closestSlotDistance)
                    {
                        closestSlotDistance = distanceToActiveIndex;    //클로즈 디스턴스가 최소 값이됨
                        newWeaponIndex = i; //새로운 거리가 됨 
                    }
                }
            }
            //새로 액티브할 무기 인덱스로 무기 교체 (제일가까운 인덱스로)
            SwitchToWeaponIndex(newWeaponIndex);
        }

        private void SwitchToWeaponIndex(int newWeaponIndex)
        {
            //newWeaponIndex값 체크 
            if (newWeaponIndex >= 0 && newWeaponIndex != ActiveWeaponIndex)
            {
                weaponSwitchNewIndex = newWeaponIndex;
                weaponSwitchTimeStarted = Time.time;  

                //현재 액티브한 무기가 있느냐?
                if(GetActiveWeapon() == null)
                {
                    //아래 위치한 포지션에 
                    weaponMainlocalPosition = downWeaponPostion.position;
                    weaponSwithState = WeaponSwithState.PutUpNew; //위에서 올라가는
                    ActiveWeaponIndex = newWeaponIndex; //다른걸로 바뀜

                    //가져와서 보여주기 
                    WeaponController weaponController = GetWeaponAtSlotIndex(newWeaponIndex);
                    OnSwitchToWeapon?.Invoke(weaponController);
                }
                else
                {
                    //위에올라간걸 
                    weaponSwithState = WeaponSwithState.PutDownPrvious;
                }

                /*if(ActiveWeaponIndex >= 0) //마이너스가 되면 안됨 
                {
                    WeaponController nowWeapon = GetWeaponAtSlotIndex(ActiveWeaponIndex);
                    nowWeapon.ShowWeapon(false);
                }
                WeaponController newWeapon = GetWeaponAtSlotIndex(newWeaponIndex);
                newWeapon.ShowWeapon(true);
                ActiveWeaponIndex = newWeaponIndex;*/
            }
        }

        //슬롯간 거리,
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
                distanceBetweenSlots += weaponSlots.Length; //거리 마이너스 없음 
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
