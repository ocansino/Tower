public class RubyItem : Item
{
    public int value; // currency value of this item
    protected override void OnCollect(Player player)
    {
        GameManager.instance.GainRubies(value);
    }
}