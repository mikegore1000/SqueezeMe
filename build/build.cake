#tool "nuget:?package=NUnit.Runners&version=2.6.4"

var target = Argument("target", "Build");

Task("Build")
	.Does(() => 
{
	NuGetRestore("./../src/SqueezeMe/SqueezeMe.sln");

	MSBuild("./../src/SqueezeMe/SqueezeMe.sln", 
		config => config.SetConfiguration("Release")
	);

	NUnit("./../src/SqueezeMe/SqueezeMe.UnitTests/bin/Release/SqueezeMe.UnitTests.dll");

	NuGetPack("./../src/SqueezeMe/SqueezeMe/SqueezeMe.csproj", new NuGetPackSettings() {
		ArgumentCustomization = args => args.Append("-Prop Configuration=Release")
	});
});

Task("Deploy")
	.IsDependentOn("Build")
	.Does(() => 
{
	var nugetSource = Argument<string>("nugetSource");
	var nugetApiKey = Argument<string>("nugetApiKey");

	var package = GetFiles("./SqueezeMe*.nupkg");

	NuGetPush(package, new NuGetPushSettings {
		Source = nugetSource,
		ApiKey = nugetApiKey
	});
});


RunTarget(target);