using System;

namespace Assignment1.Util.Menu
{
    public interface IMenu
    {
        //void SetCustomerName(string name);
        string DisplayMenu();
    }

    public enum MenuOption
    {
        Exit,
        Deposit,
        Withdraw,
        Transfer,
        Statement,
        Logout
    }
}
