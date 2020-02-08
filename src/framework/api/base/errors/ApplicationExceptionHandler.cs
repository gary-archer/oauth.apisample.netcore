namespace Framework.Api.Base.Errors
{
    using System;

    /*
     * An interface to allow the application to translate exceptions when needed
     */
    public class ApplicationExceptionHandler
    {
        /*
         * The default implementation does a null translation but can be overridden
         */
        public virtual Exception Translate(Exception ex)
        {
            return ex;
        }
    }
}
