namespace Firebend.AutoCrud.Mongo.Client;

//https://github.com/mongodb/mongo/blob/f7ffb413e2e47d793f6fbdc3220b12a0b15ea7e7/src/mongo/db/error_labels.h#L47
//https://github.com/mongodb/mongo/blob/master/src/mongo/db/error_labels.h
public static class Labels
{
    public const string UnknownTransactionCommitResultLabel = "UnknownTransactionCommitResult";

    public const string TransientTransaction = "TransientTransactionError";
    public const string RetryableWrite = "RetryableWriteError";
    public const string NonResumableChangeStream = "NonResumableChangeStreamError";
    public const string ResumableChangeStream = "ResumableChangeStreamError";
    public const string NoWritesPerformed = "NoWritesPerformed";
    public const string StreamProcessorRetryableError = "StreamProcessorRetryableError";
    public const string StreamProcessorUserError = "StreamProcessorUserError";
}
