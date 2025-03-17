using UnityEngine;

public class OxygenRoom : Room
{
    public float oxygenGenerationRate = 5f; // 초당 생성되는 산소량(%)

    protected override void UpdateRoom()
    {
        if (!IsOperational()) return;

        // 방 산소 레벨 업데이트
        SetOxygenLevel(OxygenLevel.Normal);

        // 주변 방으로 산소 확산
        foreach (Room adjacentRoom in adjacentRooms)
            // 인접한 방의 현재 산소 레벨보다 높은 경우에만 확산
            if ((int)adjacentRoom.GetOxygenLevel() < (int)OxygenLevel.Normal)
            {
                OxygenLevel newLevel =
                    (OxygenLevel)Mathf.Min((int)OxygenLevel.Normal, (int)adjacentRoom.GetOxygenLevel() + 1);
                adjacentRoom.SetOxygenLevel(newLevel);
            }
    }

    protected override void ApplyCurrentLevelEffects()
    {
        base.ApplyCurrentLevelEffects();

        // 레벨에 따른 산소 생성 속도 설정
        switch (currentPowerLevel)
        {
            case 1:
                oxygenGenerationRate = 5f;
                break;
            case 2:
                oxygenGenerationRate = 8f;
                break;
            default:
                oxygenGenerationRate = 3f;
                break;
        }
    }
}
