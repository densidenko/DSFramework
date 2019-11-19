using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;

namespace DSFramework.Threading.DataFlow
{
    public class SafeActionBlock<TInput> : ITargetBlock<TInput>
    {
        private readonly Func<TInput, Task> _action;
        private readonly ActionBlock<TInput> _actionBlock;
        private readonly ILogger _logger;

        public Task Completion => _actionBlock.Completion;

        public int InputCount => _actionBlock.InputCount;

        public SafeActionBlock(ILogger logger, Action<TInput> action, ExecutionDataflowBlockOptions dataFlowBlockOptions)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            _logger = logger;
            _action = input =>
            {
                action(input);
                return Task.CompletedTask;
            };
            _actionBlock = new ActionBlock<TInput>(ActionAsync, dataFlowBlockOptions);
        }

        public SafeActionBlock(ILogger logger, Func<TInput, Task> action, ExecutionDataflowBlockOptions dataFlowBlockOptions)
        {
            _logger = logger;
            _action = action ?? throw new ArgumentNullException(nameof(action));
            _actionBlock = new ActionBlock<TInput>(ActionAsync, dataFlowBlockOptions);
        }

        public override string ToString() => _actionBlock.ToString();

        public void Complete() => _actionBlock.Complete();

        public void Fault(Exception exception) => ((IDataflowBlock)_actionBlock).Fault(exception);

        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader,
                                                  TInput messageValue,
                                                  ISourceBlock<TInput> source,
                                                  bool consumeToAccept)
            => ((ITargetBlock<TInput>)_actionBlock).OfferMessage(messageHeader, messageValue, source, consumeToAccept);

        private async Task ActionAsync(TInput input)
        {
            try
            {
                await _action(input);
            }
            catch (Exception e)
            {
                _logger.LogError(new EventId(-1), e, e.Message);
            }
        }
    }
}