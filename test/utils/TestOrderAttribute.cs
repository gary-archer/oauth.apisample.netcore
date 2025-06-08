namespace FinalApi.Test.Utils
{
    using System;

    /*
     * Order tests by this custom attribute
     */
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class TestOrderAttribute : Attribute
    {
        public TestOrderAttribute(int testOrder)
        {
            this.TestOrder = testOrder;
        }

        public int TestOrder { get; private set; }
    }
}
