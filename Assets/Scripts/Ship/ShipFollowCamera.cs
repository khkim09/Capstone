using System;
using System.Collections.Generic;
using UnityEngine;

// 디버깅용으로 에디터에서도 실행되도록 설정
[ExecuteInEditMode]
public class ShipFollowCamera : MonoBehaviour
{
    [SerializeField] private Ship targetShip;
    [SerializeField] private Camera camera;
    [SerializeField] private float targetShipPositionX = 0.3f; // 함선 중심이 위치할 화면 X 좌표 (0.3 = 30%)
    [SerializeField] private float targetShipPositionY = 0.5f; // 함선 중심이 위치할 화면 Y 좌표 (0.5 = 50%)
    [SerializeField] private float leftBoundaryPercent = 0.0f; // 함선이 표시될 영역의 왼쪽 경계 (0%)
    [SerializeField] private float rightBoundaryPercent = 0.6f; // 함선이 표시될 영역의 오른쪽 경계 (60%)
    [SerializeField] private float paddingPercent = 0.05f; // 함선 주변 여백 비율 (5%)
    [SerializeField] private float safetyMarginPercent = 0.95f; // 계산시 사용할 안전 마진 (95%만 사용)
    [SerializeField] private float minOrthographicSize = 3f; // 최소 카메라 크기
    [SerializeField] private float maxOrthographicSize = 30f; // 최대 카메라 크기
    [SerializeField] private bool debugMode = false; // 디버그 모드 활성화
    [SerializeField] private bool updateEveryFrame = false; // 매 프레임마다 업데이트 (디버깅용)

    private Vector3 shipCenter; // 함선 중심 위치
    private float shipWidth, shipHeight; // 함선 크기
    private bool isWidthConstraining; // 가로가 제한 요소인지 여부

    private void Awake()
    {
        if (!Application.isPlaying) return;
        GameManager.Instance.OnShipInitialized += GetCameraStartPositionToOriginShip;
    }

    private void Update()
    {
        if (updateEveryFrame && targetShip != null && Application.isPlaying) GetCameraStartPositionToOriginShip();
    }

    private void DebugLog(string message)
    {
        if (debugMode)
            Debug.Log($"[카메라] {message}");
    }

    public void GetCameraStartPositionToOriginShip()
    {
        if (targetShip == null) return;

        List<Room> allRooms = targetShip.GetAllRooms();
        List<ShipWeapon> allWeapons = targetShip.GetAllWeapons();

        // 방이나 무기가 없으면 기본 그리드 중앙으로
        if ((allRooms == null || allRooms.Count == 0) && (allWeapons == null || allWeapons.Count == 0))
        {
            Vector3 gridCenter = targetShip.GetWorldPositionFromGrid(targetShip.GetGridSize() / 2);
            PositionCamera(gridCenter, camera.orthographicSize);
            return;
        }

        // 모든 타일 위치 수집
        List<Vector2Int> allTiles = new();

        if (allRooms != null)
            foreach (Room room in allRooms)
                if (room != null)
                {
                    List<Vector2Int> tiles = room.GetOccupiedTiles();
                    if (tiles != null)
                        allTiles.AddRange(tiles);
                }

        if (allWeapons != null)
            foreach (ShipWeapon weapon in allWeapons)
                if (weapon != null)
                {
                    Vector2Int pos = weapon.GetGridPosition();
                    allTiles.Add(pos);
                    allTiles.Add(new Vector2Int(pos.x + 1, pos.y));
                }

        if (allTiles.Count == 0)
        {
            DebugLog("경고: 함선에 유효한 타일이 없습니다!");
            return;
        }

        // 함선의 경계 계산 (최소/최대 좌표)
        Vector2Int minTile = new(int.MaxValue, int.MaxValue);
        Vector2Int maxTile = new(int.MinValue, int.MinValue);

        foreach (Vector2Int tile in allTiles)
        {
            minTile.x = Mathf.Min(minTile.x, tile.x);
            minTile.y = Mathf.Min(minTile.y, tile.y);
            maxTile.x = Mathf.Max(maxTile.x, tile.x);
            maxTile.y = Mathf.Max(maxTile.y, tile.y);
        }

        // 함선 중심점 계산 - 경계의 중점
        // (GetWorldPositionFromGrid 좌측 하단 좌표를 반환하므로 정확히 계산)
        Vector3 minWorldPos = targetShip.GetWorldPositionFromGrid(minTile);
        Vector3 maxWorldPos = targetShip.GetWorldPositionFromGrid(maxTile + Vector2Int.one); // 우측 상단 경계

        shipCenter = new Vector3(
            (minWorldPos.x + maxWorldPos.x) * 0.5f,
            (minWorldPos.y + maxWorldPos.y) * 0.5f,
            0f
        );

        // 함선 크기 계산 (최대 타일 - 최소 타일 + 1 타일) * 타일 크기
        shipWidth = (maxTile.x - minTile.x + 1) * Constants.Grids.CellSize;
        shipHeight = (maxTile.y - minTile.y + 1) * Constants.Grids.CellSize;

        DebugLog($"타일 범위 - X: {minTile.x}~{maxTile.x}, Y: {minTile.y}~{maxTile.y}");
        DebugLog($"월드 좌표 - 최소: {minWorldPos}, 최대: {maxWorldPos}");
        DebugLog($"함선 정보 - 너비: {shipWidth}, 높이: {shipHeight}, 중심: {shipCenter}");

        // 함선의 양 끝점 계산
        Vector3 shipLeftEdge = new(shipCenter.x - shipWidth / 2, shipCenter.y, shipCenter.z);
        Vector3 shipRightEdge = new(shipCenter.x + shipWidth / 2, shipCenter.y, shipCenter.z);
        Vector3 shipTopEdge = new(shipCenter.x, shipCenter.y + shipHeight / 2, shipCenter.z);
        Vector3 shipBottomEdge = new(shipCenter.x, shipCenter.y - shipHeight / 2, shipCenter.z);

        DebugLog($"함선 경계 - 좌측: {shipLeftEdge.x}, 우측: {shipRightEdge.x}, 상단: {shipTopEdge.y}, 하단: {shipBottomEdge.y}");

        // 함선 크기와 원하는 화면 위치를 고려하여 카메라 위치와 크기 조정
        CalculateCameraSettings();
    }

    private void CalculateCameraSettings()
    {
        // 디버깅 도구로써 원래 값 저장
        float originalOrthoSize = camera.orthographicSize;

        // 패딩이 적용된 함선 크기 계산
        float paddedShipWidth = shipWidth * (1 + paddingPercent * 2);
        float paddedShipHeight = shipHeight * (1 + paddingPercent * 2);

        DebugLog($"패딩 적용된 함선 크기 - 너비: {paddedShipWidth}, 높이: {paddedShipHeight}");

        // 화면 비율 계산
        float screenAspect = camera.aspect;
        DebugLog($"화면 비율(width/height): {screenAspect}");

        // 함선의 가로세로 비율
        float shipAspect = paddedShipWidth / paddedShipHeight;
        DebugLog($"함선 비율(width/height): {shipAspect}");

        // 1. 사용 가능한 화면 영역 계산
        float availableWidthRatio = rightBoundaryPercent - leftBoundaryPercent;

        DebugLog($"사용 가능한 화면 영역 - 가로: {availableWidthRatio * 100}%");

        // 2. 가로/세로 방향 각각에 대해 필요한 orthoSize 계산
        float orthoSizeForWidth = CalculateOrthoSizeForWidth(paddedShipWidth, availableWidthRatio);
        float orthoSizeForHeight = CalculateOrthoSizeForHeight(paddedShipHeight);

        DebugLog($"필요 orthoSize 비교 - 가로: {orthoSizeForWidth}, 세로: {orthoSizeForHeight}");

        // 3. 어느 쪽이 제한 요소인지 결정 (더 큰 값이 제한 요소)
        isWidthConstraining = orthoSizeForWidth >= orthoSizeForHeight;
        string constrainingFactor = isWidthConstraining ? "가로" : "세로";
        DebugLog($"제한 요소: {constrainingFactor}");

        // 4. 더 제한적인 쪽(더 큰 값)을 선택하여 함선이 영역 내에 완전히 표시되도록 함
        float requiredOrthoSize = Mathf.Max(orthoSizeForWidth, orthoSizeForHeight);

        // 5. 최종 값 제한 적용
        float clampedOrthoSize = Mathf.Clamp(requiredOrthoSize, minOrthographicSize, maxOrthographicSize);

        // 제한으로 인해 값이 변경되었는지 확인
        if (clampedOrthoSize != requiredOrthoSize)
            DebugLog($"제한으로 인해 orthoSize가 {requiredOrthoSize}에서 {clampedOrthoSize}로 변경됨");

        // 계산 전후 값 비교
        DebugLog($"카메라 크기 변경: {originalOrthoSize} -> {clampedOrthoSize}");

        // 6. 카메라 크기 설정 및 위치 조정
        camera.orthographicSize = clampedOrthoSize;
        PositionCamera(shipCenter, clampedOrthoSize);

        // 7. 계산 후 함선 위치 시각화 (디버깅용)
        VisualizeShipPositionOnScreen(clampedOrthoSize);
    }

    private float CalculateOrthoSizeForWidth(float shipWidth, float availableWidthRatio)
    {
        // 안전 마진 적용 (가용 영역의 일부만 사용)
        float effectiveWidthRatio = availableWidthRatio * safetyMarginPercent;

        // 화면의 가용 영역에 함선을 맞추기 위한 최소 화면 너비 계산
        float requiredScreenWidth = shipWidth / effectiveWidthRatio;

        // 필요한 orthographicSize 계산 (화면 너비 = 2 * orthoSize * aspect)
        float orthoSize = requiredScreenWidth / (2 * camera.aspect);

        DebugLog(
            $"가로 계산 - 함선 너비: {shipWidth}, 유효 가용 비율: {effectiveWidthRatio}, 필요너비: {requiredScreenWidth}, orthoSize: {orthoSize}");

        return orthoSize;
    }

    private float CalculateOrthoSizeForHeight(float shipHeight)
    {
        // 안전 마진 적용 (화면 높이의 일부만 사용)
        float effectiveHeightRatio = safetyMarginPercent;

        // 화면의 가용 영역에 함선을 맞추기 위한 최소 화면 높이 계산
        float requiredScreenHeight = shipHeight / effectiveHeightRatio;

        // 필요한 orthographicSize 계산 (화면 높이 = 2 * orthoSize)
        float orthoSize = requiredScreenHeight / 2;

        DebugLog(
            $"세로 계산 - 함선 높이: {shipHeight}, 유효 가용 비율: {effectiveHeightRatio}, 필요높이: {requiredScreenHeight}, orthoSize: {orthoSize}");

        return orthoSize;
    }

    private void PositionCamera(Vector3 targetPosition, float orthoSize)
    {
        // 화면 크기 계산
        float cameraHeight = 2f * orthoSize;
        float cameraWidth = cameraHeight * camera.aspect;

        float xOffset;

        if (isWidthConstraining)
        {
            // 가로가 제한 요소일 때: 특별한 처리 필요
            // 함선이 rightBoundaryPercent (0.6) 내에 맞게 표시되도록 설정됨
            // 우리는 함선 중심이 targetShipPositionX (0.3) 위치에 오도록 해야 함

            // 1. 사용 가능한 가로 영역 계산 (안전 마진 적용)
            float availableWidth = (rightBoundaryPercent - leftBoundaryPercent) * safetyMarginPercent;

            // 2. 함선 중심이 30% 위치에 오도록 카메라 위치 계산
            // 카메라 중심이 화면의 50% 위치에 있으므로,
            // 카메라를 오른쪽으로 (50% - 30%) = 20% 이동해야 함
            xOffset = cameraWidth * (0.5f - targetShipPositionX);
        }
        else
        {
            // 세로가 제한 요소일 때: 일반적인 처리
            // 함선 중심이 targetShipPositionX (0.3) 위치에 오도록 간단히 계산
            xOffset = cameraWidth * (0.5f - targetShipPositionX);
        }

        // 세로 방향은 항상 중앙에 위치하도록 함
        float yOffset = cameraHeight * (0.5f - targetShipPositionY);

        // 계산된 오프셋을 적용하여 카메라 위치 조정
        Vector3 newCameraPosition = new(
            targetPosition.x + xOffset, // + 오프셋으로 카메라가 오른쪽으로 이동 → 함선이 화면 왼쪽으로 이동
            targetPosition.y + yOffset, // + 오프셋으로 카메라가 위로 이동 → 함선이 화면 아래로 이동
            camera.transform.position.z
        );

        camera.transform.position = newCameraPosition;

        DebugLog(
            $"카메라 위치 조정 - 화면크기: ({cameraWidth}, {cameraHeight}), 제한 요소: {(isWidthConstraining ? "가로" : "세로")}, " +
            $"목표 함선 위치: ({targetShipPositionX:F2}, {targetShipPositionY:F2}), 오프셋: ({xOffset:F2}, {yOffset:F2}), 최종 위치: {newCameraPosition}");
    }

    private void VisualizeShipPositionOnScreen(float orthoSize)
    {
        // 디버깅용: 함선의 위치가 화면에서 어디에 표시되는지 확인
        float cameraHeight = 2f * orthoSize;
        float cameraWidth = cameraHeight * camera.aspect;

        // 카메라 기준 월드 좌표 계산
        float camLeft = camera.transform.position.x - cameraWidth * 0.5f;
        float camRight = camera.transform.position.x + cameraWidth * 0.5f;
        float camTop = camera.transform.position.y + cameraHeight * 0.5f;
        float camBottom = camera.transform.position.y - cameraHeight * 0.5f;

        // 함선 경계 위치
        float shipLeft = shipCenter.x - shipWidth * 0.5f;
        float shipRight = shipCenter.x + shipWidth * 0.5f;
        float shipTop = shipCenter.y + shipHeight * 0.5f;
        float shipBottom = shipCenter.y - shipHeight * 0.5f;

        // 화면 내 함선 위치 (비율)
        float shipLeftScreenPos = (shipLeft - camLeft) / cameraWidth;
        float shipRightScreenPos = (shipRight - camLeft) / cameraWidth;
        float shipTopScreenPos = (shipTop - camBottom) / cameraHeight;
        float shipBottomScreenPos = (shipBottom - camBottom) / cameraHeight;

        // 함선 중심 위치 계산
        float shipCenterHorizontalPos = (shipCenter.x - camLeft) / cameraWidth;
        float shipCenterVerticalPos = (shipCenter.y - camBottom) / cameraHeight;

        DebugLog(
            $"화면 내 함선 위치(비율) - 좌: {shipLeftScreenPos:F3}, 우: {shipRightScreenPos:F3}, 상: {shipTopScreenPos:F3}, 하: {shipBottomScreenPos:F3}");
        DebugLog($"화면 내 함선 중심(비율) - 가로: {shipCenterHorizontalPos:F3}, 세로: {shipCenterVerticalPos:F3}");
        DebugLog($"목표 위치(비율) - 가로: {targetShipPositionX:F3}, 세로: {targetShipPositionY:F3}");
        DebugLog($"가용 영역 - 좌: {leftBoundaryPercent:F2}, 우: {rightBoundaryPercent:F2}");
        DebugLog(
            $"함선 폭 비율: {shipRightScreenPos - shipLeftScreenPos:F3}, 함선 높이 비율: {shipTopScreenPos - shipBottomScreenPos:F3}");

        // 경계 초과 여부 확인 (작은 오차 허용)
        float tolerance = 0.01f; // 1% 오차 허용

        if (shipLeftScreenPos < leftBoundaryPercent - tolerance)
            DebugLog($"<color=red>경고: 함선 좌측 끝({shipLeftScreenPos:F3})이 좌측 경계({leftBoundaryPercent:F2})를 초과함</color>");

        if (shipRightScreenPos > rightBoundaryPercent + tolerance)
            DebugLog(
                $"<color=red>경고: 함선 우측 끝({shipRightScreenPos:F3})이 우측 경계({rightBoundaryPercent:F2})를 초과함 (초과량: {shipRightScreenPos - rightBoundaryPercent:F3})</color>");

        // 실제 함선 중심 위치와 목표 위치 비교
        float centerXDifference = Math.Abs(shipCenterHorizontalPos - targetShipPositionX);
        if (centerXDifference > tolerance)
            DebugLog(
                $"<color=yellow>참고: 함선 중심 X 위치({shipCenterHorizontalPos:F3})가 목표 위치({targetShipPositionX:F3})와 차이가 있음 (차이: {centerXDifference:F3})</color>");
    }
}
