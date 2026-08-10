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
- Other return types: `default!`
- `out` parameters: `default!`

Reference-type defaults are `null`; value types use their normal default value.
Collection interfaces receive empty compatible collections instead of `null`:

- `IEnumerable<T>`, `IReadOnlyCollection<T>`, and `IReadOnlyList<T>`: an empty array
- `ICollection<T>` and `IList<T>`: an empty `List<T>`
- `ISet<T>`: an empty `HashSet<T>`
- `IDictionary<TKey, TValue>` and `IReadOnlyDictionary<TKey, TValue>`: an empty `Dictionary<TKey, TValue>`
- Non-generic `IEnumerable`, `ICollection`, and `IList`: an empty array or `ArrayList`

Properties are generated with the accessors required by their declaration. Interface
properties always receive both a getter and setter. Abstract properties preserve
their accessor shape, so getter-only and setter-only abstract properties remain
valid overrides. Generated properties use regular auto-implemented properties
whenever possible, with initial values following the same default rules as
method returns. Abstract properties with only one accessor use a typed
`GetPropertyHelper()` extension backed by a weak table, because C# does not allow the
missing accessor to be added to an override. Each stub gets its own generated
`PropertyHelper` storage type without adding a storage property or nested type to the
original stub class. The generated extension container is named
The generated extension methods are placed in a partial static `Nubbin.Stubs`
helper class, avoiding conflicts with members named `Properties` or
`PropertyAccessor`. Each typed property helper is placed in the stub namespace's
`.Nubbin` child namespace.

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

Properties and collection return types are stubbed as follows:

```csharp
public interface IReportClient
{
  IList<string> GetTags();
  string Name { get; set; }
}

[Stub]
public partial class TestReportClient : IReportClient
{
}
```

`GetTags()` returns an empty `List<string>`, and `Name` uses typed weak-table
storage with a `null` initial value. The generated storage can be accessed with
`client.GetPropertyHelper()` when direct inspection or setup is useful.

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