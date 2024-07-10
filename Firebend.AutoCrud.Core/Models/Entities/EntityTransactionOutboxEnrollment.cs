using System;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;

namespace Firebend.AutoCrud.Core.Models.Entities;

public record EntityTransactionOutboxEnrollment(
    string TransactionId,
    IEntityTransactionOutboxEnrollment Enrollment,
    Type EntityType);
