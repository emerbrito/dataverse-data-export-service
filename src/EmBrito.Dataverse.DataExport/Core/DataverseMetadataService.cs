using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Metadata.Query;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.DataExport.Core
{
    public class DataverseMetadataService
    {

        ServiceClient client;

        public DataverseMetadataService(ServiceClient serviceClient)
        {
            client = serviceClient ?? throw new ArgumentNullException(nameof(serviceClient));
        }

        public async Task<EntityMetadata> GetMetadata(string logicalName)
        {
            var e = new EntityMetadata();
            var query = new EntityQueryExpression
            {
                Properties = new MetadataPropertiesExpression(
                    "Attributes",
                    "ChangeTrackingEnabled",
                    "LogicalName", 
                    "PrimaryIdAttribute", 
                    "PrimaryNameAttribute", 
                    "SchemaName"),
                Criteria = new MetadataFilterExpression(LogicalOperator.And)
                {
                    Conditions =
                    {
                        new MetadataConditionExpression("LogicalName", MetadataConditionOperator.Equals, logicalName)
                    }
                }
            };

            var req = new RetrieveMetadataChangesRequest
            {
                Query = query
            };

            var resp = await client.ExecuteAsync(req);
            var metadata = ((RetrieveMetadataChangesResponse)resp).EntityMetadata.FirstOrDefault();

            return metadata;
        }

        public async Task EnableChangeTracking(EntityMetadata entityMetadata)
        {
            entityMetadata.ChangeTrackingEnabled = true;

            var req = new UpdateEntityRequest
            {
                Entity = entityMetadata, 
            };

            var resp = await client.ExecuteAsync(req);
        }
    }
}
