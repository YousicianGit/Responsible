using System;
using System.Collections;
using NUnit.Framework;
using UniRx;
using UnityEngine.TestTools;
using static Responsible.Responsibly;
// ReSharper disable AccessToModifiedClosure

namespace Responsible.Tests.Runtime
{
	public class UntilReadyToTests : ResponsibleTestBase
	{
		[UnityTest]
		public IEnumerator UntilReadyToRespond_DoesNotExecuteFirst_WhenSecondIsFirstToBeReady()
		{
			var cond1 = false;
			var cond2 = false;
			var mayComplete2 = false;

			var executed1 = false;
			var executed2 = false;

			var first = WaitForCondition("1", () => cond1)
				.ThenRespondWith("Do 1", _ => executed1 = true);

			var second = WaitForCondition("2", () => cond2)
				.ThenRespondWith("Do 2", WaitForCondition("May complete 2", () => mayComplete2)
					.ThenRespondWith("Complete 2", _  => executed2 = true)
					.ExpectWithinSeconds(1));

			first.Optionally().UntilReadyTo(second).ExpectWithinSeconds(1).ToObservable().Subscribe();

			cond2 = true;
			yield return null;
			cond1 = true;

			// A few yields to be safe
			yield return null;
			yield return null;

			mayComplete2 = true;
			yield return null;

			Assert.AreEqual(
				(false, true),
				(executed1, executed2));
		}

		[UnityTest]
		public IEnumerator UntilReadyToRespond_TimesOut_AsExpected()
		{
			var completed = false;

			var responder = Never.ThenRespondWith("complete", _ => completed = true);

			responder
				.Optionally()
				.UntilReadyTo(responder)
				.ExpectWithinSeconds(1)
				.ToObservable()
				.Subscribe(Nop, this.StoreError);

			yield return null;
			this.Scheduler.AdvanceBy(OneSecond);
			yield return null;

			Assert.IsFalse(completed);
			Assert.IsInstanceOf<AssertionException>(this.Error);
		}
	}
}