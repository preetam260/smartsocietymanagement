namespace SmartSociety.Application.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string entity, Guid id) 
        : base($"{entity} with the ID: {id} was not found") {}
    
    public NotFoundException(string message) : base(message) {}
}