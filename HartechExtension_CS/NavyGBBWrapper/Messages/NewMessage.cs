namespace NavyGBBWrapper.Messages
{
    public struct NewMessage : IBaseStruct
    {
        public int FirstParam;

        public object SetDefault()
        {
            return Default();
        }

        public static NewMessage Default()
        {
            return new NewMessage
            {
                FirstParam = 0
            };
        }
    }
}
