using MoonSharp.Interpreter;

namespace ClientCommon.ClientService.ScriptUtilities
{
    [MoonSharpUserData]
    public class LuaTask
    {
        public bool IsCompleted { get; private set; }

        public int TaskCompleted()
        {
            IsCompleted = true;
            throw new FinishExecutionException();
        }

        public int TaskIncomplete()
        {
            IsCompleted = false;
            throw new FinishExecutionException();
        }
    }
}
