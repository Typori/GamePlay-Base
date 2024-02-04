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


        private PlayerInput _playerInput;
        private GameObject _mainCamera;
        private StarterAssetsInputs _input;
        private CharacterController _controller;
        private float _speed;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;


        private void Awake()
        {
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        void Start()
        {
            _input = GetComponent<StarterAssetsInputs>();
            _controller = GetComponent<CharacterController>();
        }

        void Update()
        {
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
    }
}