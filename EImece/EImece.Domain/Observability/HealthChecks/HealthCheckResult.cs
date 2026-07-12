namespace EImece.Domain.Observability.HealthChecks
{
    public sealed class HealthCheckResult
    {
        public HealthCheckResult(string name, HealthStatus status, string message)
        {
            Name = name;
            Status = status;
            Message = message;
        }

        public string Name { get; }

        public HealthStatus Status { get; }

        public string Message { get; }

        public static HealthCheckResult Up(string name, string message)
        {
            return new HealthCheckResult(name, HealthStatus.Up, message);
        }

        public static HealthCheckResult Down(string name, string message)
        {
            return new HealthCheckResult(name, HealthStatus.Down, message);
        }
    }
}
