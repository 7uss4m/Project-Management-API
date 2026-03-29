namespace TaskManager.Domain.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }

    public static NotFoundException For<T>(int id) =>
        new($"{typeof(T).Name} with id {id} was not found.");
}
