namespace App.Common;

public abstract class AppException(string message) : Exception(message)
{
	public abstract int StatusCode { get; }
}

public class NotFoundException(string message = "Lol, not found ;(") : AppException(message)
{
	public override int StatusCode => 404;
}

public class ValidationException(string message = "Lol, validation failed ;(") : AppException(message)
{
	public override int StatusCode => 400;
}

public class ConflictException(string message = "Lol, conflict ;(") : AppException(message)
{
	public override int StatusCode => 409;
}

public class AuthException(string message = "Lol, auth failed ;(") : AppException(message)
{
	public override int StatusCode => 401;
}

public class ForbiddenException(string message = "Lol, forbidden;(") : AppException(message)
{
	public override int StatusCode => 403;
}

