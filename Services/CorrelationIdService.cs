using FerramentariaTest.Services.Interfaces;

namespace FerramentariaTest.Services
{
    public class CorrelationIdService : ICorrelationIdService
    {
        // REMOVED "static" keyword
        private readonly AsyncLocal<string> _currentCorrelationId = new AsyncLocal<string>();

        public string GetCurrentCorrelationId()
        {
            return _currentCorrelationId.Value ??= GenerateNewCorrelationId();
        }

        public string GenerateNewCorrelationId() => Guid.NewGuid().ToString();

        public IDisposable BeginScope(string correlationId = null)
        {
            var previous = _currentCorrelationId.Value;
            _currentCorrelationId.Value = correlationId ?? GenerateNewCorrelationId();
            return new CorrelationIdScope(previous, this);
        }

        private class CorrelationIdScope : IDisposable
        {
            private readonly string _previousCorrelationId;
            private readonly CorrelationIdService _service;

            public CorrelationIdScope(string previousCorrelationId, CorrelationIdService service)
            {
                _previousCorrelationId = previousCorrelationId;
                _service = service;
            }

            public void Dispose()
            {
                _service._currentCorrelationId.Value = _previousCorrelationId;
            }
        }
    }
}
