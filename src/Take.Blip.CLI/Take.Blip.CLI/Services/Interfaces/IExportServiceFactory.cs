using System;
using System.Collections.Generic;
using System.Text;

namespace Take.BlipCLI.Services.Interfaces
{
    public interface IExportServiceFactory
    {
        IExportService GetGenericExportInstance();
        INLPModelExportService GetNLPExportInstance();
        IBucketExportService GetBucketExportInstance();
    }
}
