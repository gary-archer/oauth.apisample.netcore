namespace Framework.Api.Base.Errors
{
    using System;

    /*
     * An interface to allow the application to translate some types of exception
     */
    public interface IApplicationExceptionHandler
    {
        Exception Translate(Exception ex);
    }
}
