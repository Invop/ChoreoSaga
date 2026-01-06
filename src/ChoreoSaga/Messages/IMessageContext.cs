namespace ChoreoSaga.Messages;

public interface IMessageContext<out TMessage> where TMessage : ISagaMessage
{
    TMessage Message { get; }
    string CorrelationId { get; }
    string MessageId { get; }
    string SenderId { get; }
}