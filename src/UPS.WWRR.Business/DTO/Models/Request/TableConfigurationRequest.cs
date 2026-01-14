namespace UPS.WWRR.Business.DTO.Models.Request
{
    /// <summary>
    /// Database Table configuration including target table_name and parent DataLoad id for logging
    /// </summary>
    public class TableConfigurationRequest
    {
        /// <summary>
        /// target table_name to copy data into
        /// </summary>
        public required string TableName { get; init; }

        /// <summary>
        /// Parent DataLoad Id used for inserting DataLoadDetail records
        /// </summary>
        public long DataLoadId { get; init; }
    }
}
