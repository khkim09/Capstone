public class EngineRoom : Room
{
    public float warpCharge; // 워프 충전량
    public float fuelEfficiency;
    public float dodgeRate;

    protected override void UpdateRoom()
    {
        if (isActive && HasEnoughCrew())
        {
            // 엔진 효율, 회피율 계산 등
        }
    }
}
