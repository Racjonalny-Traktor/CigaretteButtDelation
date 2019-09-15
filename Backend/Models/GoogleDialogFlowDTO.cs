namespace microserv.Models
{
    #region Model

    public class GoogleRequest
    {
        public string ResponseId { get; set; }
        public GoogleQueryResult QueryResult { get; set; }
        public GoogleOriginalDetectIntentRequest OriginalDetectIntentRequest { get; set; }
        public string Session { get; set; }
    }

    public class GoogleQueryResult
    {
        public string QueryText { get; set; }
        public string FullfillmentText { get; set; }
    }

    public class GoogleOriginalDetectIntentRequest
    {
        public string Source { get; set; }
        public GooglePayload Payload { get; set; }
    }

    public class GooglePayload
    {
        public GooglePayloadData Data { get; set; }
        public string Source { get; set; }
    }

    public class GooglePayloadData
    {
        public GoogleRecipent Recipent { get; set; }
        public GoogleMessage Message { get; set; }
        public double Timestamp { get; set; }
        public GoogleSender Sender { get; set; }
        public GooglePostback Postback { get; set; }
    }

    public class GooglePostback
    {
        public string Payload { get; set; }
        public GoogleCoords Data { get; set; }
    }

    public class GoogleCoords
    {
        public double Lat { get; set; }
        public double Long { get; set; }
    }

    public class GoogleRecipent
    {
        public string Id { get; set; }
    }

    public class GoogleMessage
    {
        public string Mid { get; set; }
        public GoogleAttachment[] Attachments { get; set; }
    }

    public class GoogleAttachment
    {
        public GoogleAttachmentPayload Payload { get; set; }
        public string Type { get; set; }
    }

    public class GoogleAttachmentPayload
    {
        public string Url { get; set; }
    }

    public class GoogleSender
    {
        public string Id { get; set; }
    }

    #endregion model

}
