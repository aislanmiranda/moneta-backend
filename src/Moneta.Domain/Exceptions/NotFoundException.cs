namespace Moneta.Domain.Exceptions;

/// <summary>A referenced entity does not exist. The Api layer maps this to HTTP 404.</summary>
public class NotFoundException(string message) : Exception(message);
