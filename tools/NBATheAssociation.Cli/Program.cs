using NBATheAssociation.Data;

return CliApplication.Run(args, Console.Out, Console.Error);

public static class CliApplication
{
    public static int Run(string[] args, TextWriter output, TextWriter error)
    {
        if (args.Length < 2) { error.WriteLine("Usage: validate|inspect|trace-external|diff <package> [...]"); return 2; }
        try
        {
            return args[0] switch
            {
                "validate" => Validate(args[1], output), "inspect" => Inspect(args[1], output),
                "trace-external" when args.Length == 5 => Trace(args[1], args[2], args[3], args[4], output),
                "diff" when args.Length == 3 => Diff(args[1], args[2], output), _ => Invalid(error)
            };
        }
        catch (IOException ex) { error.WriteLine(ex.Message); return 2; }
    }
    private static V2PackageLoadResult Load(string path) => V2PackageSerializer.Load(File.ReadAllText(path));
    private static int Validate(string path, TextWriter output) { var r = Load(path); foreach (var x in r.Issues) output.WriteLine($"{x.Severity} {x.Code} {x.Path}: {x.Message}"); output.WriteLine(r.IsSuccess ? "VALID" : "INVALID"); return r.IsSuccess ? 0 : 1; }
    private static int Inspect(string path, TextWriter output) { var r = Load(path); if (!r.IsSuccess) return WriteInvalid(r, output); var s = PackageInspector.Inspect(r.Package!); output.WriteLine($"Package: {s.PackageId} {s.PackageVersion} schema {s.SchemaVersion}"); output.WriteLine($"Era: {s.EraLabel ?? "(none)"}"); output.WriteLine($"Seasons: {string.Join(", ", s.Seasons)}"); output.WriteLine($"Sources: {string.Join(", ", s.Sources)}"); output.WriteLine($"Categories: {string.Join(", ", r.Package!.Manifest!.ContainedCategories ?? [])}"); foreach (var c in s.EntityCounts.OrderBy(x => x.Key)) output.WriteLine($"{c.Key}: {c.Value}"); return 0; }
    private static int Trace(string path, string source, string type, string externalId, TextWriter output) { var r = Load(path); if (!r.IsSuccess) return WriteInvalid(r, output); var t = PackageInspector.TraceExternal(r.Package!, source, type, externalId); if (t is null) { output.WriteLine("NOT FOUND"); return 1; } output.WriteLine($"{t.EntityType} {t.InternalId}"); if (t.Provenance is not null) output.WriteLine($"Source: {t.Provenance.SourceName}"); return 0; }
    private static int Diff(string left, string right, TextWriter output) { var l = Load(left); var r = Load(right); if (!l.IsSuccess) return WriteInvalid(l, output); if (!r.IsSuccess) return WriteInvalid(r, output); var d = PackageDiffer.Compare(l.Package!, r.Package!); foreach (var x in d.ManifestChanges) output.WriteLine($"manifest changed: {x}"); foreach (var x in d.AddedIds) foreach (var id in x.Value) output.WriteLine($"added {x.Key}: {id}"); foreach (var x in d.RemovedIds) foreach (var id in x.Value) output.WriteLine($"removed {x.Key}: {id}"); foreach (var x in d.MappingChanges) output.WriteLine($"mapping changed: {x}"); if (!d.HasChanges) output.WriteLine("NO CHANGES"); return 0; }
    private static int WriteInvalid(V2PackageLoadResult r, TextWriter output) { foreach (var x in r.Issues) output.WriteLine($"{x.Code}: {x.Message}"); return 1; }
    private static int Invalid(TextWriter error) { error.WriteLine("Invalid command or arguments."); return 2; }
}
