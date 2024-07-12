using System.Linq;

namespace Firebend.AutoCrud.Mongo.Client;

//https://github.com/mongodb/mongo/blob/master/src/mongo/base/error_codes.yml

public static class ErrorCodes
{
    public const int LockTimeout = 24;
    public const int LogWriteFailed = 42;
    public const int NoMatchingDocument = 47;
    public const int MaxTimeMsExpiredErrorCode = 50;
    public const int InvalidDbRef = 55;
    public const int ShardKeyNotFound = 61;
    public const int WriteConcernFailed = 64;
    public const int MultipleErrorsOccurred = 65;
    public const int InvalidNamespace = 73;
    public const int NetworkTimeout = 89;
    public const int ShutdownInProgress = 91;
    public const int DbPathInUse = 98;
    public const int DistributedClockSkewed = 106;
    public const int LockFailed = 107;
    public const int WriteConflict = 112;
    public const int ConflictingOperationInProgress = 117;
    public const int CommandFailed = 125;
    public const int LockNotFound = 128;
    public const int ReadConcernMajorityNotAvailableYet = 134;
    public const int RemoteOplogStale = 138;
    public const int OplogOutOfOrder = 152;
    public const int DurationOverflow = 159;
    public const int TransportSessionClosed = 172;
    public const int TransportSessionNotFound = 173;
    public const int TransportSessionUnknown = 174;
    public const int MasterSlaveConnectionFailure = 190;
    public const int ChunkRangeCleanupPending = 200;
    public const int TimeProofMismatch = 204;
    public const int NoSuchSession = 206;
    public const int TooManyLocks = 208;
    public const int DuplicateSession = 213;
    public const int IncompleteTransactionHistory = 217;
    public const int SessionTransferIncomplete = 228;
    public const int InternalErrorNotSupported = 235;
    public const int CursorKilled = 237;
    public const int NoSuchTransaction = 251;
    public const int TransactionCommitted = 256;
    public const int TransactionTooLarge = 257;
    public const int TooManyLogicalSessions = 261;
    public const int ExceededTimeLimit = 262;
    public const int TooManyFilesOpen = 264;
    public const int ClientDisconnect = 279;
    public const int TransactionCoordinatorSteppingDown = 281;
    public const int TransactionCoordinatorReachedAbortDecision = 282;
    public const int TransactionExceededLifetimeLimitSeconds = 290;
    public const int QueryExceededMemoryLimitNoDiskUseAllowed = 292;
    public const int ObjectIsBusy = 314;
    public const int SkipCommandExecution = 330;
    public const int InterruptedDueToStorageChange = 355;
    public const int InternalTransactionNotSupported = 358;
    public const int TemporarilyUnavailable = 365;
    public const int DuplicateKeyId = 386;
    public const int StreamTerminated = 398;
    public const int StreamProcessorWorkerOutOfMemory = 418;
    public const int StreamProcessorAtlasConnectionError = 421;
    public const int SocketException = 9001;
    public const int DuplicateKey = 11000;
    public const int InterruptedDueToReplStateChange = 11602;
    public const int ClientMarkedKilled = 46841;
    public const int BackupCursorOpenConflictWithCheckpoint = 50915;
    public const int RetriableRemoteCommandFailure = 91331;

    public static readonly int[] All =
    [
        LockTimeout,
        LogWriteFailed,
        NoMatchingDocument,
        MaxTimeMsExpiredErrorCode,
        InvalidDbRef,
        ShardKeyNotFound,
        WriteConcernFailed,
        MultipleErrorsOccurred,
        InvalidNamespace,
        NetworkTimeout,
        ShutdownInProgress,
        DbPathInUse,
        DistributedClockSkewed,
        LockFailed,
        WriteConflict,
        ConflictingOperationInProgress,
        CommandFailed,
        LockNotFound,
        ReadConcernMajorityNotAvailableYet,
        RemoteOplogStale,
        OplogOutOfOrder,
        DurationOverflow,
        TransportSessionClosed,
        TransportSessionNotFound,
        TransportSessionUnknown,
        MasterSlaveConnectionFailure,
        ChunkRangeCleanupPending,
        TimeProofMismatch,
        NoSuchSession,
        TooManyLocks,
        DuplicateSession,
        IncompleteTransactionHistory,
        SessionTransferIncomplete,
        InternalErrorNotSupported,
        CursorKilled,
        NoSuchTransaction,
        TransactionCommitted,
        TransactionTooLarge,
        TooManyLogicalSessions,
        ExceededTimeLimit,
        TooManyFilesOpen,
        ClientDisconnect,
        TransactionCoordinatorSteppingDown,
        TransactionCoordinatorReachedAbortDecision,
        TransactionExceededLifetimeLimitSeconds,
        QueryExceededMemoryLimitNoDiskUseAllowed,
        ObjectIsBusy,
        SkipCommandExecution,
        InterruptedDueToStorageChange,
        InternalTransactionNotSupported,
        TemporarilyUnavailable,
        DuplicateKeyId,
        StreamTerminated,
        StreamProcessorWorkerOutOfMemory,
        StreamProcessorAtlasConnectionError,
        SocketException,
        DuplicateKey,
        InterruptedDueToReplStateChange,
        ClientMarkedKilled,
        BackupCursorOpenConflictWithCheckpoint,
        RetriableRemoteCommandFailure
    ];

    public static readonly int[] DuplicateKeys = [DuplicateKey, DuplicateKeyId];

    public static readonly int[] Retryable = All.Except(DuplicateKeys).ToArray();
}
