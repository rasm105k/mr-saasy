using MR.SAASy.Contracts.Identity;
using MR.SAASy.Core.Identity;
using Xunit;

namespace MR.SAASy.Core.Tests;

public sealed class InMemoryIdentityDirectoryTests
{
    private static readonly IdentityId Alice = new("id_human_alice");

    [Fact]
    public async Task Finds_a_seeded_identity_by_id()
    {
        var directory = new InMemoryIdentityDirectory([Identity(Alice)]);

        var found = await directory.FindAsync(Alice);

        Assert.NotNull(found);
        Assert.Equal(Alice, found.IdentityId);
    }

    [Fact]
    public async Task Returns_null_for_unknown_id()
    {
        var directory = new InMemoryIdentityDirectory([Identity(Alice)]);

        var found = await directory.FindAsync(new IdentityId("id_human_unknown"));

        Assert.Null(found);
    }

    [Fact]
    public async Task Finds_an_identity_by_external_subject()
    {
        var subject = new ExternalIdentitySubject(new IdentityProviderKey("entra"), "sub-123");
        var directory = new InMemoryIdentityDirectory([Identity(Alice, subject)]);

        var found = await directory.FindByExternalSubjectAsync(subject);

        Assert.NotNull(found);
        Assert.Equal(Alice, found.IdentityId);
    }

    [Fact]
    public async Task Returns_null_when_no_identity_has_the_external_subject()
    {
        var directory = new InMemoryIdentityDirectory([Identity(Alice)]);

        var found = await directory.FindByExternalSubjectAsync(
            new ExternalIdentitySubject(new IdentityProviderKey("entra"), "sub-999"));

        Assert.Null(found);
    }

    private static IdentityDescriptor Identity(IdentityId id, params ExternalIdentitySubject[] subjects) =>
        new(id, IdentityKind.Human, "Test Human", IdentityLifecycleState.Active, subjects);
}
