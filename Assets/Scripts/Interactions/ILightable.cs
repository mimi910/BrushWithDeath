public interface ILightable
{
    bool IsLit { get; }
    void Light(PlayerController player);
}
