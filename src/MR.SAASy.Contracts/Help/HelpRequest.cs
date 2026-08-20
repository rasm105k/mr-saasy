using MR.SAASy.Contracts.Application;
using MR.SAASy.Contracts.Identity;
using MR.SAASy.Contracts.Tenant;

namespace MR.SAASy.Contracts.Help;

public sealed record HelpRequest(
    ApplicationIdentifier ApplicationId,
    HelpTopicKey Topic,
    TenantId? TenantId = null,
    IdentityId? IdentityId = null);
