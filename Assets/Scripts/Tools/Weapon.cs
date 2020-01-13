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

    public virtual void HandleState(Character character) {}

    protected void LaunchProjectile(GameObject obj, Transform launchPoint, float speed, int damage)
    {
        var proj = Instantiate(obj, launchPoint.position, launchPoint.rotation * Quaternion.Euler(0.0f, 90.0f, 0.0f));
        proj.GetComponent<Rigidbody>().velocity = launchPoint.forward * speed;
        proj.GetComponent<Projectile>().Damage = damage;
    }
}
