using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum EWeaponType { Melee, Range };
    public string ToolName;
    public string ToolDesc;
    public int AttackStrength = 10;
    public EWeaponType WeaponType = EWeaponType.Melee;

    public GameObject Projectile = null;
    public float ProjectileSpeed;
    public Transform LaunchPosition;
}
