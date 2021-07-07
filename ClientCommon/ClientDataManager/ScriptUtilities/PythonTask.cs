namespace ClientCommon.ClientService.ScriptUtilities
{
    public class PythonTask
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
