using MoonSharp.Interpreter;

namespace ClientCommon.ClientService.ScriptUtilities
{
    [MoonSharpUserData]
    public class LuaTask
    {
        public readonly int TaskCompleted = 0;

        public readonly int TaskIncomplete = 1;
    }
}
