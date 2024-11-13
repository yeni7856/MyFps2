using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;
using UnityEditor.Search;
using System;
using UnityEngine.Events;
using UnityEngine.Scripting;
using Unity.VisualScripting;
using Unity.Fps.UI;

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
    /// 플레이어가 가진 WeaponController을 관리 무기들을 관리하는 클래스 
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
        public UnityAction<WeaponController> OnSwitchToWeapon;         //무기 교체시 등록된함수 호출
        public UnityAction<WeaponController, int> OnAddedWeapon;       //무기 추가 할때마다 등록된 함수 호출
        public UnityAction<WeaponController, int> OnRemoveWeapon;     //장착된무기 빠질때마다 등록된 함수 호출 


        private WeaponSwithState weaponSwithState;                              //무기 교체시 상태

        //플레이어 인풋 
        private PlayerInputHandler playerInputHandler;

        //무기 교체시 계산되는 최종위치 
        private Vector3 weaponMainlocalPosition;

        public Transform defaultWeaponPostion;
        public Transform downWeaponPostion;             
        public Transform aimWeaponPostion;                            //조준시 이동할 위치

        private int weaponSwitchNewIndex;                               //새로 바뀌는 무기 인데스 
        private float weaponSwitchTimeStarted = 0;                  //시간
        [SerializeField] private float weaponSwitchDelay = 1f;

        //적 포착
        public bool IsPointingAtEnemy {  get; private set; }        //적포착여부
        public Camera weaponCamera;                                     //weaponCamera에서 Ray로 적 확인
        
        //조준
        //카메라 셋팅 
        private PlayerCharacterController playerCharacterController;
        [SerializeField] private float defaultFov = 60f;                         //카메라 기본 Fov 값
        [SerializeField] private float weaponFovMultiplier = 1f;          //Fov 연산 계수

        public bool IsAiming { get; private set; }                                  //무기 조준 여부
        [SerializeField] private float aimingAnimationSpeed = 10f;      //무기 이동, Fov 연출 Lerp 속도

        //흔들림
        [SerializeField] private float bobFrequency = 10f;
        [SerializeField] private float bobSharpness = 10f;
        [SerializeField] private float defaultBobAmount = 0.05f;     //평상시 흔들림량
        [SerializeField] private float aimingBobAmount = 0.02f;      //조준중 흔들림량

        private float weaponBobFactor;                      //흔들림 갯수
        private Vector3 lastCharacterPosition;            //현재 프레임에서의 이동속도를 구하기 위한 변수
        private Vector3 weaponBobLocalPosition;     //이동시 흔들린량 최종 계산값, 이동하지 않으면 0 

        //반동
        [SerializeField] private float recoilSharpness = 50f;           //이동속도
        [SerializeField] private float maxRecoilDistance = 0.5f;     //반동시 뒤로 밀릴수 있는 최대거리 연사에 의해 뒤로 갈수있음
        private float recoileRepositionSharpness = 10f;                //제자리로 돌아오는 속도
        private Vector3 accumulateRecoil;                                   //반동시 뒤로 밀리는 량

        private Vector3 weaponRecoilLocalPosition;                    // 반동시 이동한 최종 계산값, 반동후 제자리에 돌아오면 0

        //저격 모드
        private bool isScopeOn = false;                                       //저격모드 
        [SerializeField] private float distanceOnScope = 0.1f;

        public UnityAction OnScopedWeapon;                            //저격모드 시작시 등록된 함수 호출
        public UnityAction OffScopedWeapon;                            //저격모드 끝날때 등록된 함수 호출
        #endregion

        private void Start()
        {
            //참조 
            playerInputHandler = GetComponent<PlayerInputHandler>();
            playerCharacterController = GetComponent<PlayerCharacterController>();

            //초기화
            ActiveWeaponIndex = -1;
            weaponSwithState = WeaponSwithState.Down;

            //액티브 무기 Show 함수 등록 
            OnSwitchToWeapon += OnWeaponSwitched;

            //저격 모드 함수 등록
            OnScopedWeapon += OnScope;
            OffScopedWeapon += OffScope;

            //Fov 초기값 설정
            SetFov(defaultFov);

            //지급 받은 무기 장착playerCharacterController
            foreach (var weapon in startingWeapons) //하나씩 꺼내씀
            {
                AddWeapon(weapon);
            }
            //첫번째 무기 활성화
            SwitchWeapon(true);
        }

        private void Update()
        {
            //현재 액티브 무기
            WeaponController activeWeapon = GetActiveWeapon();

           if(weaponSwithState == WeaponSwithState.Up)
            {
                //조준 입력값 처리
                IsAiming = playerInputHandler.GetAimInputHeld();

                //저격 모드 처리
                if(activeWeapon.shootType == WeaponShootType.Sniper)
                {
                    if (playerInputHandler.GetAimInputDown())
                    {
                        //저격 모드 시작
                        isScopeOn = true;
                        //OnScopedWeapon?.Invoke();
                    }
                    if(playerInputHandler.GetAimInputUp())
                    {
                        //저격 모드 끝
                        OffScopedWeapon?.Invoke();
                    }
                }

                //슛처리 
                bool isFire = activeWeapon.HandleShootInputs(
                    playerInputHandler.GetFireInputDown(),
                    playerInputHandler.GetFireInputHeld(),
                    playerInputHandler.GetFireInputUp()
                    );
               if(isFire)   //총알 발사 됬을때
                {
                    //반동효과 
                    accumulateRecoil += Vector3.back * activeWeapon.recoilForce;
                    accumulateRecoil = Vector3.ClampMagnitude(accumulateRecoil, maxRecoilDistance);  //백터3 크기를 클램프 0.5이상 뒤로 밀리지 않게 
                }
            }

            //연출끝나고나서 바꾸기 
            if(!IsAiming && (weaponSwithState == WeaponSwithState.Up || weaponSwithState == WeaponSwithState.Down))
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

            //적포착
            IsPointingAtEnemy = false;
            if (activeWeapon)
            {
                RaycastHit hit;
                //충돌체 위폰카메라 앞방향으로 쏘기
                if (Physics.Raycast(weaponCamera.transform.position, weaponCamera.transform.forward, out hit, 300f))
                {
                    //콜라이더 체크 - 적 판별 (Damageable) 
                    Damageable damageable = hit.collider.GetComponent<Damageable>();
                    if(damageable != null)
                    {
                        IsPointingAtEnemy=true;     //적포착
                    }
                }
            }
        }

        private void LateUpdate()
        {
            UpdateWeaponBob();
            UpdateWeaponRecoil();
            UpdateWeaponAiming();
            UpdateWeaponSwitching();
            //무기 최종 위치 // 최종위치 더하기 흔들린값 더하기 이동하지 않으면 0
           weaponParentSocket.localPosition = weaponMainlocalPosition + weaponBobLocalPosition + weaponRecoilLocalPosition;  
        }

        //반동
        void UpdateWeaponRecoil()
        {
            if(weaponRecoilLocalPosition.z >= accumulateRecoil.z * 0.99f) //accumulateRecoil보다 크면 밀려야함
            {
                //도착했으면
                weaponRecoilLocalPosition = Vector3.Lerp(weaponRecoilLocalPosition, accumulateRecoil,
                    recoilSharpness * Time.deltaTime);
            }
            else
            {
                //제자리로
                weaponRecoilLocalPosition = Vector3.Lerp(weaponRecoilLocalPosition, Vector3.zero,
                    recoileRepositionSharpness * Time.deltaTime);
                accumulateRecoil = weaponRecoilLocalPosition;       //이동한값이랑 맞춰주기 
                
            }
        }

        //카메라 Fov 값 세팅 : 줌인, 줌아웃 Fov 값으로 
        void SetFov(float fov)
        {
            playerCharacterController.PlayerCamera.fieldOfView = fov;
            weaponCamera.fieldOfView = fov * weaponFovMultiplier; //값 곱하기
        }

        //에임 업데이트 무기조준에 따른 연출 : 무기 위치 조정
        void UpdateWeaponAiming()
        {
            //무기를 들고 있을때만 조준 가능
            if (weaponSwithState == WeaponSwithState.Up)
            {
                WeaponController activeWeapon = GetActiveWeapon();

                if (IsAiming && activeWeapon)   //조준시 : 디폴트 -> Aiming 위치로 이동, fov : 디폴트 -> aimZoomRatio
                {
                    weaponMainlocalPosition = Vector3.Lerp(weaponMainlocalPosition,
                        aimWeaponPostion.localPosition + activeWeapon.animOffset,
                        aimingAnimationSpeed * Time.deltaTime);

                    //저격 모드 시작 
                    if (isScopeOn)
                    {
                        //weaponMainlocalPosition 와 목표지점까지의 거리
                        float dist = Vector3.Distance(weaponMainlocalPosition, aimWeaponPostion.localPosition + activeWeapon.animOffset);
                        if(dist < distanceOnScope) //0.1보다 작을때 시작
                        {
                            OnScopedWeapon?.Invoke();
                            isScopeOn = false;  
                        }
                    }
                    else
                    {
                        float fov = Mathf.Lerp(playerCharacterController.PlayerCamera.fieldOfView,
                        activeWeapon.aimZoomRatio * defaultFov, aimingAnimationSpeed * Time.deltaTime);
                        SetFov(fov);
                    }

   /*                 float fov = Mathf.Lerp(playerCharacterController.PlayerCamera.fieldOfView,
                        activeWeapon.aimZoomRatio * defaultFov, aimingAnimationSpeed * Time.deltaTime);
                    SetFov(fov);*/
                   
                }
                else              //조준이 풀렸을때 : Aiming 위치 -> 디폴트 위치로 이동 fov : aimZoomRatio -> 디폴트
                {
                    weaponMainlocalPosition = Vector3.Lerp(weaponMainlocalPosition,
                        defaultWeaponPostion.localPosition,
                        aimingAnimationSpeed * Time.deltaTime);
                    float fov = Mathf.Lerp(playerCharacterController.PlayerCamera.fieldOfView,
                       defaultFov, aimingAnimationSpeed * Time.deltaTime);
                    SetFov(fov);

                }
            }
            
        }

        //이동에 의한 무기 흔들림 값 구하기
        void UpdateWeaponBob()
        {
            if(Time.deltaTime > 0f)
            {
                //플레이어가 한 프레임동안 이동한 거리
                //playerCharacterController.transform.position - lastCharacterPosition
                //현재 프레임에서 플레이어 이동 속도
                Vector3 playerCharacterVelocity = 
                    (playerCharacterController.transform.position - lastCharacterPosition) / Time.deltaTime;    //나누기 시간 하면 현재 프레임 나옴

                float charactorMovementFactor = 0f;
                if (playerCharacterController.IsGrounded)       //땅에 있을때만 흔들리기
                {
                    //Clamp01 0-1사이나오도록 
                    charactorMovementFactor = Mathf.Clamp01(playerCharacterVelocity.magnitude / 
                        (playerCharacterController.MaxSpeedOnGround * playerCharacterController.SprintSpeedModifier));
                }
                //속도에 의한 흔들림 계수
               weaponBobFactor = Mathf.Lerp(weaponBobFactor, charactorMovementFactor, bobSharpness * Time.deltaTime);

                //흔들림량 (조준시, 평상시)
                float bobAmount = IsAiming ? aimingBobAmount : defaultBobAmount;
                float frequency = bobFrequency;
                //좌우, 위아래 흔들림(위아래는 좌우 절반)
                float hBobValue = Mathf.Sin(Time.time * frequency) * bobAmount * weaponBobFactor;
                float vBobValue = ((Mathf.Sin(Time.time * frequency) * 0.5f) + 0.5f ) * bobAmount * weaponBobFactor;

                //좌우, 위아래흔들림 
                weaponBobLocalPosition.x = hBobValue;
                weaponBobLocalPosition.y = Mathf.Abs(vBobValue);  // -값 안나오게 혹시모르니

                //플레이어의 현재 마지막 위치 저장.
                lastCharacterPosition = playerCharacterController.transform.position;
            }
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
                    weaponInstance.ShowWeapon(false);   //무기 비활성화 

                    //무기 장착 //생성된거 + 인덱스번호i
                    OnAddedWeapon?.Invoke(weaponInstance,i);

                    weaponSlots[i] = weaponInstance;            //슬롯에 무기추가 
                    return true;
                }
            }
            Debug.Log("위폰 펄스 풀");
            return false;
        }

        //weaponSlots에 장착된 무기 제거
        public bool RemoveWeapon(WeaponController oldWeapon)
        {
            for(int i=0; i < weaponSlots.Length; i++)
            {
                //같은 무기 찾아서 제거
                if(weaponSlots[i]== oldWeapon)  //같으면
                {
                    weaponSlots[i] = null;      //제거 밑으로가면x 

                    OnRemoveWeapon?.Invoke(oldWeapon,i);        //몇번째 슬롯에 있는지 넘겨주기 //필요한거있음 얘한테 등록 

                    Destroy(oldWeapon.gameObject);                    //없애기

                    //현재 제거한 무기가 active 면 
                    if(i == ActiveWeaponIndex)
                    {
                        SwitchWeapon(true); //새로운 액티브 무기를 찾는다.
                    }
                    return true;    
                }
            }
            return false;
        }

        //매개변수로 들어온 프리팹으로 만든 무기가 있는지 체크
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
            if (newWeapon != null)
            {
                newWeapon.ShowWeapon(true);
            }
        }
        void OnScope()
        {
            weaponCamera.enabled = false;   
        }
        void OffScope()
        {
            weaponCamera.enabled = true;    
        }
    }
}
