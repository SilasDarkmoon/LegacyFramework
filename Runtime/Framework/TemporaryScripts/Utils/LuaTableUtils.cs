using XLua;

public static class LuaTableUtils
{
    public static LuaTable GetXLuaTable(string tablePath)
    {
        object[] result = LuaBehaviour.luaEnv.DoString("return require('" + tablePath + "')");

        if (result != null && result.Length > 0)
        {
            return result[0] as LuaTable;
        }

        return null;
    }
}
