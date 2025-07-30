namespace InvenBank.API.Middleware
{
    public class ValidationException : Exception
    {
        public IEnumerable<string> Errors { get; }

        public ValidationException(string message) : base(message)
        {
            Errors = new List<string> { message };
        }

        public ValidationException(IEnumerable<string> errors) : base("Error de validación")
        {
            Errors = errors;
        }

        public ValidationException(string message, IEnumerable<string> errors) : base(message)
        {
            Errors = errors;
        }
    }

}
