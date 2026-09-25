namespace ClubAL.Domain.IdentityAccess;

public sealed class HumanSession
{
    public static readonly TimeSpan MaximumPlatformLifetime = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan MaximumPlatformIdleTimeout = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan MaximumPlatformMfaFreshness = TimeSpan.FromMinutes(5);

    private HumanSession()
    {
    }

    public HumanSession(
        Guid id,
        Guid userId,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc,
        DateTimeOffset? mfaVerifiedAtUtc = null)
    {
        if (id == Guid.Empty || userId == Guid.Empty)
        {
            throw new ArgumentException("Sessão requer identificadores estáveis.");
        }

        if (expiresAtUtc <= createdAtUtc)
        {
            throw new ArgumentOutOfRangeException(
                nameof(expiresAtUtc),
                "Expiração deve ser posterior à criação.");
        }

        if (mfaVerifiedAtUtc is { } verified
            && (verified < createdAtUtc || verified >= expiresAtUtc))
        {
            throw new ArgumentOutOfRangeException(
                nameof(mfaVerifiedAtUtc),
                "Verificação MFA deve pertencer à sessão humana.");
        }

        Id = id;
        UserId = userId;
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        ExpiresAtUtc = expiresAtUtc.ToUniversalTime();
        MfaVerifiedAtUtc = mfaVerifiedAtUtc?.ToUniversalTime();
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public DateTimeOffset? RevokedAtUtc { get; private set; }

    public DateTimeOffset? MfaVerifiedAtUtc { get; private set; }

    public long Version { get; private set; } = 1;

    public Guid? PlatformWindowId { get; private set; }

    public Guid? PlatformAssignmentId { get; private set; }

    public long? PlatformAssignmentVersion { get; private set; }

    public DateTimeOffset? PlatformEnteredAtUtc { get; private set; }

    public DateTimeOffset? PlatformExpiresAtUtc { get; private set; }

    public DateTimeOffset? PlatformLastActivityAtUtc { get; private set; }

    public DateTimeOffset? PlatformEndedAtUtc { get; private set; }

    public string? PlatformEndReasonCode { get; private set; }

    public Guid? TenantId { get; private set; }

    public Guid? CurrentMembershipId { get; private set; }

    public Guid? CurrentUnitId { get; private set; }

    public Guid? CurrentLocationId { get; private set; }

    public bool IsActiveAt(DateTimeOffset nowUtc) =>
        RevokedAtUtc is null && CreatedAtUtc <= nowUtc && ExpiresAtUtc > nowUtc;

    public void EnterPlatform(
        Guid assignmentId,
        long assignmentVersion,
        DateTimeOffset nowUtc,
        TimeSpan absoluteLifetime,
        TimeSpan idleTimeout,
        TimeSpan mfaFreshness)
    {
        if (assignmentId == Guid.Empty || assignmentVersion < 1)
        {
            throw new ArgumentException("Janela requer atribuição e versão válidas.");
        }

        RequireBoundedDuration(absoluteLifetime, MaximumPlatformLifetime, nameof(absoluteLifetime));
        RequireBoundedDuration(idleTimeout, MaximumPlatformIdleTimeout, nameof(idleTimeout));
        RequireBoundedDuration(mfaFreshness, MaximumPlatformMfaFreshness, nameof(mfaFreshness));
        RequireActiveSession(nowUtc);
        RequireMonotonicPlatformTime(nowUtc);
        if (MfaVerifiedAtUtc is not { } verified
            || verified < CreatedAtUtc
            || verified > nowUtc
            || nowUtc - verified >= mfaFreshness)
        {
            throw new InvalidOperationException("Administração requer MFA recente do servidor.");
        }

        if (IsPlatformWindowActiveAt(nowUtc, idleTimeout))
        {
            if (PlatformAssignmentId != assignmentId
                || PlatformAssignmentVersion != assignmentVersion)
            {
                throw new InvalidOperationException("A janela pertence a outra atribuição ou versão.");
            }

            return;
        }

        TenantId = null;
        CurrentMembershipId = null;
        CurrentUnitId = null;
        CurrentLocationId = null;
        PlatformWindowId = Guid.NewGuid();
        PlatformAssignmentId = assignmentId;
        PlatformAssignmentVersion = assignmentVersion;
        PlatformEnteredAtUtc = nowUtc.ToUniversalTime();
        PlatformExpiresAtUtc = ExpiresAtUtc - nowUtc <= absoluteLifetime
            ? ExpiresAtUtc
            : nowUtc.Add(absoluteLifetime).ToUniversalTime();
        PlatformLastActivityAtUtc = nowUtc.ToUniversalTime();
        PlatformEndedAtUtc = null;
        PlatformEndReasonCode = null;
        Version++;
    }

    public bool IsPlatformWindowActiveAt(DateTimeOffset nowUtc, TimeSpan idleTimeout)
    {
        RequireBoundedDuration(idleTimeout, MaximumPlatformIdleTimeout, nameof(idleTimeout));
        return IsActiveAt(nowUtc)
            && PlatformWindowId is { } windowId && windowId != Guid.Empty
            && PlatformAssignmentId is { } assignmentId && assignmentId != Guid.Empty
            && PlatformAssignmentVersion is > 0
            && PlatformEndedAtUtc is null
            && PlatformEndReasonCode is null
            && PlatformEnteredAtUtc is { } entered && entered <= nowUtc
            && PlatformExpiresAtUtc is { } expires && nowUtc < expires
            && PlatformLastActivityAtUtc is { } activity && activity <= nowUtc
            && nowUtc - activity < idleTimeout
            && TenantId is null && CurrentMembershipId is null
            && CurrentUnitId is null && CurrentLocationId is null;
    }

    public void RecordPlatformActivity(DateTimeOffset nowUtc, TimeSpan idleTimeout)
    {
        if (!IsPlatformWindowActiveAt(nowUtc, idleTimeout))
        {
            throw new InvalidOperationException("Janela de plataforma indisponível.");
        }

        if (PlatformLastActivityAtUtc == nowUtc)
        {
            return;
        }

        PlatformLastActivityAtUtc = nowUtc.ToUniversalTime();
        Version++;
    }

    public void EndPlatformWindow(DateTimeOffset nowUtc, string reasonCode = "exited")
    {
        RequirePlatformEndReason(reasonCode);
        if (PlatformWindowId is null || PlatformEndedAtUtc is not null)
        {
            return;
        }

        RequireMonotonicPlatformTime(nowUtc);
        EndPlatformWindowCore(nowUtc, reasonCode);
        Version++;
    }

    public void SelectContext(
        Guid tenantId,
        Guid membershipId,
        Guid unitId,
        Guid? locationId)
    {
        if (PlatformWindowId is not null)
        {
            throw new InvalidOperationException("Após entrada na plataforma, a troca requer horário do servidor.");
        }

        RequireContextIdentifiers(tenantId, membershipId, unitId, locationId);
        SelectContextCore(tenantId, membershipId, unitId, locationId);
    }

    public void SelectContext(
        Guid tenantId,
        Guid membershipId,
        Guid unitId,
        Guid? locationId,
        DateTimeOffset nowUtc)
    {
        RequireContextIdentifiers(tenantId, membershipId, unitId, locationId);
        RequireActiveSession(nowUtc);
        RequireMonotonicPlatformTime(nowUtc);
        var windowEnded = EndPlatformWindowCore(nowUtc, "context_changed");
        SelectContextCore(tenantId, membershipId, unitId, locationId, windowEnded);
    }

    public void Revoke(DateTimeOffset revokedAtUtc)
    {
        if (RevokedAtUtc is not null)
        {
            return;
        }

        RequireMonotonicPlatformTime(revokedAtUtc);
        EndPlatformWindowCore(revokedAtUtc, "revoked");
        RevokedAtUtc = revokedAtUtc.ToUniversalTime();
        Version++;
    }

    private void SelectContextCore(
        Guid tenantId,
        Guid membershipId,
        Guid unitId,
        Guid? locationId,
        bool windowEnded = false)
    {
        if (!windowEnded && TenantId == tenantId
            && CurrentMembershipId == membershipId
            && CurrentUnitId == unitId && CurrentLocationId == locationId)
        {
            return;
        }

        TenantId = tenantId;
        CurrentMembershipId = membershipId;
        CurrentUnitId = unitId;
        CurrentLocationId = locationId;
        Version++;
    }

    private bool EndPlatformWindowCore(DateTimeOffset nowUtc, string reasonCode)
    {
        if (PlatformWindowId is null || PlatformEndedAtUtc is not null)
        {
            return false;
        }

        PlatformEndedAtUtc = nowUtc.ToUniversalTime();
        PlatformEndReasonCode = reasonCode;
        return true;
    }

    private void RequireActiveSession(DateTimeOffset nowUtc)
    {
        if (!IsActiveAt(nowUtc))
        {
            throw new InvalidOperationException("Sessão humana indisponível.");
        }
    }

    private void RequireMonotonicPlatformTime(DateTimeOffset nowUtc)
    {
        if (nowUtc < CreatedAtUtc || nowUtc < PlatformEnteredAtUtc
            || nowUtc < PlatformLastActivityAtUtc || nowUtc < PlatformEndedAtUtc)
        {
            throw new ArgumentOutOfRangeException(nameof(nowUtc), "Horário não pode retroceder.");
        }
    }

    private static void RequireBoundedDuration(TimeSpan duration, TimeSpan maximum, string parameterName)
    {
        if (duration <= TimeSpan.Zero || duration > maximum)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Prazo fora dos limites de plataforma.");
        }
    }

    private static void RequireContextIdentifiers(Guid tenantId, Guid membershipId, Guid unitId, Guid? locationId)
    {
        if (tenantId == Guid.Empty || membershipId == Guid.Empty
            || unitId == Guid.Empty || locationId == Guid.Empty)
        {
            throw new ArgumentException("Contexto requer identificadores estáveis.");
        }
    }

    private static void RequirePlatformEndReason(string reasonCode)
    {
        if (reasonCode is not ("expired" or "exited" or "context_changed"
            or "revoked" or "assignment_revoked" or "user_disabled"))
        {
            throw new ArgumentOutOfRangeException(nameof(reasonCode), "Motivo de encerramento não registrado.");
        }
    }
}
