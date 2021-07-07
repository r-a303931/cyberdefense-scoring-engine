using MoonSharp.Interpreter;

namespace ClientCommon.ClientService.ScriptUtilities
{
    [MoonSharpUserData]
    public class LuaPenalty
    {
        public readonly int ApplyPenalty = 0;

        public readonly int DontApplyPenalty = 1;
    }
}
