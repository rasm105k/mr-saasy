using MR.SAASy.Contracts.Application;

namespace MR.SAASy.Contracts.Help;

public sealed record HelpPrompt(
    HelpTopicKey Topic,
    ApplicationIdentifier ApplicationId,
    string Text);
