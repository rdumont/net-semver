namespace SemanticVersioning
{
    internal static class ReSrc
    {
        // ## Numeric Identifier
        // A single `0`, or a non-zero digit followed by zero or more digits.
        public const string NumericIdentifier = "0|[1-9]\\d*";
        public const string NumericIdentifierLoose = "[0-9]+";

        // ## Non-numeric Identifier
        // Zero or more digits, followed by a letter or a hyphen, and then zero or
        // more letters, digits, or hyphens.
        public const string NonNumericIdentifier = "\\d*[a-zA-Z-][a-zA-Z0-9-]*";

        // ## Main Version
        // Three dot-separated numeric identifiers.
        public const string MainVersion = "(" + NumericIdentifier + ")\\." +
                                          "(" + NumericIdentifier + ")\\." +
                                          "(" + NumericIdentifier + ")";

        public const string MainVersionLoose = "(" + NumericIdentifierLoose + ")\\." +
                                               "(" + NumericIdentifierLoose + ")\\." +
                                               "(" + NumericIdentifierLoose + ")";

        // ## Pre-release Version Identifier
        // A numeric identifier, or a non-numeric identifier.
        public const string PrereleaseIdentifier = "(?:" + NumericIdentifier +
                                                   "|" + NonNumericIdentifier + ")";

        public const string PrereleaseIdentifierLoose = "(?:" + NumericIdentifierLoose +
                                                        "|" + NonNumericIdentifier + ")";

        // ## Pre-release Version
        // Hyphen, followed by one or more dot-separated pre-release version
        // identifiers.
        public const string Prerelease = "(?:-(" + PrereleaseIdentifier +
                                         "(?:\\." + PrereleaseIdentifier + ")*))";

        public const string PrereleaseLoose = "(?:-?(" + PrereleaseIdentifierLoose +
                                              "(?:\\." + PrereleaseIdentifierLoose + ")*))";

        // ## Build Metadata Identifier
        // Any combination of digits, letters, or hyphens.
        public const string BuildIdentifier = "[0-9A-Za-z-]+";

        // ## Build Metadata
        // Plus sign, followed by one or more period-separated build metadata
        // identifiers.
        public const string Build = "(?:\\+(" + BuildIdentifier +
                                    "(?:\\." + BuildIdentifier + ")*))";

        // ## Full Version String
        // A main version, followed optionally by a pre-release version and
        // build metadata.

        // Note that the only major, minor, patch and pre-release section of
        // the version string are capturing groups. The build metadata is not a
        // capturing group, because it should not ever be used in version comparison.
        private const string FullPlain = "v?" + MainVersion +
                                         Prerelease + "?" +
                                         Build + "?";

        public const string Full = "^" + FullPlain + "$";

        // like full, but allows v1.2.3 and =1.2.3, which people do sometimes.
        // also, 1.0.0alpha1 (prerelease without the hyphen) which is pretty
        // common in the npm registry.
        private const string LoosePlain = "[v=\\s]*" + MainVersionLoose +
                                          PrereleaseLoose + "?" +
                                          Build + "?";

        public const string Loose = "^" + LoosePlain + "$";
    }
}