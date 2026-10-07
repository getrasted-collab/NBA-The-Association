# Repository Recovery

## Purpose

Prove that an authorized developer can reconstruct NBA The Association from GitHub without hidden files from the working copy. Always use a separate temporary directory or another machine. Never delete or modify the only working repository for this test.

## Standalone prerequisites

- Git
- .NET SDK 10.0.x
- Network access to `https://github.com/getrasted-collab/NBA-The-Association.git`

No Unity installation or Git LFS content is currently required.

## Safe standalone procedure

From outside the authoritative working repository:

1. Record the expected Git commit SHA.
2. Create a unique temporary parent directory.
3. Clone the GitHub repository into that directory:

   ```powershell
   git clone https://github.com/getrasted-collab/NBA-The-Association.git NBA-The-Association-Recovery
   ```

4. Enter the clone and verify identity/state:

   ```powershell
   git rev-parse HEAD
   git status --short
   ```

   `HEAD` must equal the expected SHA and status output must be empty.

5. Record tool versions:

   ```powershell
   git --version
   dotnet --version
   ```

6. Restore dependencies:

   ```powershell
   dotnet restore NBATheAssociation.slnx
   ```

7. Build every current project:

   ```powershell
   dotnet build NBATheAssociation.slnx --no-restore
   ```

8. Run the complete suite:

   ```powershell
   dotnet test NBATheAssociation.slnx --no-build --no-restore
   ```

9. Smoke-test the CLI using a committed fictional package:

   ```powershell
   dotnet run --project tools/NBATheAssociation.Cli --no-build --no-restore -- validate tests/NBATheAssociation.Tests/Fixtures/v2-cross-era.json
   ```

   Expected final output includes `VALID` and exit code 0.

10. Confirm the solution contains and built:

    - `NBATheAssociation.Core`
    - `NBATheAssociation.Application`
    - `NBATheAssociation.Data`
    - `NBATheAssociation.Cli`
    - `NBATheAssociation.Tests`

11. Confirm Application references only Core:

    ```powershell
    dotnet list src/NBATheAssociation.Application/NBATheAssociation.Application.csproj reference
    ```

12. Check the clone remains clean after ignoring generated output:

    ```powershell
    git status --short
    ```

13. Record results in the current foundation report. Remove only the isolated temporary clone after confirming its resolved path is outside the authoritative repository.

## Failure handling

Do not claim recovery succeeded if any step is unavailable or fails. Record:

- expected and actual commit;
- failed command and exit code;
- tool/SDK version;
- missing network, package, credential, LFS, or external-asset requirement;
- whether the problem comes from documentation, ignored files, environment prerequisites, or repository contents.

Repair the authoritative repository/documentation, push it, and repeat against the repaired commit.

## Future Unity extension

After Unity is explicitly created, extend this test to verify the exact editor revision, package restoration, LFS/external assets, `.meta` integrity, scenes, Unity tests, and a development build. Unity recovery is not complete and is not part of the current standalone test.

## Latest verified result

**Passed on 2026-10-07.**

- Tested GitHub commit: `865f6e7d998717f0f1281f9343ff9e759d0f827f`
- Clone location: a unique directory beneath the Windows user temporary directory, outside the authoritative repository
- Fresh clone status before restore: clean
- Git: `2.55.0.windows.1`
- .NET SDK: `10.0.302`
- Restore: successful for all five projects
- Build: successful, 0 warnings and 0 errors
- Tests: 33 passed, 0 failed, 0 skipped
- CLI validation smoke test: exit code 0 and `VALID`
- Application project references: Core only
- Fresh clone status after restore/build/test: clean; generated output was correctly ignored
- Second restore used `--force --no-cache` with a newly created isolated `NUGET_PACKAGES` directory; build and all 33 tests passed again
- Hidden working-copy dependencies found: none

The standalone process still requires the documented Git/network access and .NET 10 SDK. NuGet dependencies are public package dependencies restored by the standard toolchain, not files from the authoritative working copy.

After recording this result, only the isolated temporary recovery directory was removed. The authoritative repository was not deleted, reset, or modified by the recovery procedure.
