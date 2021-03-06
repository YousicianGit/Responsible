using System;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.State;
using UniRx;

namespace Responsible.TestInstructions
{
	internal class ContinuationTestInstruction<T1, T2> : TestInstructionBase<T2>
	{
		public ContinuationTestInstruction(
			ITestInstruction<T1> first,
			Func<T1, ITestInstruction<T2>> selector,
			SourceContext sourceContext)
			: base(() => new State(first, selector, sourceContext))
		{
		}

		private class State : TestOperationState<T2>
		{
			private readonly ITestOperationState<T1> first;
			private readonly Func<T1, ITestInstruction<T2>> selector;

			[CanBeNull] private ITestOperationState<T2> nextInstruction;

			public State(
				ITestInstruction<T1> first,
				Func<T1, ITestInstruction<T2>> selector,
				SourceContext sourceContext)
				: base(sourceContext)
			{
				this.first = first.CreateState();
				this.selector = selector;
			}

			protected override IObservable<T2> ExecuteInner(RunContext runContext) => this.first
				.Execute(runContext)
				.ContinueWith(result =>
				{
					this.nextInstruction = this.selector(result).CreateState();
					return this.nextInstruction.Execute(runContext);
				});

			public override void BuildDescription(StateStringBuilder builder) =>
				builder.AddContinuation(this.first, this.nextInstruction);
		}
	}
}