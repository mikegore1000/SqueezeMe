#tool "nuget:?package=NUnit.Runners&version=2.6.4"

var target = Argument("target", "Default");

Task("Default")
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


RunTarget(target);