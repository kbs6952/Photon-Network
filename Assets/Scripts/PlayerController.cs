using ExitGames.Client.Photon.StructWrapping;
using System;
using System.Collections;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviourPunCallbacks,IPunObservable
{
    [Header("Move")]
    [SerializeField] private float moveSpeed = 5f;      // 플레이어 이동 속도
    private Vector3 moveDir;                            // 이동방향
    private Vector3 moveMent;                           // 실제 움직임
    private Rigidbody rigidbody;

    [Header("View")]
    public Transform viewPoint;                         // 뷰포인트를 통해서 캐릭터의 상하 회전을 구현
    public float mouseSensity = 1f;
    public float verticalRotation;
    private Vector2 mouseInput;
    public bool inverseMouse;                           // 체크되면 마우스 반전
    public Camera cam;                                  // Player 본인이 소유한 카메라
    public GameObject hiddenObject;

    [Header("Jump")]
    [SerializeField] private float jumpPower = 5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector3 groundCheck;
    [SerializeField] private float groundCheckDistance = 5f;
    [SerializeField] private Transform groundCheckPos;
    public bool isGrounded;
    private float yValue;


    [Header("photon Component")]
    PhotonView PV;

    [Header("Player")]
    public int maxHP = 100;
    private int currentHP;
    public bool isPlayerDead;
    private Animator animator;

    public GameObject[] allSkins;           // 캐릭터의 외형 변경
    private int currentSkinIndex;

    private void OnEnable()
    {
        PhotonSetup();
    }
    private void Awake()
    {
        PhotonSetup();
        InitializeComponent();
    }
    private void InitializeComponent()
    {
        PV = GetComponent<PhotonView>();
        rigidbody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }
    private void PhotonSetup()
    {
        currentSkinIndex = UnityEngine.Random.RandomRange(0, allSkins.Length);

        foreach(var skin in allSkins)
        {
            skin.SetActive(false);
        }

        

        if (!photonView.IsMine)         // 내 플레이어가 아니면 카메라를 비활성화
        {
            cam.gameObject.SetActive(false);
            hiddenObject.SetActive(false);
            gameObject.tag = "OtherPlayer";
            hiddenObject.SetActive(true);
        }
        else
        {
            isPlayerDead = false;
            playerUI.deathScreenObj.SetActive(true);

            currentHP = maxHP;
            playerUI.playerHpText.text = $"{currentHP} / {maxHP}";
        }
    }
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        InitailizeAttackInfo();
    }
    void Update()
    {
        if (photonView.IsMine && isPlayerDead) return;      // 플레이어의 소유권이 나이고, 플레이어가 죽음 상태일때 코드를 멈춘다.
       
        if (photonView.IsMine)
        {
            // 플레이어 인풋
            CheckCollider();
            HandleInput();
            HandleView();
            HandleAnimation();
            PlayerAttack();
            ZoomIn();
        }
        else
        {
            if ((transform.position - curPos).sqrMagnitude >= 100)
                transform.position = curPos;
        }
    }        
    private void FixedUpdate()
    {
        Move();
        // Rigidbody AddForce(관성의 힘을 제어해주는 함수) - moveSpeed 만큼만 움직임
        LimitSpeed();
    }
    private void HandleAnimation()
    {
        //speed
        animator.SetFloat("speed", rigidbody.velocity.magnitude);   // 0 ~ 1을 반환
        //isGround
        animator.SetBool("isGrounded", isGrounded);
    }
    private void HandleInput()
    {
        // 캐릭터 이동 방향
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        moveDir = new Vector3(h, 0, v);

        // 캐릭터 회전
        mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSensity;
        moveMent = (transform.forward * moveDir.z) + (transform.right * moveDir.x).normalized;
    }
    private void HandleView()
    {
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + mouseInput.x, transform.eulerAngles.z);

        verticalRotation += mouseInput.y;
        verticalRotation = Math.Clamp(verticalRotation, -60f, 60);

        if (inverseMouse)
        {
            viewPoint.rotation = Quaternion.Euler(verticalRotation, transform.eulerAngles.y, transform.eulerAngles.z);
        }
        else
            viewPoint.rotation = Quaternion.Euler(-verticalRotation, transform.eulerAngles.y, transform.eulerAngles.z);
    }

    private void LimitSpeed()
    {
        // Rigidbody.Velocity : 현재 Rigidbody 객체의 속도
        Vector3 currentSpeed = new Vector3(rigidbody.velocity.x,0,rigidbody.velocity.z);

        if(currentSpeed.magnitude > moveSpeed)
        {
            Vector3 limitSpeed = currentSpeed.normalized * moveSpeed;
            rigidbody.velocity = new Vector3(limitSpeed.x, rigidbody.velocity.y, limitSpeed.z);         // y에 rigidbody.velocity.y를 넣는 이유는 나중에 점프구현할때 꼬이기 떄문에
        }
    }
    private void Move()
    {
        rigidbody.AddForce(moveMent * moveSpeed, ForceMode.Impulse);     // 점점 속도가 빨라지는
    }
    private void ButtonJump()
    {
        if(Input.GetKeyDown(KeyCode.Space)&&isGrounded)
        {
            Jump();
        }
        // 조건문 : 키를 입력 + 현재 상태 땅인지 아닌지
    }
    private void Jump()
    { 
        rigidbody.velocity = new Vector3(rigidbody.velocity.x,0,rigidbody.velocity.z) ;

        rigidbody.AddForce(groundCheckPos.up*jumpPower,ForceMode.Impulse);
    }
    private void CheckCollider()
    {
        isGrounded = Physics.Raycast(groundCheckPos.position, -transform.up, groundCheckDistance, groundLayer);
    }
    #region Player Attack
    public GameObject bulletImpact;         // 플레이어 공격의 피격 효과 인스턴스
    public float fireCoolTIme = 0.1f;
    private float fireCounter;
    public float shootDistance = 10f;
    public float bulletAliveTime = 0.1f;
    public bool isAutomatic;

    [Header("오버히트 시스템")]
    public float maxHeat = 10f, heatCount, heatPerShot;      // 열기를 저장하는 변수
    public float coolRate, overHeatCoolRate;    // 열기를 식히기 위한 변수
    private bool overHeated = false;            // maxHeat에 도달하면 true, heatCount <= 0 다시 false

    public Gun[] allGuns;
    private int currentGunIndex = 0;
    private int currentGunPower;
    private int currentGunFOV;
    private MuzzleFlash currentMuzzle;

    public PlayerUI playerUI;
    private void PlayerAttack()
    {
        CoolDownFuntion();
        SelectGun();
        InputAttack();
        
    }

    private void ZoomIn()
    {
        if (Input.GetMouseButton(1))
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, currentGunFOV, Time.deltaTime * 5);
        }
        else
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView,60,Time.deltaTime * 5);
        }
    }

    private void SelectGun()    // 마우스 휠 버튼을 이용해서 1~ n 까지 등록된 무기를 변경 기능
    {
        if(Input.GetAxisRaw("Mouse ScrollWheel")>0)
        {
            currentGunIndex++;
            if(currentGunIndex >= allGuns.Length)
            {
                currentGunIndex = 0;
            }
            SwitchGunRPC();
            playerUI.SetWeaponSlot(currentGunIndex);
        }
        else if(Input.GetAxisRaw("Mouse ScrollWheel")<0)
        {
            currentGunIndex--;
            if(currentGunIndex < 0)
            {
                currentGunIndex = allGuns.Length-1;
            }
            SwitchGunRPC();
            playerUI.SetWeaponSlot(currentGunIndex);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentGunIndex = 0;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentGunIndex = 1;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currentGunIndex = 2;
        }
        SwitchGunRPC();
        playerUI.SetWeaponSlot(currentGunIndex);
    }



    [PunRPC]
    private void SwitchGunRPC()
    {
        for(int i = 0; i< allGuns.Length; i++)
        {
        // 올 건즈안에 있는 모든 오브젝트 비활성화
            allGuns[i].gameObject.SetActive(false);
        }
        // allguns[현재인덱스] 오브젝트 활성화
        allGuns[currentGunIndex].gameObject.SetActive(true);

        // 건을 매개 변수로 사용하는 gun정보 동기화 함수
        SetGunAttribute(allGuns[currentGunIndex]);
    }
    private void SwitchGun()
    {
        //RPC 함수 호출
        photonView.RPC(nameof(SwitchGunRPC), RpcTarget.AllBuffered);
    }
    private void SetGunAttribute(Gun gun) // Class -> Data
    {
        fireCoolTIme = gun.fireCoolTIme;
        isAutomatic = gun.isAutomatic;
        currentMuzzle=gun.MuzzleFlash.GetComponent<MuzzleFlash>();
        heatPerShot = gun.heatPerShot;
        shootDistance=gun.shootDistance;
        currentGunPower = gun.gunPower;
        currentGunFOV = gun.gunFOV;

        playerUI.currentWeaponSlider.maxValue = maxHeat;
    }

    private void CoolDownFuntion()
    {
        fireCounter -= Time.deltaTime;
        isAutomatic = allGuns[currentGunIndex].isAutomatic;
        OverHeatedCoolDown();
    }
    private void OverHeatedCoolDown()
    {
        // 현재 OverHeat 상태
        if (overHeated)
        {
            heatCount -= overHeatCoolRate * Time.deltaTime;

            if (heatCount <= 0)
            {
                heatCount = 0;
                overHeated = false;
                // UI에서 OverHeat 표시를 해제
                playerUI.overHeatTextObject.SetActive(false);
            }
        }
        else
        {
            heatCount -= coolRate * Time.deltaTime;
            if (heatCount <= 0)
                heatCount = 0;
        }
        playerUI.currentWeaponSlider.value = heatCount;


        // 아닐 때
    }
    private void InputAttack()
    {
        if (Input.GetMouseButtonDown(0)&&!isAutomatic&&!overHeated)
        {
            if (fireCounter <= 0)
                photonView.RPC(nameof(ShootRPC), RpcTarget.AllBuffered);
        }
        if(Input.GetMouseButton(0)&&isAutomatic&&!overHeated)
        {
            if (fireCounter <= 0)
                photonView.RPC(nameof(ShootRPC), RpcTarget.AllBuffered);
        }
    }

    private void InitailizeAttackInfo()
    {
        fireCounter = fireCoolTIme;
        currentGunIndex = 0;
        

        SwitchGunRPC();
    }
    [PunRPC]
    private void ShootRPC()
    {
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, shootDistance))
        {
            // Tag를 이용해서.. player 태그 대상에게 이펙트 발생. 공격을 받았음,. 함수를
            //if (hit.collider.CompareTag("Enemy") && hit.collider.GetComponent<PhotonView>().IsMine)
               //TakeDamageRPC(10);
            
            if(hit.collider.CompareTag("OtherPlayer"))
            {
                hit.collider.gameObject.GetPhotonView().RPC(nameof(TakeDamageRPC), RpcTarget.AllBuffered, photonView.Owner.NickName, 
                    currentGunPower, PhotonNetwork.LocalPlayer.ActorNumber);
            }
            // Raycast가 hit한 지점에 object가 생성된다.
            // 생성된 각도..
            // 생성되는 위치가 오브젝트랑 겹쳐보이는 현상..
            GameObject bulletObject = Instantiate(bulletImpact, hit.point + (hit.normal * 0.02f), Quaternion.LookRotation(hit.normal, Vector3.up));

            // 일정 시간 후에 인스턴스한 오브젝트를 파괴한다.
            Destroy(bulletObject, bulletAliveTime);
        }
        
        // 사격이 끝날 때, 사격 쿨타임을 리셋
        fireCounter = fireCoolTIme;
        // 오버히트값을 계산 함수
        ShootHeatSystem();
    }
    [PunRPC]
    private void TakeDamageRPC(string name,int damage, int actorNumber)
    {
        // 디버그로 받은 데미지 출력
        if (photonView.IsMine)
        {
            Debug.Log($"데미지 입은 대상 : {name}이 {damage}만큼의 피해를 입음.");
            currentHP -= damage;
            isPlayerDead = currentHP <= 0;

            if(isPlayerDead)
            {
                MatchManager.Instance.UpdateStatsSend(actorNumber, 0, 1);
                playerUI.ShowDeathMassage(name);
                SpawnPlayer.instance.Die();
            }
            playerUI.playerHpText.text = $"{currentHP} / {maxHP}";
        }
        
    }
    private void ShootHeatSystem()
    {
        heatCount += heatPerShot;

        if (heatCount >= maxHeat)
        {
            heatCount = maxHeat;
            overHeated = false;
            playerUI.overHeatTextObject.SetActive(true);
        }
    }
    #endregion
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(groundCheckPos.position, groundCheckPos.position + (-transform.up * groundCheckDistance));
        // 플레이어의 사격 범위를 파악하기 위한 기즈모 함수
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(cam.transform.position, cam.transform.forward * shootDistance);
    }
    private Vector3 curPos;
    private float lag;
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // 보낼 정보를 isWriting 작성하면, 그 정보를 isReading으로 읽어온다.
        // 주의사항 : 반드시 보낼 변수의 순서를 똑같이 해줘야 한다.

        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(rigidbody.velocity);
            stream.SendNext(currentHP);
            stream.SendNext(currentGunIndex);
        }
        else if(stream.IsReading)
        {
            curPos = (Vector3)stream.ReceiveNext();
            rigidbody.velocity = (Vector3)stream.ReceiveNext();
            currentHP = (int)stream.ReceiveNext();
            currentGunIndex = (int)stream.ReceiveNext();
        }
        lag = Mathf.Abs((float)(PhotonNetwork.Time - info.timestamp));
    }
}
