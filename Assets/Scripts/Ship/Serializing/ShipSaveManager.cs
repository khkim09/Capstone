// using System;
// using System.Collections.Generic;
// using System.IO;
// using UnityEngine;
// using UnityEngine.Events;
//
// /// <summary>
// /// 게임 내에서 함선 저장 및 불러오기를 관리하는 클래스
// /// </summary>
// public class ShipSaveManager : MonoBehaviour
// {
//     /// <summary>
//     /// 싱글톤 인스턴스
//     /// </summary>
//     public static ShipSaveManager Instance { get; private set; }
//
//     /// <summary>
//     /// 함선이 저장되었을 때 발생하는 이벤트
//     /// </summary>
//     public UnityEvent OnShipSaved = new();
//
//     /// <summary>
//     /// 함선이 로드되었을 때 발생하는 이벤트
//     /// </summary>
//     public UnityEvent OnShipLoaded = new();
//
//     /// <summary>
//     /// 저장 슬롯별 명칭
//     /// </summary>
//     [SerializeField] private string[] saveSlotNames = { "Auto Save", "Save Slot 1", "Save Slot 2", "Save Slot 3" };
//
//     /// <summary>
//     /// 저장 폴더 경로
//     /// </summary>
//     private string saveFolderPath;
//
//     /// <summary>
//     /// 자동 저장 간격 (초)
//     /// </summary>
//     [SerializeField] private float autoSaveInterval = 300f; // 5분
//
//     /// <summary>
//     /// 자동 저장 타이머
//     /// </summary>
//     private float autoSaveTimer;
//
//     /// <summary>
//     /// 자동 저장 활성화 여부
//     /// </summary>
//     [SerializeField] private bool autoSaveEnabled = true;
//
//     /// <summary>
//     /// 저장 메타데이터
//     /// </summary>
//     [Serializable]
//     public class SaveMetadata
//     {
//         public string saveName;
//         public string saveTime;
//         public string playerName;
//         public float playTime;
//         public string lastVisitedPlanet;
//         public int crewCount;
//         public int coma; // 소지금
//     }
//
//     private void Awake()
//     {
//         // 싱글톤 패턴 적용
//         if (Instance == null)
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject);
//
//             // 저장 경로 설정
//             saveFolderPath = Path.Combine(Application.persistentDataPath, "ShipSaves");
//             Directory.CreateDirectory(saveFolderPath);
//
//             autoSaveTimer = autoSaveInterval;
//         }
//         else
//         {
//             Destroy(gameObject);
//         }
//     }
//
//     private void Update()
//     {
//         // 자동 저장 처리
//         if (autoSaveEnabled)
//         {
//             autoSaveTimer -= Time.deltaTime;
//             if (autoSaveTimer <= 0)
//             {
//                 AutoSave();
//                 autoSaveTimer = autoSaveInterval;
//             }
//         }
//     }
//
//     /// <summary>
//     /// 자동 저장 실행
//     /// </summary>
//     private void AutoSave()
//     {
//         Ship playerShip = GameManager.Instance.GetPlayerShip();
//         if (playerShip != null)
//         {
//             SaveShip(playerShip, 0); // 0번 슬롯은 자동 저장용
//             Debug.Log("Auto-save completed");
//         }
//     }
//
//     /// <summary>
//     /// 함선을 지정된 슬롯에 저장
//     /// </summary>
//     /// <param name="ship">저장할 함선</param>
//     /// <param name="slotIndex">저장 슬롯 (0: 자동 저장)</param>
//     /// <returns>성공 여부</returns>
//     public bool SaveShip(Ship ship, int slotIndex)
//     {
//         if (ship == null || slotIndex < 0 || slotIndex >= saveSlotNames.Length)
//             return false;
//
//         // 저장 경로 생성
//         string saveFileName = $"ship_slot_{slotIndex}.json";
//         string metaFileName = $"ship_slot_{slotIndex}_meta.json";
//         string savePath = Path.Combine(saveFolderPath, saveFileName);
//         string metaPath = Path.Combine(saveFolderPath, metaFileName);
//
//         try
//         {
//             // 함선 JSON 저장
//             string shipJson = ship.ToJson();
//             File.WriteAllText(savePath, shipJson);
//
//             // 메타데이터 생성 및 저장
//             SaveMetadata metadata = CreateSaveMetadata(ship, slotIndex);
//             string metaJson = JsonUtility.ToJson(metadata, true);
//             File.WriteAllText(metaPath, metaJson);
//
//             // 이벤트 발생
//             OnShipSaved.Invoke();
//
//             return true;
//         }
//         catch (Exception e)
//         {
//             Debug.LogError($"함선 저장 오류 (슬롯 {slotIndex}): {e.Message}");
//             return false;
//         }
//     }
//
//     /// <summary>
//     /// 저장 메타데이터 생성
//     /// </summary>
//     private SaveMetadata CreateSaveMetadata(Ship ship, int slotIndex)
//     {
//         SaveMetadata metadata = new()
//         {
//             saveName = saveSlotNames[slotIndex],
//             saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
//             playerName = "Captain", // GameManager에서 실제 플레이어 이름 가져오기
//             playTime = Time.timeSinceLevelLoad, // 또는 GameManager에서 누적 플레이 시간 가져오기
//             lastVisitedPlanet = "Earth", // 실제 방문한 행성 이름 가져오기
//             crewCount = ship.GetCrewCount(),
//             coma = 0 // ResourceManager에서 실제 Coma 값 가져오기
//         };
//
//         // ResourceManager에서 Coma 값 가져오기 (있다면)
//         if (ResourceManager.Instance != null)
//             metadata.coma = (int)ResourceManager.Instance.GetResource(ResourceType.COMA);
//
//         return metadata;
//     }
//
//     /// <summary>
//     /// 지정된 슬롯에서 함선 불러오기
//     /// </summary>
//     /// <param name="ship">대상 함선 객체</param>
//     /// <param name="slotIndex">저장 슬롯</param>
//     /// <returns>성공 여부</returns>
//     public bool LoadShip(Ship ship, int slotIndex)
//     {
//         if (ship == null || slotIndex < 0 || slotIndex >= saveSlotNames.Length)
//             return false;
//
//         string saveFileName = $"ship_slot_{slotIndex}.json";
//         string savePath = Path.Combine(saveFolderPath, saveFileName);
//
//         if (!File.Exists(savePath))
//         {
//             Debug.LogWarning($"저장 파일이 존재하지 않음: {savePath}");
//             return false;
//         }
//
//         try
//         {
//             bool success = ship.LoadFromFile(savePath);
//
//             if (success)
//             {
//                 // 이벤트 발생
//                 OnShipLoaded.Invoke();
//                 return true;
//             }
//
//             return false;
//         }
//         catch (Exception e)
//         {
//             Debug.LogError($"함선 불러오기 오류 (슬롯 {slotIndex}): {e.Message}");
//             return false;
//         }
//     }
//
//     /// <summary>
//     /// 모든 저장 슬롯의 메타데이터 가져오기
//     /// </summary>
//     /// <returns>슬롯별 메타데이터 목록 (없는 슬롯은 null)</returns>
//     public SaveMetadata[] GetAllSaveMetadata()
//     {
//         SaveMetadata[] results = new SaveMetadata[saveSlotNames.Length];
//
//         for (int i = 0; i < saveSlotNames.Length; i++)
//         {
//             string metaFileName = $"ship_slot_{i}_meta.json";
//             string metaPath = Path.Combine(saveFolderPath, metaFileName);
//
//             if (File.Exists(metaPath))
//                 try
//                 {
//                     string json = File.ReadAllText(metaPath);
//                     results[i] = JsonUtility.FromJson<SaveMetadata>(json);
//                 }
//                 catch (Exception e)
//                 {
//                     Debug.LogError($"메타데이터 로드 오류 (슬롯 {i}): {e.Message}");
//                     results[i] = null;
//                 }
//             else
//                 results[i] = null;
//         }
//
//         return results;
//     }
//
//     /// <summary>
//     /// 특정 저장 슬롯의 메타데이터 가져오기
//     /// </summary>
//     /// <param name="slotIndex">슬롯 인덱스</param>
//     /// <returns>메타데이터 또는 null</returns>
//     public SaveMetadata GetSaveMetadata(int slotIndex)
//     {
//         if (slotIndex < 0 || slotIndex >= saveSlotNames.Length)
//             return null;
//
//         string metaFileName = $"ship_slot_{slotIndex}_meta.json";
//         string metaPath = Path.Combine(saveFolderPath, metaFileName);
//
//         if (File.Exists(metaPath))
//             try
//             {
//                 string json = File.ReadAllText(metaPath);
//                 return JsonUtility.FromJson<SaveMetadata>(json);
//             }
//             catch (Exception e)
//             {
//                 Debug.LogError($"메타데이터 로드 오류 (슬롯 {slotIndex}): {e.Message}");
//                 return null;
//             }
//
//         return null;
//     }
//
//     /// <summary>
//     /// 저장 슬롯 삭제
//     /// </summary>
//     /// <param name="slotIndex">삭제할 슬롯 인덱스</param>
//     /// <returns>성공 여부</returns>
//     public bool DeleteSaveSlot(int slotIndex)
//     {
//         if (slotIndex < 0 || slotIndex >= saveSlotNames.Length)
//             return false;
//
//         string saveFileName = $"ship_slot_{slotIndex}.json";
//         string metaFileName = $"ship_slot_{slotIndex}_meta.json";
//         string savePath = Path.Combine(saveFolderPath, saveFileName);
//         string metaPath = Path.Combine(saveFolderPath, metaFileName);
//
//         bool success = true;
//
//         if (File.Exists(savePath))
//             try
//             {
//                 File.Delete(savePath);
//             }
//             catch (Exception e)
//             {
//                 Debug.LogError($"저장 파일 삭제 오류 (슬롯 {slotIndex}): {e.Message}");
//                 success = false;
//             }
//
//         if (File.Exists(metaPath))
//             try
//             {
//                 File.Delete(metaPath);
//             }
//             catch (Exception e)
//             {
//                 Debug.LogError($"메타데이터 파일 삭제 오류 (슬롯 {slotIndex}): {e.Message}");
//                 success = false;
//             }
//
//         return success;
//     }
//
//     /// <summary>
//     /// 자동 저장 활성화/비활성화 설정
//     /// </summary>
//     /// <param name="enabled">활성화 여부</param>
//     public void SetAutoSaveEnabled(bool enabled)
//     {
//         autoSaveEnabled = enabled;
//
//         // 활성화되면 타이머 리셋
//         if (enabled) autoSaveTimer = autoSaveInterval;
//     }
//
//     /// <summary>
//     /// 자동 저장 간격 설정
//     /// </summary>
//     /// <param name="intervalInSeconds">간격 (초)</param>
//     public void SetAutoSaveInterval(float intervalInSeconds)
//     {
//         autoSaveInterval = Mathf.Max(60f, intervalInSeconds); // 최소 1분
//         autoSaveTimer = autoSaveInterval; // 타이머 리셋
//     }
// }


