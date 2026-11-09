namespace Moneta.Domain.Exceptions;

/// <summary>A business rule was violated. The Api layer maps this to HTTP 400.</summary>
public class DomainValidationException(string message) : Exception(message);
