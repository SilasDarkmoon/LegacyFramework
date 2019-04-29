using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XLua;

public static class HotfixInjectConfig
{
    [Hotfix]
    public static List<Type> hotFixTypeByField = new List<Type>()
    {
        
    };

    [Hotfix]
    public static List<Type> hotFixTypeByProperty
    {
        get
        {
            return (from type in Assembly.Load("Assembly-CSharp").GetTypes()
                where type.Namespace != null && (type.Namespace.StartsWith("CoreGame.UI") || type.Namespace.StartsWith("ChatUI"))
                select type).ToList();
        }
    }
}
