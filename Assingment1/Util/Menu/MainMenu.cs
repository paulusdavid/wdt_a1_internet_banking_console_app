namespace Assignment1.Util.Menu
{
    class MainMenu : IMenu
    {
        private string customerName;

        public MainMenu(string name)
        {
            customerName = name ?? "Guest";
        }

        public string DisplayMenu()
        {
            string menu = $"""
            --- Welcome, {customerName} ---
            [1] Deposit
            [2] Withdraw
            [3] Transfer
            [4] My Statement
            [5] Logout
            [0] Exit

            Enter an option:
            """;
            return menu;
        }
    }
}
