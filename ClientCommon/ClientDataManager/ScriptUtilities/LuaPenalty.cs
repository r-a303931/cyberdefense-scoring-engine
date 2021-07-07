using MoonSharp.Interpreter;

namespace ClientCommon.ClientService.ScriptUtilities
{
    [MoonSharpUserData]
    public class LuaPenalty
    {
        public bool DoesApply { get; private set; }

        public void ApplyPenalty()
        {
            DoesApply = true;
            throw new FinishExecutionException();
        }

        public void DontApplyPenalty()
        {
            DoesApply = false;
            throw new FinishExecutionException();
        }
    }
}
