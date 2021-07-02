public class HealthItem : Item
{
    public int healing;  // how much this item heals for
    
    protected override void OnCollect(Player player)
    {
        player.GainHealth(healing);
    }
}