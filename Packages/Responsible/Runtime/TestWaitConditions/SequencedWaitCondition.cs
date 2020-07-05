using System;
using Responsible.Context;
using UniRx;

namespace Responsible.TestWaitConditions
{
	internal class SequencedWaitCondition<TFirst, TSecond> : ITestWaitCondition<TSecond>
	{
		private readonly ITestWaitCondition<TFirst> first;
		private readonly ITestWaitCondition<TSecond> second;

		public SequencedWaitCondition(
			ITestWaitCondition<TFirst> first, ITestWaitCondition<TSecond> second)
		{
			this.first = first;
			this.second = second;
		}

		public IObservable<TSecond> WaitForResult(RunContext runContext, WaitContext waitContext) => this.first
			.WaitForResult(runContext, waitContext)
			.ContinueWith(_ => this.second.WaitForResult(runContext, waitContext));

		public void BuildDescription(ContextStringBuilder builder)
		{
			builder.Add("FIRST", this.first);
			builder.Add("AND THEN",  this.second);
		}

		public void BuildFailureContext(ContextStringBuilder builder) => this.BuildDescription(builder);
	}
}