using System;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Responsible.Tests.Runtime.Utilities;
using UniRx;
using UnityEngine;
using static Responsible.Responsibly;

namespace Responsible.Tests.Runtime
{
	public class ErrorOutputTests
	{
		private class ResponderState
		{
			public string Condition1Name { get; set; }
			public bool Condition1 { get; set; }

			public string Condition2Name { get; set; }
			public bool Condition2 { get; set; }

			public string ResponseName { get; set; }

			public Action ResponseAction { get; set; }
		}

		private static ITestResponder<Unit> MakeResponder(ResponderState state) =>
			WaitForCondition(state.Condition1Name, () => state.Condition1)
				.AndThen(_ => WaitForCondition(state.Condition2Name, () => state.Condition2))
				.ThenRespondWith(state.ResponseName, Do(state.ResponseAction));

		private static ITestInstruction<Unit> MakeInstruction(ResponderState state1, ResponderState state2) =>
			RespondToAnyOf(MakeResponder(state1), MakeResponder(state2))
				.Until(WaitForCondition("Never", () => false))
				.ExpectWithinSeconds(10);

		[Test]
		public void ErrorOutput_IsAsExpected()
		{
			// Some aspects of the output is tested in other smaller tests, but
			// Having one sort of system test to assert the exact output is nice.
			// This could arguably be a bunch of smaller tests, also, but those
			// might become a pain to maintain, and the core functionality is
			// more important than the output details in various situations.
			// If issues with output arise, maybe this will be changed.
			// We especially do not want to assert things like line numbers in multiple places!
			var logger = Substitute.For<ILogger>();
			var scheduler = new TestScheduler();
			var poll = new Subject<Unit>();
			using (TestInstruction.OverrideExecutor(scheduler, poll, logger))
			{
				var state1 = new ResponderState
				{
					Condition1Name = "Cond 1.1",
					Condition2Name = "Cond 1.2",
					ResponseName = "Response 1",
					ResponseAction = () => throw new Exception("Exception"),
				};

				var state2 = new ResponderState
				{
					Condition1Name = "Cond 2.1",
					Condition2Name = "Cond 2.2",
					ResponseName = "Response 2",
				};

				MakeInstruction(state1, state2)
					.ToObservable()
					.CatchIgnore()
					.Subscribe();

				// Store logger output to variable, for easier setup (can be actually logged)
				string message = null;
				logger.Log(LogType.Error, Arg.Do<string>(msg => message = msg));

				// Advance time and frames, and complete one condition
				scheduler.AdvanceBy(TimeSpan.FromSeconds(1.5));
				poll.OnNext(Unit.Default);
				state1.Condition1 = true;
				poll.OnNext(Unit.Default);

				// Advance time and frames, and complete second condition
				scheduler.AdvanceBy(TimeSpan.FromSeconds(1.5));
				poll.OnNext(Unit.Default);
				state1.Condition2 = true;
				poll.OnNext(Unit.Default);

				StringAssert.StartsWith(ExpectedOutput, message);
			}
		}

		private const string ExpectedOutput =
			@"Test operation execution failed:
RESPOND TO
  ANY OF
    Response 1
    Response 2
UNTIL
  Never
 
Failed after 3.00s and 4 frames
 
Failure context:
RESPOND TO
  ANY OF
    [.] Response 1
      WAIT FOR
        FIRST
          [✓] Cond 1.1 (Completed in: 1.50s and 2 frames)
        AND THEN
          [✓] Cond 1.2 (Completed in: 3.00s and 4 frames)
      AND THEN RESPOND WITH
        [!] DO<Unit>
          FAILED WITH: Exception
          MakeResponder (at Packages/Responsible/Tests/Runtime/ErrorOutputTests.cs:30)
    [.] Response 2
      WAIT FOR
        FIRST
          [.] Cond 2.1
        AND THEN ...
      AND THEN RESPOND WITH ...
UNTIL
  [.] Never
 
Test instruction stack: 
MakeInstruction (at Packages/Responsible/Tests/Runtime/ErrorOutputTests.cs:35)
ErrorOutput_IsAsExpected (at Packages/Responsible/Tests/Runtime/ErrorOutputTests.cs:68)
 
Error: NUnit.Framework.AssertionException: Synchronous test action failed:";
	}
}