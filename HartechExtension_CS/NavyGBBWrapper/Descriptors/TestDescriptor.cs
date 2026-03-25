namespace NavyGBBWrapper.Descriptors
{
    public struct TestDescriptor : IBaseStruct
    {
        public int FieldA;

        public object SetDefault()
        {
            return Default();
        }

        public static TestDescriptor Default()
        {
            return new TestDescriptor
            {
                FieldA = 10
            };
        }
    }
}
