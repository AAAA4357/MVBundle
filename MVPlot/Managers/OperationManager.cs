namespace MVPlot.Managers
{
    public class OperationManager
    {
        public static uint maxCapacity
        {
            get => 32;
            set
            {
                uint capacity = 32;
                if (capacity > value && value != 0 && Operations.Count > 0)
                {
                    Operations = (Stack<UserOperationBase>)Operations.Reverse();
                    while (Operations.Count != capacity) Operations.Pop();
                    Operations = (Stack<UserOperationBase>)Operations.Reverse();
                }
            }
        }

        static Stack<UserOperationBase> Operations = new();

        static Stack<UserOperationBase> RedoOperations = new();

        public static void Add(UserOperationBase operation)
        {
            RedoOperations.Clear();
            if (Operations.Count == maxCapacity)
            {
                List<UserOperationBase> operations = [.. Operations];
                operations.RemoveAt(0);
                Operations.Clear();
                foreach (UserOperationBase userOperation in operations) Operations.Push(userOperation);
            }
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
    }

    public abstract class UserOperationBase
    {
        public abstract void ReDO();

        public abstract void UnDO();
    }
}
