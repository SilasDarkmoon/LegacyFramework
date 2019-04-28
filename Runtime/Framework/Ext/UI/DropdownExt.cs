using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class DropdownExt
{
    public static void AddTextOptions(this Dropdown dropdown, List<string> list)
    {
        dropdown.AddOptions(list);
    }
}
