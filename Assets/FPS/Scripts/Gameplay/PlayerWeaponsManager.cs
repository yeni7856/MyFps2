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
    /// �÷��̾ ���� WeaponController�� ���� ������� �����ϴ� Ŭ���� 
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
        public UnityAction<WeaponController> OnSwitchToWeapon;         //���� ��ü�� ��ϵ��Լ� ȣ��
        public UnityAction<WeaponController, int> OnAddedWeapon;       //���� �߰� �Ҷ����� ��ϵ� �Լ� ȣ��
        public UnityAction<WeaponController, int> OnRemoveWeapon;     //�����ȹ��� ���������� ��ϵ� �Լ� ȣ�� 


        private WeaponSwithState weaponSwithState;                              //���� ��ü�� ����

        //�÷��̾� ��ǲ 
        private PlayerInputHandler playerInputHandler;

        //���� ��ü�� ���Ǵ� ������ġ 
        private Vector3 weaponMainlocalPosition;

        public Transform defaultWeaponPostion;
        public Transform downWeaponPostion;             
        public Transform aimWeaponPostion;                            //���ؽ� �̵��� ��ġ

        private int weaponSwitchNewIndex;                               //���� �ٲ�� ���� �ε��� 
        private float weaponSwitchTimeStarted = 0;                  //�ð�
        [SerializeField] private float weaponSwitchDelay = 1f;

        //�� ����
        public bool IsPointingAtEnemy {  get; private set; }        //����������
        public Camera weaponCamera;                                     //weaponCamera���� Ray�� �� Ȯ��
        
        //����
        //ī�޶� ���� 
        private PlayerCharacterController playerCharacterController;
        [SerializeField] private float defaultFov = 60f;                         //ī�޶� �⺻ Fov ��
        [SerializeField] private float weaponFovMultiplier = 1f;          //Fov ���� ���

        public bool IsAiming { get; private set; }                                  //���� ���� ����
        [SerializeField] private float aimingAnimationSpeed = 10f;      //���� �̵�, Fov ���� Lerp �ӵ�

        //��鸲
        [SerializeField] private float bobFrequency = 10f;
        [SerializeField] private float bobSharpness = 10f;
        [SerializeField] private float defaultBobAmount = 0.05f;     //���� ��鸲��
        [SerializeField] private float aimingBobAmount = 0.02f;      //������ ��鸲��

        private float weaponBobFactor;                      //��鸲 ����
        private Vector3 lastCharacterPosition;            //���� �����ӿ����� �̵��ӵ��� ���ϱ� ���� ����
        private Vector3 weaponBobLocalPosition;     //�̵��� ��鸰�� ���� ��갪, �̵����� ������ 0 

        //�ݵ�
        [SerializeField] private float recoilSharpness = 50f;           //�̵��ӵ�
        [SerializeField] private float maxRecoilDistance = 0.5f;     //�ݵ��� �ڷ� �и��� �ִ� �ִ�Ÿ� ���翡 ���� �ڷ� ��������
        private float recoileRepositionSharpness = 10f;                //���ڸ��� ���ƿ��� �ӵ�
        private Vector3 accumulateRecoil;                                   //�ݵ��� �ڷ� �и��� ��

        private Vector3 weaponRecoilLocalPosition;                    // �ݵ��� �̵��� ���� ��갪, �ݵ��� ���ڸ��� ���ƿ��� 0

        //���� ���
        private bool isScopeOn = false;                                       //���ݸ�� 
        [SerializeField] private float distanceOnScope = 0.1f;

        public UnityAction OnScopedWeapon;                            //���ݸ�� ���۽� ��ϵ� �Լ� ȣ��
        public UnityAction OffScopedWeapon;                            //���ݸ�� ������ ��ϵ� �Լ� ȣ��
        #endregion

        private void Start()
        {
            //���� 
            playerInputHandler = GetComponent<PlayerInputHandler>();
            playerCharacterController = GetComponent<PlayerCharacterController>();

            //�ʱ�ȭ
            ActiveWeaponIndex = -1;
            weaponSwithState = WeaponSwithState.Down;

            //��Ƽ�� ���� Show �Լ� ��� 
            OnSwitchToWeapon += OnWeaponSwitched;

            //���� ��� �Լ� ���
            OnScopedWeapon += OnScope;
            OffScopedWeapon += OffScope;

            //Fov �ʱⰪ ����
            SetFov(defaultFov);

            //���� ���� ���� ����playerCharacterController
            foreach (var weapon in startingWeapons) //�ϳ��� ������
            {
                AddWeapon(weapon);
            }
            //ù��° ���� Ȱ��ȭ
            SwitchWeapon(true);
        }

        private void Update()
        {
            //���� ��Ƽ�� ����
            WeaponController activeWeapon = GetActiveWeapon();

           if(weaponSwithState == WeaponSwithState.Up)
            {
                //���� �Է°� ó��
                IsAiming = playerInputHandler.GetAimInputHeld();

                //���� ��� ó��
                if(activeWeapon.shootType == WeaponShootType.Sniper)
                {
                    if (playerInputHandler.GetAimInputDown())
                    {
                        //���� ��� ����
                        isScopeOn = true;
                        //OnScopedWeapon?.Invoke();
                    }
                    if(playerInputHandler.GetAimInputUp())
                    {
                        //���� ��� ��
                        OffScopedWeapon?.Invoke();
                    }
                }

                //��ó�� 
                bool isFire = activeWeapon.HandleShootInputs(
                    playerInputHandler.GetFireInputDown(),
                    playerInputHandler.GetFireInputHeld(),
                    playerInputHandler.GetFireInputUp()
                    );
               if(isFire)   //�Ѿ� �߻� ������
                {
                    //�ݵ�ȿ�� 
                    accumulateRecoil += Vector3.back * activeWeapon.recoilForce;
                    accumulateRecoil = Vector3.ClampMagnitude(accumulateRecoil, maxRecoilDistance);  //����3 ũ�⸦ Ŭ���� 0.5�̻� �ڷ� �и��� �ʰ� 
                }
            }

            //���ⳡ������ �ٲٱ� 
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

            //������
            IsPointingAtEnemy = false;
            if (activeWeapon)
            {
                RaycastHit hit;
                //�浹ü ����ī�޶� �չ������� ���
                if (Physics.Raycast(weaponCamera.transform.position, weaponCamera.transform.forward, out hit, 300f))
                {
                    //�ݶ��̴� üũ - �� �Ǻ� (Damageable) 
                    Damageable damageable = hit.collider.GetComponent<Damageable>();
                    if(damageable != null)
                    {
                        IsPointingAtEnemy=true;     //������
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
            //���� ���� ��ġ // ������ġ ���ϱ� ��鸰�� ���ϱ� �̵����� ������ 0
           weaponParentSocket.localPosition = weaponMainlocalPosition + weaponBobLocalPosition + weaponRecoilLocalPosition;  
        }

        //�ݵ�
        void UpdateWeaponRecoil()
        {
            if(weaponRecoilLocalPosition.z >= accumulateRecoil.z * 0.99f) //accumulateRecoil���� ũ�� �з�����
            {
                //����������
                weaponRecoilLocalPosition = Vector3.Lerp(weaponRecoilLocalPosition, accumulateRecoil,
                    recoilSharpness * Time.deltaTime);
            }
            else
            {
                //���ڸ���
                weaponRecoilLocalPosition = Vector3.Lerp(weaponRecoilLocalPosition, Vector3.zero,
                    recoileRepositionSharpness * Time.deltaTime);
                accumulateRecoil = weaponRecoilLocalPosition;       //�̵��Ѱ��̶� �����ֱ� 
                
            }
        }

        //ī�޶� Fov �� ���� : ����, �ܾƿ� Fov ������ 
        void SetFov(float fov)
        {
            playerCharacterController.PlayerCamera.fieldOfView = fov;
            weaponCamera.fieldOfView = fov * weaponFovMultiplier; //�� ���ϱ�
        }

        //���� ������Ʈ �������ؿ� ���� ���� : ���� ��ġ ����
        void UpdateWeaponAiming()
        {
            //���⸦ ��� �������� ���� ����
            if (weaponSwithState == WeaponSwithState.Up)
            {
                WeaponController activeWeapon = GetActiveWeapon();

                if (IsAiming && activeWeapon)   //���ؽ� : ����Ʈ -> Aiming ��ġ�� �̵�, fov : ����Ʈ -> aimZoomRatio
                {
                    weaponMainlocalPosition = Vector3.Lerp(weaponMainlocalPosition,
                        aimWeaponPostion.localPosition + activeWeapon.animOffset,
                        aimingAnimationSpeed * Time.deltaTime);

                    //���� ��� ���� 
                    if (isScopeOn)
                    {
                        //weaponMainlocalPosition �� ��ǥ���������� �Ÿ�
                        float dist = Vector3.Distance(weaponMainlocalPosition, aimWeaponPostion.localPosition + activeWeapon.animOffset);
                        if(dist < distanceOnScope) //0.1���� ������ ����
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
                else              //������ Ǯ������ : Aiming ��ġ -> ����Ʈ ��ġ�� �̵� fov : aimZoomRatio -> ����Ʈ
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

        //�̵��� ���� ���� ��鸲 �� ���ϱ�
        void UpdateWeaponBob()
        {
            if(Time.deltaTime > 0f)
            {
                //�÷��̾ �� �����ӵ��� �̵��� �Ÿ�
                //playerCharacterController.transform.position - lastCharacterPosition
                //���� �����ӿ��� �÷��̾� �̵� �ӵ�
                Vector3 playerCharacterVelocity = 
                    (playerCharacterController.transform.position - lastCharacterPosition) / Time.deltaTime;    //������ �ð� �ϸ� ���� ������ ����

                float charactorMovementFactor = 0f;
                if (playerCharacterController.IsGrounded)       //���� �������� ��鸮��
                {
                    //Clamp01 0-1���̳������� 
                    charactorMovementFactor = Mathf.Clamp01(playerCharacterVelocity.magnitude / 
                        (playerCharacterController.MaxSpeedOnGround * playerCharacterController.SprintSpeedModifier));
                }
                //�ӵ��� ���� ��鸲 ���
               weaponBobFactor = Mathf.Lerp(weaponBobFactor, charactorMovementFactor, bobSharpness * Time.deltaTime);

                //��鸲�� (���ؽ�, ����)
                float bobAmount = IsAiming ? aimingBobAmount : defaultBobAmount;
                float frequency = bobFrequency;
                //�¿�, ���Ʒ� ��鸲(���Ʒ��� �¿� ����)
                float hBobValue = Mathf.Sin(Time.time * frequency) * bobAmount * weaponBobFactor;
                float vBobValue = ((Mathf.Sin(Time.time * frequency) * 0.5f) + 0.5f ) * bobAmount * weaponBobFactor;

                //�¿�, ���Ʒ���鸲 
                weaponBobLocalPosition.x = hBobValue;
                weaponBobLocalPosition.y = Mathf.Abs(vBobValue);  // -�� �ȳ����� Ȥ�ø𸣴�

                //�÷��̾��� ���� ������ ��ġ ����.
                lastCharacterPosition = playerCharacterController.transform.position;
            }
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
                    weaponInstance.ShowWeapon(false);   //���� ��Ȱ��ȭ 

                    //���� ���� //�����Ȱ� + �ε�����ȣi
                    OnAddedWeapon?.Invoke(weaponInstance,i);

                    weaponSlots[i] = weaponInstance;            //���Կ� �����߰� 
                    return true;
                }
            }
            Debug.Log("���� �޽� Ǯ");
            return false;
        }

        //weaponSlots�� ������ ���� ����
        public bool RemoveWeapon(WeaponController oldWeapon)
        {
            for(int i=0; i < weaponSlots.Length; i++)
            {
                //���� ���� ã�Ƽ� ����
                if(weaponSlots[i]== oldWeapon)  //������
                {
                    weaponSlots[i] = null;      //���� �����ΰ���x 

                    OnRemoveWeapon?.Invoke(oldWeapon,i);        //���° ���Կ� �ִ��� �Ѱ��ֱ� //�ʿ��Ѱ����� ������ ��� 

                    Destroy(oldWeapon.gameObject);                    //���ֱ�

                    //���� ������ ���Ⱑ active �� 
                    if(i == ActiveWeaponIndex)
                    {
                        SwitchWeapon(true); //���ο� ��Ƽ�� ���⸦ ã�´�.
                    }
                    return true;    
                }
            }
            return false;
        }

        //�Ű������� ���� ���������� ���� ���Ⱑ �ִ��� üũ
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
