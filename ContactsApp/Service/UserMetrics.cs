using System.Diagnostics.Metrics;
using System.Threading;

namespace ContactsApp.Service
{
    public class UserMetrics
    {
        private Histogram<int> NumberOfContactsPerUserHistogram { get; }
        private ObservableUpDownCounter<int> minorUsers { get; }
        private Histogram<double> getUsersResponseTime; 

        private int _minorUsers = 0;

        public UserMetrics(IMeterFactory meterFactory)
        {
            var meter = meterFactory.Create("user.service");
            NumberOfContactsPerUserHistogram = meter.CreateHistogram<int>("contacts-per-user", "Contact", "Number of contacts per users");
            minorUsers = meter.CreateObservableUpDownCounter<int>("minor-users", () => _minorUsers);
            getUsersResponseTime = meter.CreateHistogram<double>("http_response_time_get_users", "milliseconds", "Distribution of HTTP response times for get users request");
        }

        public void RecordNumberOfContactsPerUser (int ammount) => NumberOfContactsPerUserHistogram.Record(ammount);
        public void RecordResponseTimeHttpGetUsersRequest(double ammount) => getUsersResponseTime.Record(ammount);
        public void UpdateMinorUserCount() => _minorUsers++;

    }
}
