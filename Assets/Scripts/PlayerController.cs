using ExitGames.Client.Photon.StructWrapping;
using System;
using System.Collections;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviourPunCallbacks
{
    [Header("Move")]
    [SerializeField] private float moveSpeed = 5f;      // �÷��̾� �̵� ����
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

        moveDir = new Vector3(h, 0, v).normalized;

        // ĳ���� ȸ��
        mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSensity;
        moveMent = (transform.forward * moveDir.z) + (transform.right * moveDir.z).normalized;
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
    public float shootDistance = 1f;

    private void PlayerAttack()
    {
        InputAttack();   
    }
    private void InputAttack()
    {
        fireCounter -= Time.deltaTime;
        if (Input.GetMouseButtonDown(0))
        {
            if (fireCounter <= 0)
                Shoot();
        }
    }

    private void InitailizeAttackInfo()
    {
        fireCounter = fireCoolTIme;
    }

    private void Shoot()
    {
        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, 100f))
        {
            // Raycast�� hit�� ������ object�� �����ȴ�.
            // ������ ����..
            // �����Ǵ� ��ġ�� ������Ʈ�� ���ĺ��̴� ����..
            GameObject bulletObject = Instantiate(bulletImpact, hit.point + (hit.normal * 0.02f), Quaternion.LookRotation(hit.normal, Vector3.up));

            // ���� �ð� �Ŀ� �ν��Ͻ��� ������Ʈ�� �ı��Ѵ�.
            Destroy(bulletObject, 1f);

        }

        Debug.Log($"�浹�� ������Ʈ�� �̸� :{hit.collider.gameObject.name}");        // raycasthit�� ���� �浹�� ������ collider�� ������ ��ȯ���ִ� �ڵ�
        Debug.Log($"�浹�� ������ Vector3�� ��ȯ�Ѵ� : {hit.point}");                // Raycast�� ���ؼ� ������ ��ġ�� ��ȯ
        Debug.Log($"ī�޶�� �浹�� ���� ������ �Ÿ��� ��ȯ : {hit.distance}");      // �� ������ ����
        Debug.Log($"�浹�� ������Ʈ�� ����(normal)�� ��ȯ : {hit.normal}");          // cam - �浹�� ������Ʈ ����� ���� ����..normal

        // ����� ���� ��, ��� ��Ÿ���� ����
        fireCounter = fireCoolTIme;
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
