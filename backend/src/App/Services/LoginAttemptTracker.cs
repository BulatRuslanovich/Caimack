using System.Collections.Concurrent;

namespace App.Services;

public class LoginAttemptTracker(TimeProvider time)
{
	private record Attempts(int Fails, DateTimeOffset LockUntil, DateTimeOffset LastFailAt);

	private readonly ConcurrentDictionary<string,  Attempts> _byUser = new(StringComparer.Ordinal);

	public TimeSpan? LockoutRemaining(string username)
	{
		if (!_byUser.TryGetValue(username, out var attempts))
		{
			return null;
		}

		var remaining = attempts.LockUntil - time.GetUtcNow();

		return remaining > TimeSpan.Zero ? remaining : null;
	}

	public void WriteSuccess(string username)
	{
		_byUser.TryRemove(username, out _);
	}

	public void WriteFail(string username)
	{
		var now = time.GetUtcNow();
		var window = TimeSpan.FromMinutes(15);

		_byUser.AddOrUpdate(username, _ => new Attempts(1, DateTimeOffset.MinValue, now), (_, prev) =>
		{
			var fails = now - prev.LastFailAt > window ? 1 : prev.Fails + 1;
			return new Attempts(fails, fails >= 5 ? now + window : prev.LockUntil, now);
		});

		if (_byUser.Count < 1000)
		{
			return;
		}


		//INFO: I very much doubt that this piece of code will ever work -_-
		foreach (var (name, attempts) in _byUser)
		{
			if (now - attempts.LastFailAt > window && attempts.LockUntil <= now)
			{
				_byUser.TryRemove(name, out _);
			}
		}
	}
}
