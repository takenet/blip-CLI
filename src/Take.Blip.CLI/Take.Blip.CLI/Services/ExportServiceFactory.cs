using System;
using System.Collections.Generic;
using System.Text;
using Take.BlipCLI.Services.Interfaces;

namespace Take.BlipCLI.Services
{
    public class ExportServiceFactory : IExportServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ExportServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        IBucketExportService IExportServiceFactory.GetBucketExportInstance()
        {
            return (IBucketExportService)_serviceProvider.GetService(typeof(IBucketExportService));
        }

        IExportService IExportServiceFactory.GetGenericExportInstance()
        {
            return (IExportService)_serviceProvider.GetService(typeof(IExportService));
        }

        INLPModelExportService IExportServiceFactory.GetNLPExportInstance()
        {
            return (INLPModelExportService)_serviceProvider.GetService(typeof(INLPModelExportService));
        }
    }
}
