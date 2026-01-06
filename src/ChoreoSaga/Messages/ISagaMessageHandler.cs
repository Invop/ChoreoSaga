namespace ChoreoSaga.Messages;

public interface ISagaMessageHandler<in TSagaMessage> where TSagaMessage : ISagaMessage
{
    ValueTask HandleAsync(IMessageContext<TSagaMessage> context, CancellationToken cancellationToken = default);
}