public class WeaponItem : Item
{
    protected override void OnCollect(Player player)
    {
        player.UpgradeWeapon();
    }
}