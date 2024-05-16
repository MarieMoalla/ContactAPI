using System.Diagnostics;

namespace ContactsApp.Service
{
    public class ContactTraces : IDisposable, IContactTraces
    {
        public ActivitySource activitySource { get; }

        const string activitySourceName = "contact.source";
        const string version = "1.0.0";

        public void Dispose()
        {
            activitySource.Dispose();
        }
        public ActivitySource CreateActivity()
        {
            return new ActivitySource(activitySourceName, version);
        }


        //public ContactTraces() 
        //{
        //    activitySource = new ActivitySource(activitySourceName, version);
        //}
    }
}
