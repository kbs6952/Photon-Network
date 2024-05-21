using ExitGames.Client.Photon.StructWrapping;
using System;
using System.Collections;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviourPunCallbacks
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

    [Header("Jump")]
    [SerializeField] private float jumpPower = 5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector3 groundCheck;
    [SerializeField] private float groundCheckDistance = 5f;
    public bool isGrounded;
    private float yValue;


    [Header("photon Component")]
    PhotonView PV;
    private void Awake()
    {
        PhotonSetup();
        InitializeComponent();
    }
    private void InitializeComponent()
    {
        PV = GetComponent<PhotonView>();
        rigidbody = GetComponent<Rigidbody>();
    }
    private void PhotonSetup()
    {
        if (!photonView.IsMine)         // 내 플레이어가 아니면 카메라를 비활성화
        {
            cam.gameObject.SetActive(false);
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
        if(photonView.IsMine)
        {
            // 플레이어 인풋
            CheckCollider();
            ButtonJump();
            HandleInput();
            HandleView();

            PlayerAttack();
        }
    }
    private void FixedUpdate()
    {
        Move();
        // Rigidbody AddForce(관성의 힘을 제어해주는 함수) - moveSpeed 만큼만 움직임
        LimitSpeed();
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

        rigidbody.AddForce(transform.up*jumpPower,ForceMode.Impulse);
    }
    private void CheckCollider()
    {
        isGrounded = Physics.Raycast(transform.position, -transform.up, groundCheckDistance, groundLayer);
    }
    #region Player Attack
    public GameObject bulletImpact;         // 플레이어 공격의 피격 효과 인스턴스
    public float fireCoolTIme = 0.1f;
    private float fireCounter;
    public float shootDistance = 1f;
    public float bulletAliveTime = 0.1f;
    private MuzzleFlash currentMuzzle;
    public bool isAutomatic;

    public Gun[] allGuns;
    private int currentGunIndex = 0;
    private void PlayerAttack()
    {
        CoolDownFuntion();
        SelectGun();
        InputAttack();   
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
            SwitchGun();
        }
        else if(Input.GetAxisRaw("Mouse ScrollWheel")<0)
        {
            currentGunIndex--;
            if(currentGunIndex < allGuns.Length)
            {
                currentGunIndex = 3;
            }
            SwitchGun();
        }
            

        
    }

    private void SwitchGun()
    {
        for(int i = 0; i< allGuns.Length; i++)
        {
            allGuns[i].gameObject.SetActive(false);
        }
        allGuns[currentGunIndex].gameObject.SetActive(true);
        // 올 건즈안에 있는 모든 오브젝트 비활성화
        // allguns[현재인덱스] 오브젝트 활성화

        // 건을 매개 변수로 사용하는 gun정보 동기화 함수
        
        SetGunAttribute(allGuns[currentGunIndex]);
    }
    private void SetGunAttribute(Gun gun) // Class -> Data
    {
        fireCoolTIme = gun.fireCoolTIme;
        
    }

    private void CoolDownFuntion()
    {
        fireCounter -= Time.deltaTime;
        isAutomatic = allGuns[currentGunIndex].isAutomatic;
    }

    private void InputAttack()
    {
        if (Input.GetMouseButton(0))
        {
            if (fireCounter <= 0)
                Shoot();
        }
    }

    private void InitailizeAttackInfo()
    {
        fireCounter = fireCoolTIme;
        currentGunIndex = 0;
        SwitchGun();


    }

    private void Shoot()
    {
        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, 100f))
        {
            // Raycast가 hit한 지점에 object가 생성된다.
            // 생성된 각도..
            // 생성되는 위치가 오브젝트랑 겹쳐보이는 현상..
            GameObject bulletObject = Instantiate(bulletImpact, hit.point + (hit.normal * 0.02f), Quaternion.LookRotation(hit.normal, Vector3.up));

            currentMuzzle.gameObject.SetActive(true);
            // 일정 시간 후에 인스턴스한 오브젝트를 파괴한다.
            Destroy(bulletObject, bulletAliveTime);

        }

        //Debug.Log($"충돌한 오브젝트의 이름 :{hit.collider.gameObject.name}");        // raycasthit에 의해 충돌한 지점에 collider가 있으면 반환해주는 코드
        Debug.Log($"충돌한 지점의 Vector3를 반환한다 : {hit.point}");                // Raycast에 의해서 감지된 위치를 반환
        Debug.Log($"카메라와 충돌한 지점 사이의 거리를 반환 : {hit.distance}");      // 두 벡터의 차이
        Debug.Log($"충돌한 오브젝트의 법선(normal)을 반환 : {hit.normal}");          // cam - 충돌한 오브젝트 평면의 벡터 외적..normal

        // 사격이 끝날 때, 사격 쿨타임을 리셋
        fireCounter = fireCoolTIme;
    }
    #endregion
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (-transform.up * groundCheckDistance));
        // 플레이어의 사격 범위를 파악하기 위한 기즈모 함수
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(cam.transform.position, cam.transform.forward * shootDistance);
    }
}
