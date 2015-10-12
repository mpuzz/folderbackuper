using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using FolderBackup.CommunicationProtocol;
using System.ServiceModel.Description; 

namespace FolderBackup.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            
            ServiceHost selfHost = new ServiceHost(typeof(Server));

            try
            {
                Console.WriteLine(DatabaseManager.getInstance().getUser("test1", "b444ac06613fc8d63795be9ad0beaf55011936ac"));
                Console.WriteLine(User.authUser("test1", "b444ac06613fc8d63795be9ad0beaf55011936ac").rootDirectory);
                // Step 5 Start the service.
                selfHost.Open();
                Console.WriteLine("The service is ready.");
                Console.WriteLine("Press <ENTER> to terminate service.");
                Console.WriteLine();
                Console.ReadLine();
                
                // Close the ServiceHostBase to shutdown the service.
                selfHost.Close();
            }
            catch (CommunicationException ce)
            {
                Console.WriteLine("An exception occurred: {0}", ce.Message);
                selfHost.Abort();
            }
        }
    }
}
