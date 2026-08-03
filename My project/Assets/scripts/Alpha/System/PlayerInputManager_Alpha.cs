using UnityEngine;
using UnityEngine.InputSystem;

namespace Alpha.Managers
{
    public class PlayerInputManager_Alpha : MonoBehaviour
    {
        public static PlayerInputManager_Alpha Instance { get; private set; }

        private PlayerInput playerInput;

        // 値の公開プロパティ
        public Vector2 MoveVector { get; private set; }
        public Vector2 AimVector { get; private set; }
        public bool IsFiring { get; private set; }
        public bool IsSpecialPressed { get; private set; } 
        
        public bool WasSpecialPressed { get; private set; }
        public bool WasWeaponPrevPressed { get; private set; }
        public bool WasWeaponNextPressed { get; private set; }
        public bool WasPausePressed { get; private set; }

        public bool IsUsingGamepad { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            playerInput = new PlayerInput();

            InputSystem.onActionChange += (obj, change) =>
            {
                if (change == InputActionChange.ActionPerformed)
                {
                    InputAction action = (InputAction)obj;
                    InputDevice device = action.activeControl.device;
                    if (device is Gamepad)
                    {
                        IsUsingGamepad = true;
                    }
                    else if (device is Keyboard || device is Mouse)
                    {
                        IsUsingGamepad = false;
                    }
                }
            };
        }

        private void OnEnable()
        {
            playerInput.Enable();
        }

        private void OnDisable()
        {
            playerInput.Disable();
        }

        private void Update()
        {
            MoveVector = playerInput.Player.Move.ReadValue<Vector2>();
            AimVector = playerInput.Player.Aim.ReadValue<Vector2>();
            IsFiring = playerInput.Player.Fire.ReadValue<float>() >= 0.5f;
            IsSpecialPressed = playerInput.Player.Special.ReadValue<float>() >= 0.5f;

            WasSpecialPressed = playerInput.Player.Special.WasPressedThisFrame();
            WasWeaponPrevPressed = playerInput.Player.WeaponPrev.WasPressedThisFrame();
            WasWeaponNextPressed = playerInput.Player.WeaponNext.WasPressedThisFrame();
            WasPausePressed = playerInput.Player.Pause.WasPressedThisFrame();
        }

        public Vector3 GetWorldAimPosition(Vector3 playerPosition, Camera mainCamera = null)
        {
            if (mainCamera == null) mainCamera = Camera.main;

            if (IsUsingGamepad)
            {
                if (AimVector.sqrMagnitude > 0.01f)
                {
                    return playerPosition + new Vector3(AimVector.x, AimVector.y, 0f);
                }
                else if (MoveVector.sqrMagnitude > 0.01f)
                {
                    return playerPosition + new Vector3(MoveVector.x, MoveVector.y, 0f);
                }
                return playerPosition;
            }
            else
            {
                Vector3 mouseScreenPos = new Vector3(AimVector.x, AimVector.y, Mathf.Abs(mainCamera.transform.position.z));
                Vector3 worldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
                worldPos.z = 0f;
                return worldPos;
            }
        }
    }
}
