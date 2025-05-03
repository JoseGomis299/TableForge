namespace TableForge.Exceptions
{
    internal class InvalidCellValueException : System.Exception
    {
        public InvalidCellValueException(string message) : base(message) { }
    }
}