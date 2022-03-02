namespace BluePrism.Api.Services
{
    using System.Threading;

    public class CallTokenAccessor : ITokenAccessor
    {
        private AsyncLocal<string> _tokenString;

        public CallTokenAccessor() => _tokenString = new AsyncLocal<string>();
        public string TokenString => _tokenString.Value.Replace("Bearer ", string.Empty);
        public void SetToken(string token) => _tokenString.Value = token ?? string.Empty;
    }
}
