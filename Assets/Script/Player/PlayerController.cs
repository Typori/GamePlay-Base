using System;
using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;


namespace TyporiGame
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Player")] [Tooltip("角色移动速度（m/s）")]
        public float MoveSpeed = 2.0f;

        [Tooltip("角色冲刺速度（m/s）")] public float SprintSpeed = 5.335f;

        [Tooltip("角色转向面对运动方向的速度有多快")] [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("加速度")] public float SpeedChangeRate = 10.0f;

        [Space(10)] [Tooltip("跳跃次数 -> 每次的跳跃高度")]
        public float[] JumpHeight = new []{1.2f, 1};

        [Tooltip("角色自身重力，默认值-9.81f")] public float Gravity = -15.0f;

        [Space(10)] [Tooltip("重置跳跃的最小间隔")] public float JumpTimeout = 0.05f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")] [Tooltip("独立的地面检测")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")] public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("地面检测Layer")]
        public LayerMask GroundLayers;


        private PlayerInput _playerInput;
        private GameObject _mainCamera;
        private PlayerInputs _input;
        private CharacterController _controller;
        private float _speed;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        private void Awake()
        {
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        void Start()
        {
            _input = GetComponent<PlayerInputs>();
            _controller = GetComponent<CharacterController>();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        void Update()
        {
            JumpAndGravity();
            GroundedCheck();
            Move();
        }

        void Move()
        {
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
            //如果没有输入，则应用速度0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            //把当前的水平速度算成标量，记录下来
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;

            //加速/减速到目标速度
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed,
                    Time.deltaTime * SpeedChangeRate);

                // 四舍五入到小数点后三位
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            // 输入方向归一化
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            //角色朝向调整
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            //移动角色
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset 好像是设置一个球形碰撞体作为当前物体，看在Layer中是否有物体碰到
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
        }


        // [Tooltip("可以跳跃的次数")] public int JumpCount = 2;
        private int _currentJumpCount;

        /// <summary>
        /// 这里面只为计算纵向速度，最终的移动操作还是放在Move中一块进行了
        /// </summary>
        private void JumpAndGravity()
        {
            if (Grounded)
            {
                //重制已跳跃次数，用重复跳跃时间稍微卡一下，防止因地面检测精度而刚起跳就重置跳跃次数
                if (_currentJumpCount > 0 && _jumpTimeoutDelta <= 0.0f) _currentJumpCount = 0;

                // 只是为了记录下落时间，以切换动画
                _fallTimeoutDelta = FallTimeout;

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                //重制跳跃间隔，对于判定也很有用
                // _jumpTimeoutDelta = JumpTimeout;

                // 只是为了记录下落时间，以切换动画
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                }

                //不在地面上，就要把跳跃设为关闭 - 废弃，因为要二段跳
                // _input.jump = false;
            }

            // 跳跃操作计算
            if (JumpHeight.Length != 0)
            {
                // if (_currentJumpCount == JumpHeight.Length) _currentJumpCount = 0;
                if (_input.jump && _currentJumpCount < JumpHeight.Length)
                {
                    // H * -2 * G的平方 = 达到目标高度所需的速度，由已知加速度算总路程的公式d=1/2(a(t^2))可以推出
                    _verticalVelocity = Mathf.Sqrt(JumpHeight[_currentJumpCount] * -2f * Gravity);
                    _currentJumpCount++;
                    _input.jump = false;

                    //重制跳跃间隔，对于判定也很有用
                    _jumpTimeoutDelta = JumpTimeout;
                    Debug.Log("_currentJumpCount = " + _currentJumpCount);
                }
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            //纵向速度受重力影响，但设置了一个最大值。但解释中的twice没懂
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }
    }
}