namespace MR.SAASy.Core.Modules;

internal static class SemanticVersionComparer
{
    public static bool TryCompare(string left, string right, out int comparison)
    {
        comparison = 0;

        if (!ParsedSemanticVersion.TryParse(left, out var leftVersion) ||
            !ParsedSemanticVersion.TryParse(right, out var rightVersion))
        {
            return false;
        }

        comparison = leftVersion.Major.CompareTo(rightVersion.Major);
        if (comparison != 0) return true;

        comparison = leftVersion.Minor.CompareTo(rightVersion.Minor);
        if (comparison != 0) return true;

        comparison = leftVersion.Patch.CompareTo(rightVersion.Patch);
        if (comparison != 0) return true;

        comparison = ComparePreRelease(leftVersion.PreRelease, rightVersion.PreRelease);
        return true;
    }

    private static int ComparePreRelease(IReadOnlyList<string> left, IReadOnlyList<string> right)
    {
        if (left.Count == 0 && right.Count == 0) return 0;
        if (left.Count == 0) return 1;
        if (right.Count == 0) return -1;

        var count = Math.Min(left.Count, right.Count);
        for (var index = 0; index < count; index++)
        {
            var leftPart = left[index];
            var rightPart = right[index];
            var leftIsNumber = int.TryParse(leftPart, out var leftNumber);
            var rightIsNumber = int.TryParse(rightPart, out var rightNumber);

            int comparison;
            if (leftIsNumber && rightIsNumber)
            {
                comparison = leftNumber.CompareTo(rightNumber);
            }
            else if (leftIsNumber)
            {
                comparison = -1;
            }
            else if (rightIsNumber)
            {
                comparison = 1;
            }
            else
            {
                comparison = string.CompareOrdinal(leftPart, rightPart);
            }

            if (comparison != 0) return comparison;
        }

        return left.Count.CompareTo(right.Count);
    }

    private readonly record struct ParsedSemanticVersion(
        int Major,
        int Minor,
        int Patch,
        string[] PreRelease)
    {
        public static bool TryParse(string value, out ParsedSemanticVersion version)
        {
            version = default;
            if (string.IsNullOrWhiteSpace(value)) return false;

            var withoutBuildMetadata = value.Split('+', 2)[0];
            var versionAndPreRelease = withoutBuildMetadata.Split('-', 2);
            var coreParts = versionAndPreRelease[0].Split('.');

            if (coreParts.Length != 3 ||
                !int.TryParse(coreParts[0], out var major) ||
                !int.TryParse(coreParts[1], out var minor) ||
                !int.TryParse(coreParts[2], out var patch) ||
                major < 0 || minor < 0 || patch < 0)
            {
                return false;
            }

            var preRelease = versionAndPreRelease.Length == 2
                ? versionAndPreRelease[1].Split('.')
                : Array.Empty<string>();

            if (preRelease.Any(string.IsNullOrWhiteSpace)) return false;

            version = new ParsedSemanticVersion(major, minor, patch, preRelease);
            return true;
        }
    }
}
