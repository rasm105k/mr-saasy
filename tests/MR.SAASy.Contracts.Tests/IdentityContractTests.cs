using MR.SAASy.Contracts.Identity;
using Xunit;

namespace MR.SAASy.Contracts.Tests;

public sealed class IdentityContractTests
{
    [Fact]
    public void Service_identity_does_not_require_fake_human_email()
    {
        var identity = new IdentityDescriptor(
            new IdentityId("id_service_deployer"),
            IdentityKind.Service,
            "Deployment service",
            IdentityLifecycleState.Active,
            [new ExternalIdentitySubject(new IdentityProviderKey("entra"), "service-object-id")]);

        Assert.Equal(IdentityKind.Service, identity.Kind);
        Assert.Null(identity.Email);
    }

    [Fact]
    public void Automation_is_explicitly_distinct_from_human_and_service_identity()
    {
        var identity = new IdentityDescriptor(
            new IdentityId("id_automation_release-agent"),
            IdentityKind.Automation,
            "Release agent",
            IdentityLifecycleState.Active,
            [new ExternalIdentitySubject(new IdentityProviderKey("github"), "rasm105k/mr-saasy:environment:production")]);

        Assert.Equal(IdentityKind.Automation, identity.Kind);
        Assert.NotEqual(IdentityKind.Human, identity.Kind);
        Assert.NotEqual(IdentityKind.Service, identity.Kind);
    }

    [Fact]
    public void Email_is_mutable_metadata_not_platform_identity()
    {
        var identityId = new IdentityId("id_human_001");
        var before = new IdentityDescriptor(
            identityId,
            IdentityKind.Human,
            "Example Admin",
            IdentityLifecycleState.Active,
            [new ExternalIdentitySubject(new IdentityProviderKey("entra"), "entra-object-id")],
            "old@example.test");
        var after = before with { Email = "new@example.test" };

        Assert.Equal(before.IdentityId, after.IdentityId);
        Assert.NotEqual(before.Email, after.Email);
    }

    [Fact]
    public void External_subject_is_provider_scoped_and_opaque()
    {
        var entra = new ExternalIdentitySubject(new IdentityProviderKey("entra"), "same-value");
        var product = new ExternalIdentitySubject(new IdentityProviderKey("workslip"), "same-value");

        Assert.NotEqual(entra, product);
        Assert.Equal("same-value", entra.SubjectId);
    }
}
