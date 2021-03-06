using System;
using Ekklesia.Api.Data.Models;
using ServiceStack;
using Ekklesia.Api.ServiceModel;
using Microsoft.Extensions.Logging;
using ServiceStack.OrmLite;

namespace Ekklesia.Api.ServiceInterface
{
    public class MyServices : Service
    {
        private readonly ILogger<MyServices> _logger;

        public MyServices(ILogger<MyServices> logger)
        {
            _logger = logger;
        }
        
        public object Any(Hello request)
        {
            try
            {
                Db.Insert(new ScheduledMessage
                {
                    Description = "Foo"
                }, selectIdentity: true);

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Something bad happened");
                throw;
            }
            return new HelloResponse { Result = $"Hello, {request.Name}!" };
        }
    }
}
