module.exports = {
  branches: ["main"],
  preset: "angular",
  extends: "semantic-release-monorepo",
  plugins: [
    "@semantic-release/commit-analyzer",
    [
      "@semantic-release/release-notes-generator",
      {
        parserOpts: {
          noteKeywords: ["BREAKING CHANGE", "BREAKING CHANGES", "BREAKING"],
        },
        preset: "conventionalcommits",
        presetConfig: {
          types: [
            { type: "build", section: "Build System", hidden: false },
            { type: "chore", section: "Build System", hidden: false },
            { type: "ci", section: "Continuous Integration", hidden: false },
            { type: "docs", section: "Documentation", hidden: true },
            { type: "feat", section: "Features", hidden: true },
            { type: "fix", section: "Bug Fixes", hidden: true },
            { type: "perf", section: "Performance", hidden: false },
            { type: "refactor", section: "Refactoring", hidden: false },
            { type: "style", section: "Styles", hidden: false },
            { type: "test", section: "Tests", hidden: false },
          ],
        },
        writerOpts: {
          commitsSort: ["subject", "scope"],
        },
      },
    ],
    "@semantic-release/changelog",
    [
      "@semantic-release/npm",
      {
        npmPublish: true,
      },
    ],
    [
      "@semantic-release/git",
      {
        message: "chore(release): ${nextRelease.gitTag} [skip ci]",
      },
    ],
    "@semantic-release/github",
  ],
};
