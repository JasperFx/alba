namespace Alba.Exceptions;

public class EmptyResponseException : Exception
{
    public EmptyResponseException() : base("There is no content in the Response.Body")
    {
    }
}