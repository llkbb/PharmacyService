namespace PharmacyChain.Exceptions
{
    /// <summary>
    /// Виняток для порушення бізнес-правил
    /// </summary>
    public class BusinessLogicException : Exception
    {
        public BusinessLogicException(string message) : base(message)
        {
        }

        public BusinessLogicException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
