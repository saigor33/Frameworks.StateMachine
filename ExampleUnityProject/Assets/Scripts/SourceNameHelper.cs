namespace Frameworks.StateMachine.StateGraphVisualizer
{
    static class SourceNameHelper
    {
        public static string GetSourceName(string sourceId)
        {
            int lastIndexOf = sourceId.LastIndexOf('.');

            if (lastIndexOf == -1)
            {
                return sourceId;
            }

            return sourceId.Substring(lastIndexOf + 1);
        }
    }
}