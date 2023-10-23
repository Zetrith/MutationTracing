# Mutation tracing
This is a small tool for diagnosing desyncs in [RimWorld Multiplayer](https://github.com/rwmt/Multiplayer). Requires [Prepatcher](https://github.com/Zetrith/Prepatcher).

In short, it instruments assemblies to take stack traces at every field write. These stack traces are then dumped into a text file and can be used to see the context in which fields are mutated.

To install, clone into the Mods folder and build.