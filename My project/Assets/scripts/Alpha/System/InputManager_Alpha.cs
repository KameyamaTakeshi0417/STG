using UnityEngine;
using UnityEngine.InputSystem;

namespace Alpha.Managers
{
    public class InputManager_Alpha : MonoBehaviour
    {
        public static InputManager_Alpha Instance { get; private set; }

        private PlayerInput playerInput;

        // 値の公開プロパティ
        public Vector2 MoveVector { get; private set; }
        public Vector2 AimVector { get; private set; }
        public bool IsFiring { get; private set; }
        public bool IsSpecialPressed { get; private set; } // フォーカス等のホールド判定用
        
        // 特殊アクションや武器切り替えは押された瞬間（Trigger）を判定するため、
        // GetKeyDownのような「そのフレームで押されたか」を管理するフラグ
        public bool WasSpecialPressed { get; private set; }
        public bool WasWeaponPrevPressed { get; private set; }
        public bool WasWeaponNextPressed { get; private set; }
        public bool WasPausePressed { get; private set; }

        // 現在の操作がゲームパッドかどうか（Aimが絶対座標か方向ベクトルかの判定に使用）
        public bool IsUsingGamepad { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーン遷移でも破棄されないようにする

            playerInput = new PlayerInput();

            // デバイスの切り替えを検知して IsUsingGamepad を更新する
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
            // 毎フレームの継続入力を取得
            MoveVector = playerInput.Player.Move.ReadValue<Vector2>();
            AimVector = playerInput.Player.Aim.ReadValue<Vector2>();
            IsFiring = playerInput.Player.Fire.ReadValue<float>() >= 0.5f;
            IsSpecialPressed = playerInput.Player.Special.ReadValue<float>() >= 0.5f;

            // 押された瞬間（Down）の取得
            WasSpecialPressed = playerInput.Player.Special.WasPressedThisFrame();
            WasWeaponPrevPressed = playerInput.Player.WeaponPrev.WasPressedThisFrame();
            WasWeaponNextPressed = playerInput.Player.WeaponNext.WasPressedThisFrame();
            WasPausePressed = playerInput.Player.Pause.WasPressedThisFrame();
        }

        /// <summary>
        /// 自機位置を基準に、Aimの対象となるワールド座標を返す（マウス/右スティック自動判別）
        /// </summary>
        public Vector3 GetWorldAimPosition(Vector3 playerPosition, Camera mainCamera = null)
        {
            if (mainCamera == null) mainCamera = Camera.main;

            if (IsUsingGamepad)
            {
                // ゲームパッド（右スティック）の場合、AimVectorは方向(-1.0 ~ 1.0)
                if (AimVector.sqrMagnitude > 0.01f)
                {
                    // プレイヤーの位置からスティックの方向に少し離れた位置を返す（方向計算用）
                    return playerPosition + new Vector3(AimVector.x, AimVector.y, 0f);
                }
                else if (MoveVector.sqrMagnitude > 0.01f)
                {
                    // 右スティックを入力しておらず、左スティック（移動）をしている場合は移動方向を向く
                    return playerPosition + new Vector3(MoveVector.x, MoveVector.y, 0f);
                }
                // どちらも入力がない場合は現在の向きを維持するため、とりあえず今の位置を返す
                return playerPosition;
            }
            else
            {
                // マウスの場合、AimVectorはスクリーン座標
                Vector3 mouseScreenPos = new Vector3(AimVector.x, AimVector.y, Mathf.Abs(mainCamera.transform.position.z));
                Vector3 worldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
                worldPos.z = 0f;
                return worldPos;
            }
        }
    }
}
