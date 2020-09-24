using System;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Responsible.Tests.Runtime.Utilities;
using UniRx;
using UnityEngine;
using UnityEngine.TestTools;
using static Responsible.Responsibly;

namespace Responsible.Tests.Runtime
{
	public class ErrorLogHandlingTests : ResponsibleTestBase
	{
		private const string ErrorMessage = "Error!";

		// JIC someone runs these together with their test suite...
		private bool wereLogsIgnored;

		[SetUp]
		public void SetUp()
		{
			this.wereLogsIgnored = LogAssert.ignoreFailingMessages;
			LogAssert.ignoreFailingMessages = false;
		}

		[TearDown]
		public void TearDown()
		{
			LogAssert.ignoreFailingMessages = this.wereLogsIgnored;
		}

		[Test]
		public void LoggingError_CausesProperFailure_WhenErrorIsIntercepted()
		{
			this.LogErrorFromInstructionSynchronously();

			Assert.IsInstanceOf<AssertionException>(this.Error);
			Assert.IsInstanceOf<UnhandledLogMessageException>(this.Error.InnerException);
		}

		[Test]
		public void LoggingError_DoesNotCauseFailure_WhenErrorIsNotIntercepted()
		{
			LogAssert.ignoreFailingMessages = true;
			this.LogErrorFromInstructionSynchronously();
			Assert.IsNull(this.Error);
		}

		[Test]
		public void ExpectLog_Works_WhenErrorIsIntercepted()
		{
			this.Executor.ExpectLog(LogType.Error, new Regex(ErrorMessage));
			this.LogErrorFromInstructionSynchronously();
		}

		[Test]
		public void LogAssert_Works_WhenErrorIsNotIntercepted()
		{
			LogAssert.ignoreFailingMessages = true;
			this.LogErrorFromInstructionSynchronously();
			LogAssert.Expect(LogType.Error, ErrorMessage);
		}

		[Test]
		[Ignore("Should fail, can't assert that with the Unity test runner, run manually")]
		public void NotLoggedButExpectedError_FailsTest()
		{
			this.Executor.ExpectLog(LogType.Error, new Regex("foo"));
		}

		private void LogErrorFromInstructionSynchronously()
		{
			Do("Log error", () => Debug.LogError(ErrorMessage))
				.ToObservable(this.Executor)
				.Subscribe(Nop, this.StoreError);
		}
	}
}