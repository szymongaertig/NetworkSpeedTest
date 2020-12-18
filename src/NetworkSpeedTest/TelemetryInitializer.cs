using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace NetworkSpeedTest
{
    public class TelemetryInitializer : ITelemetryInitializer
    {
        private string _appName;

        public TelemetryInitializer(string appName)
        {
            _appName = appName;
        }

        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.Cloud.RoleName = _appName;
        }
    }
}