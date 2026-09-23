using System.ComponentModel.DataAnnotations;

namespace App.Common;

public static class PasswordValidator
{
	private const int Min = 4;
	private const int Max = 30;

	public static string Validate(string password)
	{
		return password.Length is >= Min and <= Max
			? password : throw new ValidationException("Password must be at least " + Min + " and " + Max);
	}
}
