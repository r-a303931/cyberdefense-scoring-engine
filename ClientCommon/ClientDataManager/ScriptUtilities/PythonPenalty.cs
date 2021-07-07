namespace ClientCommon.ClientService.ScriptUtilities
{
    public class PythonPenalty
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
