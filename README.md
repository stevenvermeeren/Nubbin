# Nubbin

Nubbin is a C# source generator that creates dummy implementations for partial
classes marked with the `Stub` attribute.

## What It Generates

The generator finds unimplemented members from:

- Implemented interfaces
- Abstract base classes
- Classes that combine both

Generated methods use these return values:

- `void`: an empty method body
- `Task`: `Task.CompletedTask`
- `Task<T>`: a completed task containing the generated default value
- A concrete class with a public parameterless constructor: `new T()`
- Other return types: `default!`
- `out` parameters: `default!`

Existing implementations are preserved. If an abstract base class and an
interface declare the same member, only one implementation is generated.

## Usage

Reference the `Nubbin` project or package from a consumer project. The target
class must be `partial` because source generators add the implementation in a
separate partial declaration.

```csharp
using Nubbin;

public interface IClock
{
    DateTime Now();
}

[Stub]
public partial class TestClock : IClock
{
}
```

The generated `TestClock.Now()` returns `default(DateTime)`.

For a concrete return type with a public parameterless constructor:

```csharp
public sealed class Response
{
}

public interface IClient
{
    Response GetResponse();
    Task<Response> GetResponseAsync();
}

[Stub]
public partial class TestClient : IClient
{
}
```

Both methods return a non-null `Response`; the asynchronous method returns a
completed task.

## Constraints

- `[Stub]` applies to classes only.
- The target class must be declared `partial`.
- Generated implementations are intentionally minimal and do not contain
  production behavior.

## Building and Testing

```text
dotnet build Nubbin.slnx
dotnet test Nubbin.Test/Nubbin.Test.csproj
```

The generator and attribute are currently kept in the main `Nubbin` assembly.

## Continuous Integration and Releases

The GitHub Actions workflow restores, builds, and tests pull requests targeting
`main`. Release Please watches pushes to `main`, parses Conventional Commits,
and opens or updates a release PR with the version and changelog changes. Merge
that PR to create the GitHub release and tag. Every Release Please PR creation
or update also builds and publishes an `-rc.<run-number>` package from the CI
build output. Merging the Release Please PR creates the GitHub release and
automatically publishes the tagged stable package.

Repository setup requires:

- NuGet Trusted Publishing configured for this GitHub repository and both
  workflows. In nuget.org account settings, add GitHub Actions trusted
  publishing policies using this repository's owner and name. Enter only
  `ci.yml` for the workflow file in the RC policy and `publish-release.yml` in
  the stable policy; do not enter the `.github/workflows/` path. Leave the
  environment field empty.
- A `NUGET_USERNAME` repository secret containing the nuget.org profile name
  that owns the package, not an email address.
- A `RELEASE_PLEASE_TOKEN` repository secret containing a PAT or GitHub App
  token that can create release PRs and trigger workflows from the created
  release.

Release Please calculates package versions from Conventional Commit messages:

- `fix:` and other recognized changes increment the patch version.
- `feat:` increments the minor version.
- `!` after the type/scope or a `BREAKING CHANGE:` footer increments the major
  version.

Stable releases are tagged as `v<version>` by Release Please. Merging the
Release Please PR is the approval gate for both the version and NuGet
publication.

## License

Nubbin is available under the MIT License. See [LICENSE](LICENSE).