namespace SmartLanche.Messages
{
    public record StatusMessage(string Content, bool IsSuccess);
    public record OrderCreatedMessage(Models.Order NewOrder);
    public record ProductsChangedMessage();
    public record ClientsChangedMessage();
}
