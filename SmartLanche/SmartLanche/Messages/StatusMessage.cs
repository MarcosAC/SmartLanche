namespace SmartLanche.Messages
{
    public class StatusMessage
    {
        public string Content { get; }
        public bool IsSuccess { get; }
        
        public StatusMessage(string content, bool isSuccess = false)
        {
            Content = content;
            IsSuccess = isSuccess;
        }
    }
}
