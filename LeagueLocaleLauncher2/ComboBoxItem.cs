namespace LeagueLocaleLauncher
{
    public class ComboBoxItem
    {
        public string Value;
        public ComboBoxItem(string value) => Value = value;

        public override string ToString()
        {
            return Translation.Translate(Value);
        }
    }
}
