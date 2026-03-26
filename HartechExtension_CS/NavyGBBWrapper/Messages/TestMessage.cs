using Globals;

namespace NavyGBBWrapper.Messages
{
    public struct TestMessage : IBaseStruct
    {
        public int TestProperty;

        public object SetDefault()
        {
            TestProperty = 0123;
            return this;
        }

        public static TestMessage Default() => (TestMessage)default(TestMessage).SetDefault();
    }
}
