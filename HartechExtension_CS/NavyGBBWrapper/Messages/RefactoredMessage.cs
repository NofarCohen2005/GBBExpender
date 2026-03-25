using Globals;

namespace NavyGBBWrapper.Messages
{
    public struct RefactoredMessage : IBaseStruct
    {
        public int MaxField;
        public uint MinField;

        public object SetDefault()
        {
            MaxField = int.MaxValue;
            MinField = 0;
            return this;
        }

        public static RefactoredMessage Default() => (RefactoredMessage)default(RefactoredMessage).SetDefault();
    }
}
