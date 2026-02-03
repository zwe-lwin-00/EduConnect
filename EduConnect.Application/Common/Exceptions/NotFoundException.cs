namespace EduConnect.Application.Common.Exceptions;

public class NotFoundException : BusinessException
{
    public NotFoundException(string entityName, object key) 
        : base($"{entityName} with key {key} was not found.", "NOT_FOUND")
    {
    }
}
