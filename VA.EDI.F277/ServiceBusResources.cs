using System.Configuration;

namespace FSC.ServiceBus.Contracts
{
    public static class ServiceBusResources
    {
        
        public static string statReceipts = ConfigurationManager.AppSettings["ASBQueueName"].ToString();

        public const string Topic = "";
        public const string Subscription = "";
        public const string SecondSubscription = "";
    }
}
//sb://test-fsc.servicebus.windows.net/;SharedAccessKeyName=contractor1;SharedAccessKey=nKjpydmu77b9uWhGdrw7UC0DLSnYLN59WVcPiAS8Jaw=;EntityPath=freelancer&sig=2c5874011bb66cc5afb70e24a26dde51308fce3f528dc075b0a8512af7d5d4a6'