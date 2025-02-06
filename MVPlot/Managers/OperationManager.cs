namespace MVPlot.Managers
{
    public class OperationManager
    {
        static Stack<UserOperationBase> Operations = new();

        static Stack<UserOperationBase> RedoOperations = new();

        public static void Add(UserOperationBase operation)
        {
            RedoOperations.Clear();
            Operations.Push(operation);
        }

        public static void Undo()
        {
            if (Operations.Count == 0) return;
            UserOperationBase operation = Operations.Pop();
            operation.UnDO();
            RedoOperations.Push(operation);
        }

        public static void Redo()
        {
            if (RedoOperations.Count == 0) return;
            UserOperationBase operation = RedoOperations.Pop();
            operation.ReDO();
            Operations.Push(operation);
        }

        public static void Clear()
        {
            Operations.Clear();
            RedoOperations.Clear();
        }
    }

    public abstract class UserOperationBase
    {
        public abstract void ReDO();

        public abstract void UnDO();
    }
}
