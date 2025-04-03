using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 문의 기본 데이터를 정의하는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "DoorData", menuName = "Ship Data/Door Data")]
public class DoorData : ScriptableObject
{
    [Serializable]
    public class DoorLevel
    {
        /// <summary>문 이름</summary>
        public string doorName;

        /// <summary>문 레벨</summary>
        public int level;

        /// <summary>문 통과 지연 시간(초)</summary>
        public float passThroughDelay;

        /// <summary>문의 가격</summary>
        public int cost;

        /// <summary>문 작동에 필요한 전력</summary>
        public float powerRequirement;

        /// <summary>문 스프라이트</summary>
        public Sprite doorSprite;

        /// <summary>문 열림 사운드</summary>
        public AudioClip openSound;

        /// <summary>문 닫힘 사운드</summary>
        public AudioClip closeSound;
    }

    /// <summary>문의 레벨별 데이터</summary>
    [SerializeField] private List<DoorLevel> doorLevels = new List<DoorLevel>();

    /// <summary>
    /// ScriptableObject가 활성화될 때 호출되며, 필요한 경우 기본 데이터를 초기화합니다.
    /// </summary>
    private void OnEnable()
    {
        if (doorLevels == null || doorLevels.Count == 0)
        {
            InitializeDefaultLevels();
        }
    }

    /// <summary>
    /// 기본 문 레벨 데이터 초기화
    /// </summary>
    private void InitializeDefaultLevels()
    {
        doorLevels = new List<DoorLevel>
        {
            new DoorLevel
            {
                doorName = "door.level1",
                level = 1,
                passThroughDelay = 0.2f,
                cost = 10,
                powerRequirement = 0f, // 전력 소모 없음
            },
            new DoorLevel
            {
                doorName = "door.level2",
                level = 2,
                passThroughDelay = 0.0f,
                cost = 30,
                powerRequirement = 1f, // 1kW 전력 소모
            }
        };
    }

    /// <summary>
    /// 지정된 레벨의 문 데이터 반환
    /// </summary>
    public DoorLevel GetDoorData(int level)
    {
        if (level <= 0 || level > doorLevels.Count)
            return null;

        return doorLevels[level - 1];
    }
}
