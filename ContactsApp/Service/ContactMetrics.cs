using System.Diagnostics.Metrics;

namespace ContactsApp.Service
{
    public class ContactMetrics
    {
        private readonly UpDownCounter<int> _totalContact;
        private readonly Histogram<double> _contactCreationProcessingTime;

        public ContactMetrics(IMeterFactory meterFactory)
        {
            var meter = meterFactory.Create("contact.service");
            _totalContact = meter.CreateUpDownCounter<int>("ContactCounter", "Contact", "Number of created Contacts");
            _contactCreationProcessingTime = meter.CreateHistogram<double>("Contact.service.Creation_prosessing_time");
        }

        public void ToTalContactUpdate(int quantity) => _totalContact.Add(quantity);
        public string TotalContact () => _totalContact.ToString() ?? "";
        
        public void RecordContactCreationProcess(double time) =>_contactCreationProcessingTime.Record(time);
        public string RecordedContactCreationProcess() => _contactCreationProcessingTime.ToString()??"";
    }
}