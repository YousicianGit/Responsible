using System.Runtime.CompilerServices;
using JetBrains.Annotations;
// ReSharper disable ExplicitCallerInfoArgument

namespace Responsible
{
	// See RF.WaitFor.cs for documentation
	public static partial class RF
	{
		/// <summary>
		/// Constructs a test instruction that return a value synchronously.
		/// Wouldn't be a monad without this ;)
		/// </summary>
		/// <summary>
		/// Constructs a test instruction that return a value synchronously.
		/// Wouldn't be a monad without this ;)
		/// </summary>
		[Pure]
		public static ITestInstruction<T> Return<T>(
			T value,
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> Do(() => value,
				sourceFilePath,
				sourceLineNumber);
	}
}