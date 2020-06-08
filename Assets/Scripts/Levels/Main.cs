﻿using System;
using Cinemachine;
using UnityEngine;
using Cubra.Helpers;
using Cubra.Controllers;

namespace Cubra
{
    public class Main : MonoBehaviour
    {
        public static Main Instance;

        // Перечисление режимов игры
        public enum GameModes { Start, Training, Play, Completed, Losing }

        // Текущий игровой режим
        public GameModes CurrentMode { get; set; }

        [Header("Номер уровня")]
        [SerializeField] private int _levelNumber;

        public int LevelNumber => _levelNumber;

        // Событие по запуску уровня
        public event Action LevelLaunched;

        // Ссылка на компонент обучения
        private Training _training;

        [Header("Префабы персонажей")]
        [SerializeField] private GameObject[] characters;

        [Header("Стартовая позиция персонажа")]
        [SerializeField] private Vector3 position;

        // Ссылка на контроллер персонажа
        public Controllers.CharacterController CharacterController { get; private set; }

        // Ссылка на персонажа
        private Character _character;

        // Объект для работы со статистикой по персонажам
        public ZombieHelper ZombieHelper { get; set; }

        [Header("Количество мозгов")]
        [SerializeField] private int _brains;

        public int Brains { get => _brains; set => _brains = value; }

        // Количество собранных монет
        public int Coins { get; set; }

        // Текущее количество звезд за уровень
        public int Stars { get; set; } = 3;

        [Header("Виртуальная камера")]
        [SerializeField] private CinemachineVirtualCamera _cinemachineVirtual;

        [Header("Таймер уровня")]
        [SerializeField] private Timer _timer;

        public Timer Timer => _timer;

        // Ссылка на компонент результатов
        public LevelResults LevelResults { get; private set; }

        // Ссылка на компонент фоновой музыки
        private BackgroundMusic _backgroundMusic;

        private void Awake()
        {
            Instance = this;

            // Получаем номер активного персонажа
            var activeCharacter = PlayerPrefs.GetInt("character");
            // Создаем активного персонажа в стартовой позиции и получаем его компонент
            _character = Instantiate(characters[activeCharacter - 1], position, Quaternion.identity).GetComponent<Character>();

            // Настраиваем камеру
            SnapCameraToTarget();

            // Преобразовваем сохраненную json строку в объект
            ZombieHelper = JsonUtility.FromJson<ZombieHelper>(PlayerPrefs.GetString("character-" + PlayerPrefs.GetInt("character")));

            // Получаем контроллер персонажа
            CharacterController = gameObject.GetComponent<Controllers.CharacterController>();

            // Получаем компоненты
            _training = gameObject.GetComponent<Training>();
            LevelResults = gameObject.GetComponent<LevelResults>();
            _backgroundMusic = FindObjectOfType<BackgroundMusic>();
        }

        private void Start()
        {
            // Если на уровне присутствует обучение
            if (_training & Training.PlayerTraining)
            {
                // Переключаем режим на обучение
                CurrentMode = GameModes.Training;

                // Отключаем управление персонажем
                CharacterController.Disable();

                // Запускаем обучение
                _ = StartCoroutine(_training.StartTraining());
            }
            else
            {
                LaunchALevel();
            }
        }

        /// <summary>
        /// Запуск уровня
        /// </summary>
        public void LaunchALevel()
        {
            CurrentMode = GameModes.Play;
            // Сообщаем о запуске уровня
            LevelLaunched?.Invoke();

            // Если звуки не отключены
            if (SoundController.PlayingSounds)
            {
                // Устанавливаем фоновую музыку и запускаем
                _backgroundMusic.SetBackgroundMusic();
                _backgroundMusic.SwitchMusic((int)BackgroundMusic.State.On);
            }    
        }

        /// <summary>
        /// Привязка/отвязка камеры от персонажа
        /// </summary>
        public void SnapCameraToTarget()
        {
            // Если персонаж живой
            if (_character.Life)
            {
                // Привязываем камеру к персонажу
                _cinemachineVirtual.Follow = _character.Transform;
            }
            else
            {
                _cinemachineVirtual.Follow = null;
            }
        }
    }
}