using System.Collections;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

namespace Objects.PlayerScripts
{
    public class PlayerControllerWithCC : MonoBehaviourPun
    {
        [Header("Move settings")]
        [SerializeField] private float moveSpeed = 5.0f;
        [SerializeField] private float jumpForce = 2f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundDistance = 0.4f;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float cameraSensitivity = 2f;
        [SerializeField] private float slopeForce = 5.0f;
        [SerializeField] private float slopeForceRayLength = 1.5f;

        [Header("Footstep Sounds")]
        [SerializeField] private AudioClip[] dirtClips;
        [SerializeField] private AudioClip[] concreteClips;
        [SerializeField] private AudioClip[] metalClips;
        [SerializeField] private AudioClip[] defaultClips;
        [SerializeField] private float stepInterval = 0.5f;
        [SerializeField] private AudioClip jumpSound;

        [Header("MVC")] 
        [SerializeField] private CharacterView view;

        [Header("Params")] 
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int maxMana = 100;
        [SerializeField] private int maxArmor = 100;
        [SerializeField] private int manaRegenAmount = 1;
        [SerializeField] private float manaRegenInterval = 1f;

        [Header("UI")] 
        [SerializeField] private GameObject canvas;
        
        [Header("Animations")] 
        [SerializeField] private PlayerAnimationController animationController;
        private bool isWalking;
        private float walkingSpeed;
        
        private CharacterController controller;
        private Vector3 velocity;
        private bool isGrounded;
        private float stepTimer = 0.0f;
        private AudioSource audioSource;
        
        private Dictionary<PhysicMaterial, AudioClip[]> materialSounds;
        
        private float _rotationX;
        private PhotonView _photonView;
        private CharacterModel model;
        private bool damaged;

        private void Start()
        {
            model = gameObject.AddComponent<CharacterModel>();
            model.Initialize(maxHealth, maxMana, maxArmor, view);
            
            controller = GetComponent<CharacterController>();
            _photonView = GetComponent<PhotonView>();

            view.UpdateHealthText(model.Health);
            view.UpdateManaText(model.Mana);
            view.UpdateArmorText(model.Armor);

            audioSource = GetComponent<AudioSource>();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            if (!_photonView.IsMine)
            {
                Destroy(GetComponentInChildren<Camera>().gameObject);
                Destroy(controller);
                Destroy(canvas);
            }
            else
            {
                StartCoroutine(RegenerateMana());
                DisableRendererForLocalPlayer(); 
            }

            materialSounds = new Dictionary<PhysicMaterial, AudioClip[]>
            {
                { Resources.Load<PhysicMaterial>("Materials/PhysicalMaterials/Dirt"), dirtClips },
                { Resources.Load<PhysicMaterial>("Materials/PhysicalMaterials/Concrete"), concreteClips },
                { Resources.Load<PhysicMaterial>("Materials/PhysicalMaterials/Metal"), metalClips }
            };
        }

        private void Update()
        {
            if (!_photonView.IsMine) return;
            
            RotatePlayerRightLeft();
            RotateCameraUpDown();
            
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                TryJump();
                AudioSource.PlayClipAtPoint(jumpSound, gameObject.transform.position);
            }
        }

        private void TryJump()
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        private void FixedUpdate()
        {
            if (!_photonView.IsMine) return;
            
            PlayerMovement();
        }

        private void PlayerMovement()
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            Vector3 moveDirection = transform.forward * horizontalInput + transform.right * -verticalInput;
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);

            walkingSpeed = new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude;
            isWalking = walkingSpeed > 0.1f;
            
            animationController.SetWalking(Mathf.Lerp(0f, 1f,walkingSpeed));
            
            if (isGrounded && isWalking)
            {
                stepTimer += Time.deltaTime;
                if (stepTimer >= stepInterval)
                {
                    photonView.RPC("PlayFootstepSound", RpcTarget.All);
                    stepTimer = 0f;
                }
            }

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            if ((horizontalInput != 0 || verticalInput != 0) && OnSlope())
            {
                moveDirection += Vector3.down * slopeForce;
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

        private bool OnSlope()
        {
            if (isGrounded)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, Vector3.down, out hit, controller.height / 2 * slopeForceRayLength))
                {
                    if (hit.normal != Vector3.up)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void RotatePlayerRightLeft()
        {
            transform.Rotate(Vector3.up, Input.GetAxisRaw("Mouse X") * cameraSensitivity);
        }
 
        private void RotateCameraUpDown()
        {
            _rotationX -= cameraSensitivity * Input.GetAxisRaw("Mouse Y");
            _rotationX = Mathf.Clamp(_rotationX, -75, 75);
            playerCamera.transform.eulerAngles = new Vector3(_rotationX, 
                playerCamera.transform.eulerAngles.y, playerCamera.transform.eulerAngles.z);
        }
        
        private void DisableRendererForLocalPlayer()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                if (renderer.CompareTag("PlayerWeapon")) continue;
                renderer.enabled = false;
            }
        }

        [PunRPC]
        private void PlayFootstepSound()
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 2.5f))
            {
                PhysicMaterial material = hit.collider.sharedMaterial;
                AudioClip[] clips;

                if (material != null && materialSounds.TryGetValue(material, out clips))
                {
                    // Material-specific clips found
                }
                else
                {
                    // Use default clips if material is not found
                    clips = defaultClips;
                }

                if (clips.Length > 0)
                {
                    AudioClip clip = clips[Random.Range(0, clips.Length)];
                    audioSource.clip = clip;
                    audioSource.volume = 0.5f;
                    audioSource.Play();
                }
            }
        }

        private IEnumerator RegenerateMana()
        {
            while (true)
            {
                yield return new WaitForSeconds(manaRegenInterval);
                if (model.Mana < maxMana)
                {
                    int newMana = Mathf.Min(model.Mana + manaRegenAmount, maxMana);
                    model.AddMana(newMana - model.Mana);
                }
            }
        }
    }
}