using System;
using Responsible.Context;
using UniRx;

namespace Responsible.TestInstructions
{
	internal class ExpectTestResponse<T> : ITestInstruction<T>
	{
		private readonly ITestResponder<T> responder;
		private readonly TimeSpan timeout;
		private readonly SourceContext sourceContext;

		public ExpectTestResponse(
			ITestResponder<T> responder,
			TimeSpan timeout,
			SourceContext sourceContext)
		{
			this.responder = responder;
			this.timeout = timeout;
			this.sourceContext = sourceContext;
		}

		public IObservable<T> Run(RunContext runContext)
			=> runContext.Executor
			.WaitFor(this.responder, this.timeout, runContext.SourceContext(this.sourceContext))
			.ContinueWith(instruction => instruction.Run(runContext));
	}
}