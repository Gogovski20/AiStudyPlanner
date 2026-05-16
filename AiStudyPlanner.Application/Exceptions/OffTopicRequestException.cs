namespace AiStudyPlanner.Application.Exceptions
{
    public class OffTopicRequestException : Exception
    {
        public OffTopicRequestException()
            : base("This request is not related to studying or interview preparation.") { }
    }
}
