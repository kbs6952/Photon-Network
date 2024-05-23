using ExitGames.Client.Photon.StructWrapping;
using System;
using System.Collections;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviourPunCallbacks
{
    [Header("Move")]
    [SerializeField] private float moveSpeed = 5f;      // �÷��̾� �̵� �ӵ�
    private Vector3 moveDir;                            // �̵�����
    private Vector3 moveMent;                           // ���� ������
    private Rigidbody rigidbody;

    [Header("View")]
    public Transform viewPoint;                         // ������Ʈ�� ���ؼ� ĳ������ ���� ȸ���� ����
    public float mouseSensity = 1f;
    public float verticalRotation;
    private Vector2 mouseInput;
    public bool inverseMouse;                           // üũ�Ǹ� ���콺 ����
    public Camera cam;                                  // Player ������ ������ ī�޶�
    public GameObject hiddenObject;

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
        if (!photonView.IsMine)         // �� �÷��̾ �ƴϸ� ī�޶� ��Ȱ��ȭ
        {
            cam.gameObject.SetActive(false);
            hiddenObject.SetActive(false);
        }
        else
        {
            cam.gameObject.SetActive(true);
            if (hiddenObject != null)
                hiddenObject.SetActive(false);
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
            // �÷��̾� ��ǲ
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
        // Rigidbody AddForce(������ ���� �������ִ� �Լ�) - moveSpeed ��ŭ�� ������
        LimitSpeed();
    }

    private void HandleInput()
    {
        // ĳ���� �̵� ����
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        moveDir = new Vector3(h, 0, v);

        // ĳ���� ȸ��
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
        // Rigidbody.Velocity : ���� Rigidbody ��ü�� �ӵ�
        Vector3 currentSpeed = new Vector3(rigidbody.velocity.x,0,rigidbody.velocity.z);

        if(currentSpeed.magnitude > moveSpeed)
        {
            Vector3 limitSpeed = currentSpeed.normalized * moveSpeed;
            rigidbody.velocity = new Vector3(limitSpeed.x, rigidbody.velocity.y, limitSpeed.z);         // y�� rigidbody.velocity.y�� �ִ� ������ ���߿� ���������Ҷ� ���̱� ������
        }
    }
    private void Move()
    {
        rigidbody.AddForce(moveMent * moveSpeed, ForceMode.Impulse);     // ���� �ӵ��� ��������
    }
    private void ButtonJump()
    {
        if(Input.GetKeyDown(KeyCode.Space)&&isGrounded)
        {
            Jump();
        }
        // ���ǹ� : Ű�� �Է� + ���� ���� ������ �ƴ���
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
    public GameObject bulletImpact;         // �÷��̾� ������ �ǰ� ȿ�� �ν��Ͻ�
    public float fireCoolTIme = 0.1f;
    private float fireCounter;
    public float shootDistance = 10f;
    public float bulletAliveTime = 0.1f;
    public bool isAutomatic;

    [Header("������Ʈ �ý���")]
    public float maxHeat = 10f, heatCount, heatPerShot;      // ���⸦ �����ϴ� ����
    public float coolRate, overHeatCoolRate;    // ���⸦ ������ ���� ����
    private bool overHeated = false;            // maxHeat�� �����ϸ� true, heatCount <= 0 �ٽ� false

    public Gun[] allGuns;
    private int currentGunIndex = 0;
    private MuzzleFlash currentMuzzle;

    public PlayerUI playerUI;
    private void PlayerAttack()
    {
        CoolDownFuntion();
        SelectGun();
        InputAttack();   
    }

    private void SelectGun()    // ���콺 �� ��ư�� �̿��ؼ� 1~ n ���� ��ϵ� ���⸦ ���� ���
    {
        if(Input.GetAxisRaw("Mouse ScrollWheel")>0)
        {
            currentGunIndex++;
            if(currentGunIndex >= allGuns.Length)
            {
                currentGunIndex = 0;
            }
            SwitchGun();
            playerUI.SetWeaponSlot(currentGunIndex);
        }
        else if(Input.GetAxisRaw("Mouse ScrollWheel")<0)
        {
            currentGunIndex--;
            if(currentGunIndex < 0)
            {
                currentGunIndex = allGuns.Length-1;
            }
            SwitchGun();
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
        SwitchGun();
        playerUI.SetWeaponSlot(currentGunIndex);
    }
        
   

    private void SwitchGun()
    {
        for(int i = 0; i< allGuns.Length; i++)
        {
        // �� ����ȿ� �ִ� ��� ������Ʈ ��Ȱ��ȭ
            allGuns[i].gameObject.SetActive(false);
        }
        // allguns[�����ε���] ������Ʈ Ȱ��ȭ
        allGuns[currentGunIndex].gameObject.SetActive(true);

        // ���� �Ű� ������ ����ϴ� gun���� ����ȭ �Լ�
        SetGunAttribute(allGuns[currentGunIndex]);
    }
    private void SetGunAttribute(Gun gun) // Class -> Data
    {
        fireCoolTIme = gun.fireCoolTIme;
        isAutomatic = gun.isAutomatic;
        currentMuzzle=gun.MuzzleFlash.GetComponent<MuzzleFlash>();
        heatPerShot = gun.heatPerShot;
        shootDistance=gun.shootDistance;

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
        // ���� OverHeat ����
        if (overHeated)
        {
            heatCount -= overHeatCoolRate * Time.deltaTime;

            if (heatCount <= 0)
            {
                heatCount = 0;
                overHeated = false;
                // UI���� OverHeat ǥ�ø� ����
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


        // �ƴ� ��
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
        
        SwitchGun();
    }
    [PunRPC]
    private void ShootRPC()
    {
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, shootDistance))
        {
            
            // Tag�� �̿��ؼ�.. player �±� ��󿡰� ����Ʈ �߻�. ������ �޾���,. �Լ���
            if(hit.collider.CompareTag("Enemy") &&hit.collider.GetComponent<PhotonView>().IsMine)
            {
                TakeDamage(10);
            }
            


            // Raycast�� hit�� ������ object�� �����ȴ�.
            // ������ ����..
            // �����Ǵ� ��ġ�� ������Ʈ�� ���ĺ��̴� ����..
            GameObject bulletObject = Instantiate(bulletImpact, hit.point + (hit.normal * 0.02f), Quaternion.LookRotation(hit.normal, Vector3.up));

            // ���� �ð� �Ŀ� �ν��Ͻ��� ������Ʈ�� �ı��Ѵ�.
            Destroy(bulletObject, bulletAliveTime);
        }
        currentMuzzle.gameObject.SetActive(true);
        // ����� ���� ��, ��� ��Ÿ���� ����
        fireCounter = fireCoolTIme;
        // ������Ʈ���� ��� �Լ�
        ShootHeatSystem();
    }

    private void TakeDamage(int damage)
    {
        // ����׷� ���� ������ ���
        Debug.Log($"{damage}��ŭ�� ���ظ� �־����ϴ�.");
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
        Gizmos.DrawLine(transform.position, transform.position + (-transform.up * groundCheckDistance));
        // �÷��̾��� ��� ������ �ľ��ϱ� ���� ����� �Լ�
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(cam.transform.position, cam.transform.forward * shootDistance);
    }
}
