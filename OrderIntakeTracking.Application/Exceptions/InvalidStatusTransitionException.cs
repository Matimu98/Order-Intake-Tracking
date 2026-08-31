namespace OrderIntakeTracking.Application.Exceptions;

public class InvalidStatusTransitionException(string message) : Exception(message);
