using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; //для подключения навмеша 
using Game.Utils; //для доступа к функции случайного выбора направления
using UnityEngine.InputSystem.XR.Haptics;
using UnityEditor.ShaderKeywordFilter;
using Unity.Burst.Intrinsics;
public class Enemy_movement : MonoBehaviour
{
    //Задаём движение врага через машинное состояние и использование NavMesh
    //private NavMeshAgent NAV_meshAgent;
    private Rigidbody2D rb;
    public event EventHandler OnEnemyAttack; //событие для атаки
    [SerializeField] private State _startingState; //начальное состояние врага
    [SerializeField] private float _roamingdistanceMax = 7f; //максимальное состояние брожения
    [SerializeField] private float _roamingdistanceMin = 3f; //минимальное состояние брожения
    [SerializeField] private float _roamingTimeMax = 2f; //максимальное время брожения
    [SerializeField] private bool _isChacingEnemy = false; //способность врага преследовать
    //флаг, говорящий о том, является ли враг преследующим
    //[SerializeField] private bool _isRunningEnemy = false;
    [SerializeField] private bool _isAttackingEnemy = false; //способность врага атаковать
    //флаг, говорящий о том, является ли враг преследующим
    private enum State
    {
        Idle,
        Roaming,
        Chacing,
        Attacking,
        Death
    }//список состояний

    [SerializeField] private float _attackingDistance = 2f; //расстояние атаки
    [SerializeField] private float _chacingDistance = 4f; //расстояние преследования
    [SerializeField] private float _chacingSpeedMultiplaier = 2f; //ускорение при преследовании

    private NavMeshAgent _navMeshAgent;
    private State _state; //текущее состояние врага
    private float _roamingTime; //время брожения
    private Vector2 _roamPosition; //конечная точка преследования
    private Vector2 _startingPosition; //текущее местоположение

    private float _MAINSpeed;
    private float _roamingSpeed; //скорость брожения(начальную скорость)
    private float _chacingSpeed; //скорость преследования

    [SerializeField] private float _attackRate = 2f; //периодичность атаки
    private float _nextAttackTime = 0f; //время следующей атаки
    public bool IsRunning
    //метод (свойство), который отслеживает бежит враг или нет
    {
        get
        {
            Vector2 V = rb.velocity;
            if ((V == Vector2.zero))
            {
                return false; //нет движения
            }
            else
            {
                return true; //есть движение
            }
        }
    }
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        //_navMeshAgent = GetComponent<NavMeshAgent>();
        _state = _startingState;
        /*
        _navMeshAgent.updateRotation = false; //чтобы не было вращения
        _navMeshAgent.updateUpAxis = false; //чтобы ориентация меша не влияла на ориентацию основного объекта по оси Y
        */
        //увеличение скорости при преследовании
        //_roamingSpeed = _navMeshAgent.speed; //опредляем скорость брожения (начальную скорость)
        _roamingSpeed = _MAINSpeed;
        //_chacingSpeed = _navMeshAgent.speed * _chacingSpeedMultiplaier; //определяем скорость преследования
        _chacingSpeed = _MAINSpeed * _chacingSpeedMultiplaier;
    }
    private void Update()
    {
        StateHandler();
        MovingDirectionHandle();
        //функция переходов к способам движения
    }
    private void StateHandler()
    {
        switch (_state)
        {
            default:
            case State.Roaming:
                _roamingTime -= Time.deltaTime; //уменьшаем время для каждого цикла брожения
                if (_roamingTime < 0) //если время брожения становится равным нулю
                {
                    Roaming(); //ищем новую точку для движения в новом направлении
                    _roamingTime = _roamingTimeMax; //обновляем время брожения
                }
                CheckCurrentState(); //проверка состояния
                break;
            case State.Chacing:
                ChacingTarget(); //логика преследования
                CheckCurrentState(); //проверка состояния
                break;
            case State.Attacking:
                AttackingTarget(); //логика атаки
                CheckCurrentState(); //проверка состояния
                break;
            case State.Death:
                break;
            case State.Idle:
                break;
        }
    }
    private void AttackingTarget()
    {
        if (Time.time > _nextAttackTime) //если текущее время больше времени атаки
        {
            OnEnemyAttack?.Invoke(this, EventArgs.Empty);
            //вызываем события атаки
            _nextAttackTime = Time.time + _attackRate;
            //устанавливаем новое время атаки
        }
    }
    /*
    public float GetRoamingAnimationSpeed()
    {
        return _navMeshAgent.speed / _roamingSpeed;
        //значение параметра для аниматора
    }
    */
    private void ChacingTarget()
    {
        rb.MovePosition(Player.Instance.transform.position);
        //_navMeshAgent.SetDestination(Player.Instance.transform.position);
        //задаём точку для движения врага как положение героя
    }
    private void CheckCurrentState() //функция для проверки состояния
    {
        float distance_to_player = Vector2.Distance(transform.position, Player.Instance.transform.position);
        State new_state = State.Roaming;
        if (_isChacingEnemy)
        {
            if (distance_to_player <= _chacingDistance)
            {
                new_state = State.Chacing;
            }
            //проверяем насколько врга приближён к состоянию преследования героя
        }
        if (_isAttackingEnemy)
        {
            if (distance_to_player <= _attackingDistance)
            {
                new_state = State.Attacking;
            }
            //проверяем насколько врга приближён к состоянию атаки героя
        }
        if (new_state != _state) //если состояние врага сменилось
        {
            if (new_state == State.Chacing) //на преследование
            {
                //_navMeshAgent.ResetPath(); //сброс точки дальнейшего движения
                //_navMeshAgent.speed = _chacingSpeed; //устанавливаем скорость преследования
                //код для сброса предыдущей точки движения
                _MAINSpeed = _chacingSpeed;
            }
            else if (new_state == State.Roaming) //на брожение
            {
                _roamingTime = 0f; //включить счётчик времени брожения
                //_navMeshAgent.speed = _roamingSpeed; //устанавливаем скорость брожения
                _MAINSpeed = _roamingSpeed;
            }
            else if (new_state == State.Attacking) //на атаку
            {
                //_navMeshAgent.ResetPath(); //сброс точки дальнейшего движения
                //код для сброса предыдущей точки движения
            }
            _state = new_state; //делаем нвоое состояние текущим
        }
    }
    private void Roaming()
    {
        _startingPosition = transform.position;
        //обновляем каждый раз текущуюю позицию
        _roamPosition = GetRoamingPosition();
        //случайным образом задаём позицию, к которой враг будет двигаться
        //ChangeFacingDirection(_startingPosition, _roamPosition);
        //разворачиваем врага, чтобы он писной не ходил
        rb.MovePosition(_roamPosition);
        //_navMeshAgent.SetDestination(_roamPosition);
        //при помощи компонента NavMesh задаём следующую конечную точку для движения
    }
    private Vector2 GetRoamingPosition()
    {
        return _startingPosition + Common1.GetRandomDir() * UnityEngine.Random.Range(_roamingdistanceMin, _roamingdistanceMax);
        //функция для опредления следующей конечной точки
        //Common.GetRandomDir() - функция для определения направления движения
    }
    private void ChangeFacingDirection(Vector2 firstPosition, Vector2 targetPosition)
    {
        //функция для разворота врага в сторону точки движения
        if (firstPosition.x > targetPosition.x)
        {
            transform.rotation = Quaternion.Euler(0, -180, 0);
            //разворот по оси Oy на 180
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            //нет разворота вообще
        }
    }
    private float _checkDirectionTime = 0f;
    private float _checkDirectionDeltaTime = 0.1f;
    private Vector2 _lastPosition;
    private void MovingDirectionHandle()
    {
        if (Time.time > _checkDirectionTime)
        {
            if (IsRunning)
            {
                ChangeFacingDirection(_lastPosition, transform.position);
            }
            else if (_state == State.Attacking)
            {
                ChangeFacingDirection(_lastPosition, Player.Instance.transform.position);
            }
            _lastPosition = transform.position;
            _checkDirectionTime = Time.time * _checkDirectionDeltaTime;
        }
    }
}