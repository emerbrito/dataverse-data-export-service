﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.DataExport.Core
{
    public class ApplicationOptions
    {
        public string DataverseInstanceUrl { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public int DataverseQueryPageSize { get; set; } = 5000;
        public string StoreConnectionString { get; set; } = string.Empty;
        public int SqlCommandTimeoutSeconds { get; set; } = 600;
        public int RetryLinearBackoffInitialDelaySeconds { get; set; } = 10;
        public int RetryLinearBackoffRetryCount { get; set; } = 6;
        public string ScheduleCronExpression { get; set; } = "*/15 * * * *";
        public string TableNamePrefix { get; set; } = string.Empty;
        public string TableNameSufix { get; set; } = string.Empty;
        public bool EnableIdColumn { get; set; } = true;
    }
}
