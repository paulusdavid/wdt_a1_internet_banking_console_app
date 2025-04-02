namespace Assignment1.Util.Menu
{
    public static class MainMenuUtils
    {
        public static IMenu CreateMenu(string customerName)
        {
            if (customerName != null)
            {
                return new MainMenu(customerName);
            }
            else
            {
                return new MainMenu(customerName);
            }
        }
    }
}
