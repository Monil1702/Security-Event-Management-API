namespace SecurityEventManagement.Api.Exceptions;

public sealed class ResourceNotFoundException(string message) : Exception(message);

public sealed class ResourceConflictException(string message) : Exception(message);

public sealed class InvalidStateTransitionException(string message) : Exception(message);
