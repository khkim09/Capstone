public class WeaponControlRoom : Room
{
    public float accuracyBonus = 0f;

    protected override void UpdateRoom()
    {
        if (!IsOperational()) return;
    }

    private void CalculateAccuracyBonus()
    {
    }
}
